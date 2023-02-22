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
using Microsoft.Oryx.Automation.DotNet.Models;
using Microsoft.Oryx.Automation.Models;
using Microsoft.Oryx.Automation.Services;
using Microsoft.Oryx.Automation.Telemetry;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
    public class DotNet
    {
        private readonly IHttpClient httpClient;
        private readonly ILogger logger;
        private readonly IVersionService versionService;
        private readonly IYamlFileReaderService yamlFileReaderService;

        public DotNet(IHttpClient httpClient, ILogger logger, IVersionService versionService, IYamlFileReaderService yamlFileReaderService)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            this.versionService = versionService;
            this.yamlFileReaderService = yamlFileReaderService;
        }

        public async Task RunAsync(string oryxRootPath)
        {
            List<VersionObj> newVersionsObjs = await this.GetNewVersionObjsAsync();
            if (newVersionsObjs.Count > 0)
            {
                string constantsYamlAbsolutePath = Path.Combine(oryxRootPath, "build", Constants.ConstantsYaml);
                List<YamlConstants> yamlConstantsObjs = await this.yamlFileReaderService.ReadConstantsYamlFileAsync(constantsYamlAbsolutePath);
                this.UpdateOryxConstantsForNewVersions(newVersionsObjs, yamlConstantsObjs, oryxRootPath);
            }
        }

        public async Task<List<VersionObj>> GetNewVersionObjsAsync()
        {
            List<VersionObj> versionObjs = new List<VersionObj>();
            string url = Constants.OryxSdkStorageBaseUrl + "/dotnet?restype=container&comp=list&include=metadata";
            HashSet<string> oryxSdkVersions = await this.httpClient.GetOryxSdkVersionsAsync(url);

            // Deserialize release meta data
            var response = await this.httpClient.GetDataAsync(Constants.ReleasesIndexJsonUrl);
            var releaseNotes = JsonConvert.DeserializeObject<ReleaseNotes>(response);
            var releasesIndex = releaseNotes == null ? new List<ReleaseNote>() : releaseNotes.ReleaseIndexes;
            foreach (var releaseIndex in releasesIndex)
            {
                string latestVersion = releaseIndex.LatestSdk;
                if (!this.versionService.IsVersionWithinRange(latestVersion, minVersion: Constants.MinSdkVersion) ||
                    oryxSdkVersions.Contains(latestVersion))
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
                    string sdkVersion = release.Sdk.Version;

                    // Check the version is not already in our storage account
                    if (oryxSdkVersions.Contains(sdkVersion) ||
                        !this.versionService.IsVersionWithinRange(sdkVersion, minVersion: Constants.MinSdkVersion))
                    {
                        continue;
                    }

                    // create sdk version object
                    string sha = this.GetSha(release.Sdk.Files);
                    VersionObj versionObj = new VersionObj
                    {
                        Version = sdkVersion,
                        Sha = sha,
                        VersionType = Constants.SdkName,
                    };
                    versionObjs.Add(versionObj);

                    // create runtime (netcore) version object
                    string runtimeVersion = release.Runtime.Version;
                    if (!this.versionService.IsVersionWithinRange(runtimeVersion, minVersion: Constants.MinRuntimeVersion))
                    {
                        continue;
                    }

                    sha = this.GetSha(release.Runtime.Files);
                    versionObj = new VersionObj
                    {
                        Version = runtimeVersion,
                        Sha = sha,
                        VersionType = Constants.DotNetCoreName,
                    };
                    versionObjs.Add(versionObj);

                    // create runtime (aspnetcore) version object
                    string aspnetCoreRuntimeVersion = release.AspNetCoreRuntime.Version;
                    if (!this.versionService.IsVersionWithinRange(aspnetCoreRuntimeVersion, minVersion: Constants.MinRuntimeVersion))
                    {
                        continue;
                    }

                    sha = this.GetSha(release.AspNetCoreRuntime.Files);
                    versionObj = new VersionObj
                    {
                        Version = aspnetCoreRuntimeVersion,
                        Sha = sha,
                        VersionType = Constants.DotNetAspCoreName,
                    };
                    versionObjs.Add(versionObj);

                    // TODO: add new Major.Minor version string to runtime-version list of runtimes
                    // for the constants.yaml list
                    // Example: https://github.com/microsoft/Oryx/pull/1560/files#diff-47c28d7a6c8135707f46b624b5913e35beea6dfbe7a8be2db7efefde606eba59R47
                }
            }

            return versionObjs = versionObjs.OrderBy(v => v.Version).ToList();
        }

        private void UpdateOryxConstantsForNewVersions(List<VersionObj> versionObjs, List<YamlConstants> yamlConstants, string oryxRootPath)
        {
            Dictionary<string, YamlConstants> dotnetYamlConstants = this.GetYamlDotNetConstants(yamlConstants);

            // update dotnetcore sdks and runtimes
            foreach (var versionObj in versionObjs)
            {
                string version = versionObj.Version;
                string sha = versionObj.Sha;
                string versionType = versionObj.VersionType;
                string dotNetConstantKey = this.GenerateDotNetConstantKey(versionObj);
                Console.WriteLine($"[UpdateConstants] version: {version} versionType: {versionType} sha: {sha} dotNetConstantKey: {dotNetConstantKey}");

                if (versionType.Equals(Constants.SdkName))
                {
                    YamlConstants dotNetYamlConstant = dotnetYamlConstants[Constants.DotNetSdkKey];
                    dotNetYamlConstant.Constants[dotNetConstantKey] = version;

                    // add sdk to versionsToBuild.txt
                    this.UpdateVersionsToBuildTxt(versionObj, oryxRootPath);
                }
                else
                {
                    YamlConstants dotNetYamlConstant = dotnetYamlConstants[Constants.DotNetRuntimeKey];
                    dotNetYamlConstant.Constants[dotNetConstantKey] = version;

                    // store SHAs for net-core and aspnet-core
                    dotNetYamlConstant.Constants[$"{dotNetConstantKey}-sha"] = sha;
                }
            }

            var serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var constantsYamlAbsolutePath = Path.Combine(oryxRootPath, "build", Constants.ConstantsYaml);
            var stringResult = serializer.Serialize(yamlConstants);
            File.WriteAllText(constantsYamlAbsolutePath, stringResult);
        }

        private Dictionary<string, YamlConstants> GetYamlDotNetConstants(List<YamlConstants> yamlContents)
        {
            var dotnetConstants = yamlContents.Where(c => c.Name == Constants.DotNetSdkKey || c.Name == Constants.DotNetRuntimeKey)
                                  .ToDictionary(c => c.Name, c => c);
            return dotnetConstants;
        }

        private string GenerateDotNetConstantKey(VersionObj versionObj)
        {
            string[] splitVersion = versionObj.Version.Split('.');
            string majorVersion = splitVersion[0];
            string minorVersion = splitVersion[1];

            // prevent duplicate majorMinor where 11.0 and 1.10 both will generate a 110 key.
            int majorVersionInt = int.Parse(majorVersion);
            string majorMinor = majorVersionInt < 10 ? $"{majorVersion}{minorVersion}" : $"{majorVersion}Dot{minorVersion}";

            string constant;
            if (versionObj.VersionType.Equals(Constants.SdkName))
            {
                // dotnet/dotnetcore are based on the major version
                string prefix = majorVersionInt < 5 ? $"dot-net-core" : "dot-net";

                constant = $"{prefix}-{majorMinor}-sdk-version";
            }
            else
            {
                constant = $"{versionObj.VersionType}-app-{majorMinor}";
            }

            return constant;
        }

        private string GetSha(List<FileObj> files)
        {
            Regex regEx = new Regex(Constants.DotNetLinuxTarFileRegex);
            foreach (var file in files)
            {
                if (regEx.IsMatch(file.Name))
                {
                    return file.Hash;
                }
            }

            throw new MissingFieldException(message: $"[GetSha] Expected SHA feild is missing in {files}\n" +
                $"Pattern matching using regex: {Constants.DotNetLinuxTarFileRegex}");
        }

        private void UpdateVersionsToBuildTxt(VersionObj platformConstant, string oryxRootPath)
        {
            HashSet<string> debianFlavors = new HashSet<string>() { "bullseye", "buster", "focal-scm", "stretch" };
            foreach (string debianFlavor in debianFlavors)
            {
                var versionsToBuildTxtAbsolutePath = Path.Combine(
                    oryxRootPath, "platforms", Constants.DotNetName, "versions", debianFlavor, Constants.VersionsToBuildTxt);
                string line = $"\n{platformConstant.Version}, {platformConstant.Sha},";
                File.AppendAllText(versionsToBuildTxtAbsolutePath, line);

                // sort
                Console.WriteLine($"[UpdateVersionsToBuildTxt] Updating {versionsToBuildTxtAbsolutePath}...");
                var contents = File.ReadAllLines(versionsToBuildTxtAbsolutePath);
                Array.Sort(contents);
                File.WriteAllLines(versionsToBuildTxtAbsolutePath, contents.Distinct());
            }
        }
    }
}
