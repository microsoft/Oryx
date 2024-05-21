// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Oryx.Automation.Commons;
using Microsoft.Oryx.Automation.DotNet.Models;
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
    public class DotNetAutomator
    {
        private readonly IHttpService httpService;
        private readonly IVersionService versionService;
        private readonly IFileService fileService;
        private readonly IYamlFileService yamlFileService;
        private string dotNetMinReleaseVersion;
        private string dotNetMaxReleaseVersion;
        private List<string> dotNetBlockedVersions = new List<string>();
        private HashSet<string> oryxDotNetSdkVersions;

        public DotNetAutomator(
            IHttpService httpService,
            IVersionService versionService,
            IFileService fileService,
            IYamlFileService yamlFileReaderService)
        {
            this.httpService = httpService;
            this.versionService = versionService;
            this.fileService = fileService;
            this.yamlFileService = yamlFileReaderService;
        }

        public async Task RunAsync()
        {
            await this.InitializeFieldsAsync();

            List<DotNetVersion> newDotNetVersions = await this.GetNewDotNetVersionsAsync();
            if (newDotNetVersions.Count > 0)
            {
                string constantsYamlSubPath = Path.Combine("build", Constants.ConstantsYaml);
                List<ConstantsYamlFile> yamlConstantsObjs = await this.yamlFileService.ReadConstantsYamlFileAsync(constantsYamlSubPath);
                this.UpdateOryxConstantsForNewVersions(newDotNetVersions, yamlConstantsObjs);
            }
        }

        public async Task InitializeFieldsAsync()
        {
            string oryxSdkStorageBaseUrl = Environment.GetEnvironmentVariable(Constants.OryxSdkStorageBaseUrlEnvVar);
            string sdkVersionsUrl = SdkStorageHelper.GetSdkStorageUrl(oryxSdkStorageBaseUrl, DotNetConstants.DotNetSuffixUrl);
            this.oryxDotNetSdkVersions = await this.httpService.GetOryxSdkVersionsAsync(sdkVersionsUrl);
            this.dotNetMinReleaseVersion = Environment.GetEnvironmentVariable(DotNetConstants.DotNetMinReleaseVersionEnvVar);
            this.dotNetMaxReleaseVersion = Environment.GetEnvironmentVariable(DotNetConstants.DotNetMaxReleaseVersionEnvVar);
            string blockedVersions = Environment.GetEnvironmentVariable(DotNetConstants.DotNetBlockedVersionsEnvVar);
            this.dotNetBlockedVersions = SdkStorageHelper.ExtractBlockedVersions(blockedVersions);
        }

        /// <summary>
        /// Retrieves a list of new .NET versions from the .NET release website,
        /// and filters the list based on specified criteria.
        /// The resulting list includes SDK versions, .NET Core runtime versions,
        /// and ASP.NET Core runtime versions, and is sorted by semantic version number.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.
        /// The task result contains a list of DotNetVersion objects representing the new
        /// .NET versions that meet the specified criteria.</returns>
        public async Task<List<DotNetVersion>> GetNewDotNetVersionsAsync()
        {
            List<DotNetVersion> dotNetVersions = new List<DotNetVersion>();
            foreach (var releaseIndex in await this.GetReleasesIndexAsync())
            {
                // If the latest release version is not within the acceptable range (inclusive),
                // or if the sdk version is present in the set of ORYX SDK versions,
                // skip to the next version in the loop.
                if (this.ReleaseVersionIsNotInRangeOrSdkVersionAlreadyExists(
                    releaseIndex.LatestRelease, releaseIndex.LatestSdk))
                {
                    continue;
                }

                foreach (var release in await this.GetReleasesAsync(releaseIndex.ReleasesJsonUrl))
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

            return dotNetVersions.Order().ToList();
        }

        private async Task<List<ReleaseNote>> GetReleasesIndexAsync()
        {
            try
            {
                // Deserialize release metadata
                var response = await this.httpService.GetDataAsync(DotNetConstants.ReleasesIndexJsonUrl);
                var releaseNotes = JsonConvert.DeserializeObject<ReleaseNotes>(response);
                return releaseNotes.ReleaseIndexes;
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Failed to deserialize release notes: {ex}");
            }
        }

        private async Task<List<Release>> GetReleasesAsync(string releaseUrl)
        {
            try
            {
                var response = await this.httpService.GetDataAsync(releaseUrl);
                if (string.IsNullOrEmpty(response))
                {
                    // If the response is empty, return an empty list
                    throw new ArgumentNullException($"An empty response was returned by: {releaseUrl}");
                }

                var releasesJson = JsonConvert.DeserializeObject<ReleasesJson>(response);
                if (releasesJson == null || releasesJson.Releases == null)
                {
                    // If deserialization fails or the releases list is null, throw exception
                    // since this should never be empty
                    throw new ArgumentNullException($"An empty response was returned by: {releaseUrl}");
                }

                return releasesJson.Releases;
            }
            catch (JsonException ex)
            {
                var errorMessage = $"Error deserializing JSON response for release url {releaseUrl}: {ex.Message}";
                throw new JsonException(errorMessage, ex);
            }
        }

        private bool ReleaseVersionIsNotInRangeOrSdkVersionAlreadyExists(string releaseVersion, string sdkVersion)
        {
            if (!this.versionService.IsVersionWithinRange(
                releaseVersion,
                minVersion: this.dotNetMinReleaseVersion,
                maxVersion: this.dotNetMaxReleaseVersion,
                blockedVersions: this.dotNetBlockedVersions))
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

            var constantsYamlSubPath = Path.Combine("build", Constants.ConstantsYaml);
            this.yamlFileService.WriteConstantsYamlFile(constantsYamlSubPath, yamlConstants);
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
            string majorMinor = majorVersionInt < 10 ? $"{majorVersion}{minorVersion}" : $"{majorVersion}_{minorVersion}";

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
    }
}
