// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Oryx.Automation.DotNet.Models;
using Microsoft.Oryx.Automation.Extensions;
using Microsoft.Oryx.Automation.Models;
using Microsoft.Oryx.Automation.Services;
using Newtonsoft.Json;

namespace Microsoft.Oryx.Automation.DotNet
{
    /// <Summary>
    /// This class is reponsible for encapsulating logic for automating SDK and runtime releases for DotNet.
    /// This includes:
    ///     - Getting new release version and sha
    ///     - Updating constants.yaml with version and sha
    ///         - This is important so build/generateConstants.sh
    ///           can be invoked to distribute updated version
    ///           throughout Oryx source code. Which updates
    ///           Oryx tests.
    ///     - Updating versionsToBuild.txt
    /// </Summary>
    public class DotNetAutomator : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly IVersionService versionService;
        private readonly IFileService fileService;
        private readonly IYamlFileService yamlFileService;
        private string oryxRootPath;
        private string oryxSdkStorageBaseUrl;
        private string dotNetMinReleaseVersion;
        private string dotNetMaxReleaseVersion;
        private List<string> dotNetBlockedVersions;
        private HashSet<string> oryxDotNetSdkVersions;

        public DotNetAutomator(
            IHttpClientFactory httpClientFactory,
            IVersionService versionService,
            IFileService fileService,
            IYamlFileService yamlFileReaderService)
        {
            this.httpClient = httpClientFactory.CreateClient();
            this.versionService = versionService;
            this.fileService = fileService;
            this.yamlFileService = yamlFileReaderService;
        }

        public async Task RunAsync(string oryxRootPath)
        {
            this.oryxRootPath = oryxRootPath;
            this.oryxSdkStorageBaseUrl = Environment.GetEnvironmentVariable(Constants.OryxSdkStorageBaseUrlEnvVar);
            if (string.IsNullOrEmpty(this.oryxSdkStorageBaseUrl))
            {
                this.oryxSdkStorageBaseUrl = Constants.OryxSdkStorageBaseUrl;
            }

            this.oryxDotNetSdkVersions = await this.httpClient.GetOryxSdkVersionsAsync(
                Constants.OryxSdkStorageBaseUrl + DotNetConstants.DotNetSuffixUrl);
            this.dotNetMinReleaseVersion = Environment.GetEnvironmentVariable(DotNetConstants.DotNetMinReleaseVersionEnvVar);
            this.dotNetMaxReleaseVersion = Environment.GetEnvironmentVariable(DotNetConstants.DotNetMaxReleaseVersionEnvVar);
            var blockedVersions = Environment.GetEnvironmentVariable(
                DotNetConstants.DotNetBlockedVersionsEnvVar);
            if (!string.IsNullOrEmpty(blockedVersions))
            {
                var versionStrings = blockedVersions.Split(',');
                foreach (var versionString in versionStrings)
                {
                    this.dotNetBlockedVersions.Add(versionString.Trim());
                }
            }

            List<DotNetVersion> newDotNetVersions = await this.GetNewDotNetVersionsAsync();
            if (newDotNetVersions.Count > 0)
            {
                string constantsYamlAbsolutePath = Path.Combine(this.oryxRootPath, "build", Constants.ConstantsYaml);
                List<ConstantsYamlFile> yamlConstantsObjs = await this.yamlFileService.ReadConstantsYamlFileAsync(constantsYamlAbsolutePath);
                this.UpdateOryxConstantsForNewVersions(newDotNetVersions, yamlConstantsObjs);
            }
        }

