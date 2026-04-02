// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    /// <summary>
    /// ACR-based version provider for .NET SDKs.
    /// Unlike other platforms, .NET requires a runtime→SDK version mapping.
    /// This provider uses a catalog tag containing a JSON mapping, and falls back
    /// to per-tag config label inspection.
    /// </summary>
    public class DotNetCoreAcrVersionProvider : AcrVersionProviderBase, IDotNetCoreVersionProvider
    {
        private readonly BuildScriptGeneratorOptions commonOptions;
        private readonly ILogger logger;
        private Dictionary<string, string> versionMap;
        private string defaultRuntimeVersion;

        public DotNetCoreAcrVersionProvider(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
            : base(commonOptions, httpClientFactory, loggerFactory)
        {
            this.commonOptions = commonOptions.Value;
            this.logger = loggerFactory.CreateLogger<DotNetCoreAcrVersionProvider>();
        }

        public string GetDefaultRuntimeVersion()
        {
            this.EnsureVersionInfo();
            return this.defaultRuntimeVersion;
        }

        public Dictionary<string, string> GetSupportedVersions()
        {
            this.EnsureVersionInfo();
            return this.versionMap;
        }

        private void EnsureVersionInfo()
        {
            if (this.versionMap != null)
            {
                return;
            }

            var repository = $"{SdkStorageConstants.AcrSdkRepositoryPrefix}/{DotNetCoreConstants.PlatformName}";
            var debianFlavor = this.commonOptions.DebianFlavor;

            // Try catalog tag first — single HTTP round-trip for the full runtime→SDK mapping
            if (!this.TryGetVersionInfoFromCatalog(repository, debianFlavor))
            {
                // Fallback: inspect individual tag configs (more HTTP calls)
                this.GetVersionInfoFromTags(repository, debianFlavor);
            }
        }

        private bool TryGetVersionInfoFromCatalog(string repository, string debianFlavor)
        {
            try
            {
                var catalogTag = $"{debianFlavor}-{SdkStorageConstants.AcrCatalogTag}";
                this.logger.LogDebug("Trying .NET catalog tag {tag} from ACR", catalogTag);

                var manifest = this.OciClient.GetManifestAsync(repository, catalogTag).GetAwaiter().GetResult();
                var configDigest = manifest.Config?.Digest;
                if (string.IsNullOrEmpty(configDigest))
                {
                    return false;
                }

                var config = this.OciClient.GetImageConfigAsync(repository, configDigest).GetAwaiter().GetResult();
                if (config?.Config?.Labels == null ||
                    !config.Config.Labels.TryGetValue("org.oryx.dotnet-version-map", out var mapJson))
                {
                    return false;
                }

                var catalog = JsonSerializer.Deserialize<DotNetCatalog>(mapJson);
                if (catalog?.Mappings == null)
                {
                    return false;
                }

                this.versionMap = new Dictionary<string, string>(catalog.Mappings, StringComparer.OrdinalIgnoreCase);
                this.defaultRuntimeVersion = catalog.DefaultRuntimeVersion;
                this.logger.LogDebug("Got .NET version map from catalog tag with {count} entries.", this.versionMap.Count);
                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogDebug(ex, "Catalog tag not available, falling back to per-tag inspection.");
                return false;
            }
        }

        private void GetVersionInfoFromTags(string repository, string debianFlavor)
        {
            this.logger.LogDebug("Getting .NET version info from individual ACR tags.");

            var allTags = this.OciClient.GetAllTagsAsync(repository).GetAwaiter().GetResult();

            var prefix = $"{debianFlavor}-";
            var versionTags = allTags
                .Where(t => t.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                         && !t.EndsWith($"-{SdkStorageConstants.AcrDefaultVersionTag}", StringComparison.OrdinalIgnoreCase)
                         && !t.EndsWith($"-{SdkStorageConstants.AcrCatalogTag}", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var supportedVersions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var tag in versionTags)
            {
                this.TryExtractDotNetVersionFromTag(repository, tag, supportedVersions);
            }

            this.versionMap = supportedVersions;
            this.defaultRuntimeVersion = this.GetDefaultRuntimeVersionFromAcr(repository, debianFlavor);
        }

        private void TryExtractDotNetVersionFromTag(
            string repository,
            string tag,
            Dictionary<string, string> supportedVersions)
        {
            try
            {
                var manifest = this.OciClient.GetManifestAsync(repository, tag).GetAwaiter().GetResult();
                var configDigest = manifest.Config?.Digest;
                if (string.IsNullOrEmpty(configDigest))
                {
                    return;
                }

                var config = this.OciClient.GetImageConfigAsync(repository, configDigest).GetAwaiter().GetResult();
                var labels = config?.Config?.Labels;
                if (labels == null)
                {
                    return;
                }

                if (labels.TryGetValue(SdkStorageConstants.AcrDotnetRuntimeVersionLabelName, out var runtimeVersion) &&
                    labels.TryGetValue(SdkStorageConstants.AcrDotnetSdkVersionLabelName, out var sdkVersion))
                {
                    supportedVersions[runtimeVersion] = sdkVersion;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, "Failed to inspect config for tag {tag}.", tag);
            }
        }

        private string GetDefaultRuntimeVersionFromAcr(string repository, string debianFlavor)
        {
            try
            {
                return this.OciClient.GetDefaultVersionAsync(repository, debianFlavor).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, "Failed to get default .NET runtime version from ACR.");
                return null;
            }
        }

        private class DotNetCatalog
        {
            public Dictionary<string, string> Mappings { get; set; }

            public string DefaultRuntimeVersion { get; set; }
        }
    }
}
