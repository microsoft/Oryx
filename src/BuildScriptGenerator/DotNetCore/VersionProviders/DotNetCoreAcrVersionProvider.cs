// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    /// <summary>
    /// ACR-based version provider for .NET SDKs.
    /// Unlike other platforms, .NET requires a runtime→SDK version mapping.
    /// This provider extracts the mapping directly from image tags, which encode
    /// both versions in the format "{osFlavor}-{sdkVersion}_{runtimeVersion}"
    /// (e.g. "noble-10.0.201_10.0.5").
    /// </summary>
    public class DotNetCoreAcrVersionProvider : AcrVersionProviderBase, IDotNetCoreVersionProvider
    {
        private readonly BuildScriptGeneratorOptions commonOptions;
        private readonly ILogger logger;
        private Dictionary<string, string> versionMap;
        private string defaultRuntimeVersion;

        public DotNetCoreAcrVersionProvider(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            OciRegistryClient ociClient,
            ILoggerFactory loggerFactory)
            : base(commonOptions, ociClient, loggerFactory)
        {
            this.commonOptions = commonOptions.Value;
            this.logger = loggerFactory.CreateLogger<DotNetCoreAcrVersionProvider>();
        }

        public string GetDefaultRuntimeVersion()
        {
            this.EnsureVersionInfo();
            if (this.versionMap == null || this.versionMap.Count == 0)
            {
                return null;
            }

            return this.defaultRuntimeVersion;
        }

        public Dictionary<string, string> GetSupportedVersions()
        {
            this.EnsureVersionInfo();
            if (this.versionMap == null || this.versionMap.Count == 0)
            {
                return null;
            }

            return this.versionMap;
        }

        private void EnsureVersionInfo()
        {
            if (this.versionMap != null)
            {
                return;
            }

            var repository = SdkStorageConstants.GetSdkImageRepository(DotNetCoreConstants.PlatformName, this.commonOptions.OryxAcrSdkRepositoryPrefix);
            var debianFlavor = this.commonOptions.DebianFlavor;

            this.GetVersionInfoFromTags(repository, debianFlavor);
        }

        /// <summary>
        /// Parses the runtime→SDK version mapping from ACR tags.
        /// Tags follow the format "{osFlavor}-{sdkVersion}_{runtimeVersion}".
        /// </summary>
        private void GetVersionInfoFromTags(string repository, string debianFlavor)
        {
            this.logger.LogDebug("Getting .NET version info from ACR tags for repository {repository}.", repository);

            var allTags = this.OciClient.GetAllTagsAsync(repository).GetAwaiter().GetResult();

            var prefix = $"{debianFlavor}-";
            var supportedVersions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var tag in allTags)
            {
                if (!tag.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Strip the osFlavor prefix to get "{sdkVersion}_{runtimeVersion}"
                var versionPart = tag.Substring(prefix.Length);
                var parts = versionPart.Split('_', 2);
                var sdkVersion = parts[0];
                var runtimeVersion = parts.Length > 1 ? parts[1] : null;
                if (parts.Length != 2 || string.IsNullOrEmpty(sdkVersion) || string.IsNullOrEmpty(runtimeVersion))
                {
                    this.logger.LogDebug("Skipping tag '{tag}' — does not match expected format.", tag);
                    continue;
                }

                supportedVersions[runtimeVersion] = sdkVersion;
            }

            this.versionMap = supportedVersions;

            DotNetCoreConstants.DefaultVersionPerFlavor.TryGetValue(
                debianFlavor ?? string.Empty, out var defaultVersion);
            this.defaultRuntimeVersion = defaultVersion;

            this.logger.LogDebug(
                "Found {count} .NET runtime→SDK mappings from tags (default runtime: {default}).",
                supportedVersions.Count,
                this.defaultRuntimeVersion ?? "none");
        }
    }
}