        public async Task<List<DotNetVersion>> GetNewDotNetVersionsAsync()
        {
            List<DotNetVersion> dotNetVersions = new List<DotNetVersion>();

            // Deserialize release metadata
            var response = await this.httpClient.GetDataAsync(DotNetConstants.ReleasesIndexJsonUrl);
            var releaseNotes = response == null ? null : JsonConvert.DeserializeObject<ReleaseNotes>(response);
            var releasesIndex = releaseNotes == null ? new List<ReleaseNote>() : releaseNotes.ReleaseIndexes;
            foreach (var releaseIndex in releasesIndex)
            {
                // If the latest release version is not within the acceptable range (inclusive),
                // or if the sdk version is present in the set of ORYX SDK versions,
                // skip to the next version in the loop.
                if (this.ReleaseVersionIsNotInRangeOrSdkVersionAlreadyExists(
                    releaseIndex.LatestRelease, releaseIndex.LatestSdk))
                {
                    continue;
                }

                // Get the actual release information from releases.json
                string releasesJsonUrl = releaseIndex.ReleasesJsonUrl;
                response = await this.httpClient.GetDataAsync(releasesJsonUrl);
                var releasesJson = JsonConvert.DeserializeObject<ReleasesJson>(response);
                var releases = releasesJson == null ? new List<Release>() : releasesJson.Releases;
                foreach (var release in releases)
                {
                    // Check again since the "release" is separate from "releaseIndex".
                    if (this.ReleaseVersionIsNotInRangeOrSdkVersionAlreadyExists(
                        release.ReleaseVersion, release.Sdk.Version))
                    {
                        continue;
                    }

                    var dotNetSdkVersion = this.GetDotNetVersion(release, DotNetConstants.SdkName);
                    dotNetVersions.Add(dotNetSdkVersion);

                    // Runtime (netcore)
                    var dotNetCoreVersion = this.GetDotNetVersion(release, DotNetConstants.DotNetCoreName);
                    dotNetVersions.Add(dotNetCoreVersion);

                    // Runtime (aspnetcore)
                    var dotNetAspCoreVersion = this.GetDotNetVersion(release, DotNetConstants.DotNetAspCoreName);
                    dotNetVersions.Add(dotNetAspCoreVersion);
                }
            }

            return dotNetVersions = dotNetVersions.OrderBy(v => v.Version).ToList();
        }

        public void Dispose()
        {
            this.httpClient.Dispose();
        }

        private bool ReleaseVersionIsNotInRangeOrSdkVersionAlreadyExists(string releaseVersion, string sdkVersion)
        {
            if (!this.versionService.IsVersionWithinRange(
                releaseVersion,
                minVersion: this.dotNetMinReleaseVersion,
                maxVersion: this.dotNetMaxReleaseVersion))
            {
                return true;
            }

            if (this.oryxDotNetSdkVersions.Contains(sdkVersion))
            {
                return true;
            }

            return false;
        }

        private DotNetVersion GetDotNetVersion(Release release, string versionType)
        {
            string version;
            string sha;
            switch (versionType)
            {
                case DotNetConstants.SdkName:
                    sha = this.GetSha(release.Sdk.Files);
                    version = release.Sdk.Version;
                    break;
                case DotNetConstants.DotNetCoreName:
                    sha = this.GetSha(release.Runtime.Files);
                    version = release.Runtime.Version;
                    break;
                case DotNetConstants.DotNetAspCoreName:
                    sha = this.GetSha(release.AspNetCoreRuntime.Files);
                    version = release.AspNetCoreRuntime.Version;
                    break;
                default:
                    throw new InvalidDataException($"Invalid DotNet version type: {versionType}");
            }

            return new DotNetVersion
            {
                Version = version,
                Sha = sha,
                VersionType = versionType,
            };
        }

