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
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.Oryx.Automation
{
    /// <Summary>
    ///
    /// TODO:
    ///     - Replace Console.WriteLine with Logging
    ///     - Add unit tests
    ///
    /// This class is reponsible for encapsulating logic for automating SDK releases for DotNet.
    /// This includes:
    ///     - Getting new release version and sha
    ///     - Updating constants.yaml with version and sha
    ///         - This is important so build/generateConstants.sh
    ///           can be invoked to distribute updated version
    ///           throughout Oryx source code. Which updates
    ///           Oryx tests.
    ///     - Updating versionsToBuild.txt
    /// </Summary>
    public class DotNet : Program
    {
        private string repoAbsolutePath = string.Empty;
        private HashSet<string> prodSdkVersions = new HashSet<string>();

        public DotNet(string repoAbsolutePath, HashSet<string> prodSdkVersions)
        {
            this.repoAbsolutePath = repoAbsolutePath;
            this.prodSdkVersions = prodSdkVersions;
        }

        /// <Summary>
        /// Gets DotNet's new release version and sha.
        ///
        /// This is accomplished by:
        ///     - Checking release meta data url if there's a new release
        ///     - If new release, then store release information into PlatformConstants
        ///        Otherwise don't store anything
        /// </Summary>
        /// <param name="dateTarget">yyyy-mm-dd format string that defaults to today's date</param>
        /// <returns>PlatformConstants used later to update constants.yaml</returns>
        public override async Task<List<PlatformConstant>> GetPlatformConstantsAsync(string dateTarget)
        {
            // check dotnet releases' meta data
            var response = await HttpClientHelper.GetRequestStringAsync(Constants.DotNetReleasesMetaDataUrl);
            var releaseNotes = JsonConvert.DeserializeObject<ReleaseNotes>(response);

            // releaseIndex contains release meta data
            var releasesIndex = releaseNotes == null ? new List<ReleaseNote>() : releaseNotes.ReleasesIndex;
            List<PlatformConstant> platformConstants = new List<PlatformConstant>();
            foreach (var releaseIndex in releasesIndex)
            {
                // TODO: check if SDK already exists in storage account
                var dateReleased = releaseIndex.LatestReleaseDate;
                if (!DatesMatch(dateTarget, dateReleased) || this.prodSdkVersions.Contains(releaseIndex.LatestSdk))
                {
                    continue;
                }

                // get actual release information and store into PlatformConstants
                string releasesJsonUrl = releaseIndex.ReleasesJsonUrl;
                response = await HttpClientHelper.GetRequestStringAsync(releasesJsonUrl);
                var releasesJson = JsonConvert.DeserializeObject<ReleasesJson>(response);
                var releases = releasesJson == null ? new List<Release>() : releasesJson.Releases;
                foreach (var release in releases)
                {
                    // check releasedToday again since there
                    // are still releases from other dates.
                    string sdkVersion = release.Sdk.Version;
                    if (!DatesMatch(dateTarget, release.ReleaseDate) || this.prodSdkVersions.Contains(sdkVersion))
                    {
                        continue;
                    }

                    // create sdk PlatformConstant
                    string sha = GetSha(release.Sdk.Files);
                    PlatformConstant platformConstant = new PlatformConstant
                    {
                        Version = sdkVersion,
                        Sha = sha,
                        PlatformName = Constants.DotNetName,
                        VersionType = Constants.SdkName,
                    };
                    platformConstants.Add(platformConstant);

                    // create runtime (netcore) PlatfromConstant
                    string runtimeVersion = release.Runtime.Version;
                    sha = GetSha(release.Runtime.Files);
                    platformConstant = new PlatformConstant
                    {
                        Version = runtimeVersion,
                        Sha = sha,
                        PlatformName = Constants.DotNetName,
                        VersionType = Constants.DotNetCoreName,
                    };
                    platformConstants.Add(platformConstant);

                    // create runtime (aspnetcore) PlatformConstant
                    string aspnetCoreRuntimeVersion = release.AspnetCoreRuntime.Version;
                    sha = GetSha(release.AspnetCoreRuntime.Files);
                    platformConstant = new PlatformConstant
                    {
                        Version = aspnetCoreRuntimeVersion,
                        Sha = sha,
                        PlatformName = Constants.DotNetName,
                        VersionType = Constants.DotNetAspCoreName,
                    };
                    platformConstants.Add(platformConstant);

                    // TODO: add new Major.Minor version string to runtime-version list of runtimes
                    // for the constants.yaml list
                    // Example: https://github.com/microsoft/Oryx/pull/1560/files#diff-47c28d7a6c8135707f46b624b5913e35beea6dfbe7a8be2db7efefde606eba59R47
                }
            }

            return platformConstants;
        }

        /// <inheritdoc/>
        public override void UpdateConstants(List<PlatformConstant> platformConstants, List<Constant> yamlConstants)
        {
            Dictionary<string, Constant> dotnetYamlConstants = GetYamlDotNetConstants(yamlConstants);

            // update dotnetcore sdks and runtimes
            foreach (var platformConstant in platformConstants)
            {
                string version = platformConstant.Version;
                string sha = platformConstant.Sha;
                string versionType = platformConstant.VersionType;
                string dotNetConstantKey = GenerateDotNetConstantKey(platformConstant);
                Console.WriteLine($"[UpdateConstants] version: {version} versionType: {versionType} sha: {sha} dotNetConstantKey: {dotNetConstantKey}");
                if (versionType.Equals(Constants.SdkName))
                {
                    Constant dotNetYamlConstant = dotnetYamlConstants[Constants.DotNetSdkKey];
                    dotNetYamlConstant.Constants[dotNetConstantKey] = version;

                    // add sdk to versionsToBuild.txt
                    this.UpdateVersionsToBuildTxt(platformConstant);
                }
                else
                {
                    Constant dotNetYamlConstant = dotnetYamlConstants[Constants.DotNetRuntimeKey];
                    dotNetYamlConstant.Constants[dotNetConstantKey] = version;

                    // store SHAs for net-core and aspnet-core
                    dotNetYamlConstant.Constants[$"{dotNetConstantKey}-sha"] = sha;
                }
            }

            var serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var constantsYamlAbsolutePath = Path.Combine(this.repoAbsolutePath, "build", Constants.ConstantsYaml);
            var stringResult = serializer.Serialize(yamlConstants);
            File.WriteAllText(constantsYamlAbsolutePath, stringResult);
        }

        private static Dictionary<string, Constant> GetYamlDotNetConstants(List<Constant> yamlContents)
        {
            var dotnetConstants = yamlContents.Where(c => c.Name == Constants.DotNetSdkKey || c.Name == Constants.DotNetRuntimeKey)
                                  .ToDictionary(c => c.Name, c => c);
            return dotnetConstants;
        }

        private static string GenerateDotNetConstantKey(PlatformConstant platformConstant)
        {
            string[] splitVersion = platformConstant.Version.Split('.');
            string majorVersion = splitVersion[0];
            string minorVersion = splitVersion[1];

            // prevent duplicate majorMinor where 11.0 and 1.10 both will generate a 110 key.
            int majorVersionInt = int.Parse(majorVersion);
            string majorMinor = majorVersionInt < 10 ? $"{majorVersion}{minorVersion}" : $"{majorVersion}Dot{minorVersion}";

            string constant;
            if (platformConstant.VersionType.Equals(Constants.SdkName))
            {
                // dotnet/dotnetcore are based on the major version
                string prefix = majorVersionInt < 5 ? $"dot-net-core" : "dot-net";

                constant = $"{prefix}-{majorMinor}-sdk-version";
            }
            else
            {
                constant = $"{platformConstant.VersionType}-app-{majorMinor}";
            }

            return constant;
        }

        private static string GetSha(List<FileObj> files)
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

        private void UpdateVersionsToBuildTxt(PlatformConstant platformConstant)
        {
            HashSet<string> debianFlavors = new HashSet<string>() { "bullseye", "buster", "focal-scm", "stretch" };
            foreach (string debianFlavor in debianFlavors)
            {
                var versionsToBuildTxtAbsolutePath = Path.Combine(
                    this.repoAbsolutePath, "platforms", Constants.DotNetName, "versions", debianFlavor, Constants.VersionsToBuildTxt);
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