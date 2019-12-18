// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Common;
using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command(Name, Description = "Write out a list of detected tools and their versions.")]
    internal class PrepareEnvironmentCommand : CommandBase
    {
        public const string Name = "prepEnv";

        [Argument(0, Description = "The source directory.")]
        [DirectoryExists]
        public string SourceDir { get; set; }

        [Option("--devImage", CommandOptionType.NoValue, Description = "Indicates if the current image is a dev image or not.")]
        public bool IsDevImage { get; set; }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var httpClient = new HttpClient();
            var logger = serviceProvider.GetRequiredService<ILogger<PrepareEnvironmentCommand>>();
            var toolDetector = serviceProvider.GetRequiredService<DefaultToolDetector>();
            var environment = serviceProvider.GetRequiredService<IEnvironment>();

            var sourceRepoProvider = serviceProvider.GetRequiredService<ISourceRepoProvider>();
            var sourceRepo = sourceRepoProvider.GetSourceRepo();

            var tools = toolDetector.DetectTools(sourceRepo);

            foreach (var tool in tools)
            {
                // Examples:
                // >=5.6
                // 8.12
                // 8.12.1
                var requestedVersion = new SemVer.Range(tool.Value);
                var installLatest = environment.GetBoolEnvironmentVariable("INSTALL_LATEST") == null
                    ? false : environment.GetBoolEnvironmentVariable("INSTALL_LATEST").Value;

                var installedVersion = GetInstalledVersion(tool.Key, new SemVer.Range(tool.Value));
                if (installLatest || installedVersion == null)
                {

                }
            }

            return ProcessConstants.ExitSuccess;
        }

        internal override void ConfigureBuildScriptGeneratorOptions(BuildScriptGeneratorOptions options)
        {
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceDir: SourceDir,
                destinationDir: null,
                intermediateDir: null,
                manifestDir: null,
                platform: null,
                platformVersion: null,
                shouldPackage: false,
                requiredOsPackages: null,
                scriptOnly: false,
                properties: null);
        }

        private bool TryGetVersion(string versionString, out SemVer.Version version)
        {
            try
            {
                version = new SemVer.Version(versionString);
                return true;
            }
            catch
            {
                version = null;
                return false;
            }
        }

        private string GetInstalledVersion(string tool, SemVer.Range requestedVersion)
        {
            string installedVersion = null;

            if (IsDevImage)
            {
                switch (tool)
                {
                    case "node":
                        break;
                    case "dotnet":
                        break;
                    case "python":
                        break;
                    case "php":
                        break;
                }
            }
            else
            {
                switch (tool)
                {
                    case "node":
                        installedVersion = GetInstalledNodeVersion(requestedVersion);
                        break;
                    case "dotnet":
                        installedVersion = GetInstalledDotNetCoreVersion(requestedVersion);
                        break;
                    case "python":
                        installedVersion = GetInstalledPythonVersion(requestedVersion);
                        break;
                    case "php":
                        installedVersion = GetInstalledPhpVersion(requestedVersion);
                        break;
                }
            }

            return installedVersion;
        }

        private string GetInstalledNodeVersion(SemVer.Range range)
        {
            var supportedVersions = VersionProviderHelper.GetVersionsFromDirectory("/opt/nodejs");
            return range.MaxSatisfying(supportedVersions);
        }

        private string GetInstalledPythonVersion(SemVer.Range range)
        {
            var supportedVersions = VersionProviderHelper.GetVersionsFromDirectory("/opt/python");
            return range.MaxSatisfying(supportedVersions);
        }

        private string GetInstalledDotNetCoreVersion(SemVer.Range range)
        {
            var supportedVersions = VersionProviderHelper.GetVersionsFromDirectory("/opt/dotnet/sdks");
            return range.MaxSatisfying(supportedVersions);
        }

        private string GetInstalledPhpVersion(SemVer.Range range)
        {
            var supportedVersions = VersionProviderHelper.GetVersionsFromDirectory("/opt/php");
            return range.MaxSatisfying(supportedVersions);
        }

        private async Task<IEnumerable<string>> GetSupportedVersions(string Platform, HttpClient httpClient)
        {
            var supportedVersions = new List<string>();

            if (string.Equals(Platform, "node", StringComparison.OrdinalIgnoreCase))
            {
                var responseContent = await httpClient.GetStringAsync("https://nodejs.org/download/release/index.json");
                var versions = JsonConvert.DeserializeObject<IEnumerable<NodeVersion>>(responseContent);
                return versions.Select(nv => nv.Version.TrimStart('v'));
            }
            else if (string.Equals(Platform, "python", StringComparison.OrdinalIgnoreCase))
            {
                var responseContent = await httpClient.GetStringAsync("https://api.github.com/repos/python/cpython/tags");
                var releaseTags = JsonConvert.DeserializeObject<IEnumerable<GitHubRleaseTag>>(responseContent);
                return releaseTags.Select(nv => nv.Name.TrimStart('v'));
            }
            else if (string.Equals(Platform, "dotnet", StringComparison.OrdinalIgnoreCase))
            {
                var responseContent = await httpClient.GetStringAsync("https://raw.githubusercontent.com/dotnet/core/master/release-notes/releases-index.json");
                var releaseIndex = JsonConvert.DeserializeObject<DotNetCoreReleaseIndex>(responseContent);
                foreach (var release in releaseIndex.RelaseIndex)
                {
                    responseContent = await httpClient.GetStringAsync(release.ReleaseInfoUrl);
                    var allReleasesForThisSdk = JsonConvert.DeserializeObject<SdkReleases>(responseContent);
                    foreach (var releaseInfo in allReleasesForThisSdk.Releases)
                    {
                        if (releaseInfo.Sdks != null)
                        {
                            supportedVersions.AddRange(releaseInfo.Sdks.Select(sdk => sdk.Version));
                        }
                    }
                }
            }
            else if (string.Equals(Platform, "php", StringComparison.OrdinalIgnoreCase))
            {
                var versions = new[] { "5", "7" };
                foreach (var version in versions)
                {
                    var responseContent = await httpClient.GetStringAsync($"https://secure.php.net/releases/index.php?json&version={version}&max=100");
                    var releaseTags = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                    supportedVersions.AddRange(releaseTags.Keys);
                }
            }
            else
            {
                throw new InvalidOperationException("Unknown platform " + Platform);
            }

            return supportedVersions;
        }

        private bool TryGetRange(string suppliedRange, out SemVer.Range range)
        {
            range = null;

            try
            {
                range = new SemVer.Range(suppliedRange);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public class NodeVersion
        {
            public string Version { get; set; }

            public string Npm { get; set; }
        }

        public class GitHubRleaseTag
        {
            public string Name { get; set; }
        }

        public class DotNetCoreReleaseIndex
        {
            [JsonProperty("releases-index")]
            public IEnumerable<DotNetCoreRelease> RelaseIndex { get; set; }
        }

        public class DotNetCoreRelease
        {
            [JsonProperty("latest-sdk")]
            public string SdkVersion { get; set; }

            [JsonProperty("releases.json")]
            public string ReleaseInfoUrl { get; set; }
        }

        public class SdkReleases
        {
            public IEnumerable<ReleaseInfo> Releases { get; set; }
        }

        public class ReleaseInfo
        {
            [JsonProperty("sdks")]
            public IEnumerable<DotNetCoreSdk> Sdks { get; set; }
        }

        public class DotNetCoreSdk
        {
            public string Version { get; set; }
        }
    }
}
