// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Base class for ACR-based SDK version providers. Parallel to <see cref="SdkStorageVersionProviderBase"/>
    /// but discovers versions via OCI Distribution API (tag listing + image config labels) instead of
    /// Azure Blob Storage listing with XML metadata.
    /// </summary>
    public class AcrVersionProviderBase
    {
        private readonly ILogger logger;
        private readonly BuildScriptGeneratorOptions commonOptions;
        private readonly OciRegistryClient ociClient;

        public AcrVersionProviderBase(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
        {
            this.commonOptions = commonOptions.Value;
            this.logger = loggerFactory.CreateLogger(this.GetType());

            var registryUrl = this.commonOptions.OryxAcrSdkRegistryUrl;
            if (string.IsNullOrEmpty(registryUrl))
            {
                registryUrl = SdkStorageConstants.DefaultAcrSdkRegistryUrl;
            }

            this.ociClient = new OciRegistryClient(registryUrl, httpClientFactory, loggerFactory);
        }

        protected OciRegistryClient OciClient => this.ociClient;

        /// <summary>
        /// Lists available versions for a platform from ACR tags.
        /// Tags are in the format "{osFlavor}-{version}" (e.g. "bookworm-20.19.3").
        /// Tags ending with "-default" or "-catalog" are excluded.
        /// </summary>
        protected PlatformVersionInfo GetAvailableVersionsFromAcr(string platformName)
        {
            this.logger.LogDebug("Getting list of available versions for platform {platformName} from ACR.", platformName);

            var repository = $"{SdkStorageConstants.AcrSdkRepositoryPrefix}/{platformName}";
            var debianFlavor = this.commonOptions.DebianFlavor;

            List<string> allTags;
            try
            {
                allTags = this.ociClient.GetAllTagsAsync(repository).Result;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to get tags from ACR for {repository}", repository);
                throw;
            }

            // Filter tags: match "{debianFlavor}-{version}", exclude "-default" and "-catalog" suffixes
            var prefix = $"{debianFlavor}-";
            var supportedVersions = allTags
                .Where(t => t.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                         && !t.EndsWith($"-{SdkStorageConstants.AcrDefaultVersionTag}", StringComparison.OrdinalIgnoreCase)
                         && !t.EndsWith($"-{SdkStorageConstants.AcrCatalogTag}", StringComparison.OrdinalIgnoreCase))
                .Select(t => t.Substring(prefix.Length))
                .ToList();

            this.logger.LogDebug("Found {count} versions for {platformName} on ACR.", supportedVersions.Count, platformName);

            string defaultVersion = null;
            try
            {
                defaultVersion = this.ociClient.GetDefaultVersionAsync(repository, debianFlavor).Result;
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, "Failed to get default version for {platformName} from ACR.", platformName);
            }

            return PlatformVersionInfo.CreateAvailableOnAcr(supportedVersions, defaultVersion);
        }
    }
}
