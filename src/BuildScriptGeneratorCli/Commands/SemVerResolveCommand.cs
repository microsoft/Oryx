// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    [Command(
        Name,
        Description = "Gets the maximum satisfying version for a given range and list of versions",
        ThrowOnUnexpectedArgument = false,
        AllowArgumentSeparator = true)]
    internal class SemVerResolveCommand : CommandBase
    {
        public const string Name = "resolveVersion";
        private HttpClient httpClient;

        [Argument(0, Description = "Version range.")]
        public string Range { get; set; }

        [Option("--platform", CommandOptionType.SingleValue, Description = "Platform for which to find the supporting version")]
        public string Platform { get; set; }

        [Option("--versions", CommandOptionType.SingleValue, Description = "Comma separated list of supported versions")]
        public string SupportedVersions { get; set; }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<SemVerResolveCommand>>();

            IEnumerable<string> supportedVersions;
            if (string.IsNullOrEmpty(SupportedVersions))
            {
                httpClient = new HttpClient();
                // NOTE: Setting user agent is required to avoid receiving 403 Forbidden response.
                httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("oryx", "1.0"));

                supportedVersions = GetSupportedVersions().Result;
                logger.LogInformation($"Supported versions for platform {Platform} are: {string.Join(", ", supportedVersions)}");
            }
            else
            {
                supportedVersions = SupportedVersions.Split(",").Select(version => version.Trim());
            }

            // We ignore text like 'lts' etc and let the underlying scripts to handle them.
            var result = Range;
            var isValidRange = TryGetRange(Range, out var range);
            logger.LogInformation($"Version range to consider: {range}");
            if (isValidRange)
            {
                result = range.MaxSatisfying(supportedVersions);
                if (string.IsNullOrEmpty(result))
                {
                    return 1;
                }
            }

            if (string.Equals(Platform, "dotnet", StringComparison.OrdinalIgnoreCase))
            {
                var version = new SemVer.Version(result);
                result = $"{version.Major}.{version.Minor}";
            }

            console.Write(result);
            return 0;
        }

        private async Task<IEnumerable<string>> GetSupportedVersions()
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