        private void UpdateOryxConstantsForNewVersions(List<DotNetVersion> dotNetVersions, List<ConstantsYamlFile> yamlConstants)
        {
            Dictionary<string, ConstantsYamlFile> dotnetYamlConstants = this.GetYamlDotNetConstants(yamlConstants);

            // update dotnetcore sdks and runtimes
            foreach (var dotNetVersion in dotNetVersions)
            {
                string version = dotNetVersion.Version;
                string sha = dotNetVersion.Sha;
                string versionType = dotNetVersion.VersionType;
                string dotNetConstantKey = this.GenerateDotNetConstantKey(dotNetVersion);
                Console.WriteLine($"[UpdateConstants] version: {version} versionType: {versionType} sha: {sha} dotNetConstantKey: {dotNetConstantKey}");

                if (versionType.Equals(DotNetConstants.SdkName))
                {
                    ConstantsYamlFile dotNetYamlConstant = dotnetYamlConstants[DotNetConstants.DotNetSdkKey];
                    dotNetYamlConstant.Constants[dotNetConstantKey] = version;

                    // update versionsToBuild.txt
                    this.fileService.UpdateVersionsToBuildTxt(
                        DotNetConstants.DotNetName,
                        $"\n{dotNetVersion.Version}, {dotNetVersion.Sha},");
                }
                else
                {
                    ConstantsYamlFile dotNetYamlConstant = dotnetYamlConstants[DotNetConstants.DotNetRuntimeKey];
                    dotNetYamlConstant.Constants[dotNetConstantKey] = version;

                    // store SHAs for net-core and aspnet-core
                    dotNetYamlConstant.Constants[$"{dotNetConstantKey}-sha"] = sha;
                }
            }

            var constantsYamlAbsolutePath = Path.Combine(this.oryxRootPath, "build", Constants.ConstantsYaml);
            this.yamlFileService.WriteConstantsYamlFile(constantsYamlAbsolutePath, yamlConstants);
        }

        private Dictionary<string, ConstantsYamlFile> GetYamlDotNetConstants(List<ConstantsYamlFile> yamlContents)
        {
            var dotnetConstants = yamlContents.Where(c => c.Name == DotNetConstants.DotNetSdkKey || c.Name == DotNetConstants.DotNetRuntimeKey)
                                  .ToDictionary(c => c.Name, c => c);
            return dotnetConstants;
        }

        private string GenerateDotNetConstantKey(DotNetVersion dotNetVersion)
        {
            string[] splitVersion = dotNetVersion.Version.Split('.');
            string majorVersion = splitVersion[0];
            string minorVersion = splitVersion[1];

            // prevent duplicate majorMinor where 11.0 and 1.10 both will generate a 110 key.
            int majorVersionInt = int.Parse(majorVersion);
            string majorMinor = majorVersionInt < 10 ? $"{majorVersion}{minorVersion}" : $"{majorVersion}Dot{minorVersion}";

            string constant;
            if (dotNetVersion.VersionType.Equals(DotNetConstants.SdkName))
            {
                // dotnet/dotnetcore are based on the major version
                string prefix = majorVersionInt < 5 ? $"dot-net-core" : "dot-net";

                constant = $"{prefix}-{majorMinor}-sdk-version";
            }
            else
            {
                constant = $"{dotNetVersion.VersionType}-app-{majorMinor}";
            }

            return constant;
        }

        private string GetSha(List<Models.File> files)
        {
            Regex regEx = new Regex(DotNetConstants.DotNetLinuxTarFileRegex);
            foreach (var file in files)
            {
                if (regEx.IsMatch(file.Name))
                {
                    return file.Hash;
                }
            }

            throw new MissingFieldException(message: $"[GetSha] Expected SHA feild is missing in {files}\n" +
                $"Pattern matching using regex: {DotNetConstants.DotNetLinuxTarFileRegex}");
        }

        /*private void UpdateVersionsToBuildTxt(DotNetVersion platformConstant)
        {
            HashSet<string> debianFlavors = new HashSet<string>() { "bullseye", "buster", "focal-scm", "stretch" };
            foreach (string debianFlavor in debianFlavors)
            {
                var versionsToBuildTxtAbsolutePath = Path.Combine(
                    this.oryxRootPath,
                    "platforms",
                    DotNetConstants.DotNetName,
                    "versions",
                    debianFlavor,
                    Constants.VersionsToBuildTxtFileName);
                string line = $"\n{platformConstant.Version}, {platformConstant.Sha},";
                System.IO.File.AppendAllText(versionsToBuildTxtAbsolutePath, line);

                // sort
                Console.WriteLine($"[UpdateVersionsToBuildTxt] Updating {versionsToBuildTxtAbsolutePath}...");
                var contents = System.IO.File.ReadAllLines(versionsToBuildTxtAbsolutePath);
                Array.Sort(contents);
                System.IO.File.WriteAllLines(versionsToBuildTxtAbsolutePath, contents.Distinct());
            }
        }*/
    }
}
