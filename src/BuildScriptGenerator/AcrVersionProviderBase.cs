// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Base class for ACR-based SDK version providers. Parallel to <see cref="SdkStorageVersionProviderBase"/>
    /// but discovers versions via OCI Distribution API (tag listing) instead of
    /// Azure Blob Storage listing with XML metadata.
    /// Default versions come from local per-flavor constants rather than ACR image labels.
    /// </summary>
    public class AcrVersionProviderBase
    {
        private readonly ILogger logger;
        private readonly string debianFlavor;
        private readonly string repositoryPrefix;

        public AcrVersionProviderBase(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            OciRegistryClient ociClient,
            ILoggerFactory loggerFactory)
        {
            var options = commonOptions.Value;
            this.logger = loggerFactory.CreateLogger(this.GetType());
            this.debianFlavor = options.DebianFlavor;
            this.repositoryPrefix = options.OryxAcrSdkRepositoryPrefix;
            this.OciClient = ociClient;
        }

        protected OciRegistryClient OciClient { get; }

        /// <summary>
        /// Lists available versions for a platform from ACR tags and resolves the default
        /// version from the supplied per-flavor dictionary.
        /// </summary>
        protected PlatformVersionInfo GetAvailableVersionsFromAcr(
            string platformName,
            Dictionary<string, string> defaultVersionPerFlavor)
        {
            var repository = SdkStorageConstants.GetSdkImageRepository(platformName, this.repositoryPrefix);

            this.logger.LogDebug("Getting available versions for {platformName} from ACR repository {repository}.", platformName, repository);

            var allTags = this.GetTags(repository);
            var supportedVersions = this.FilterVersionTags(allTags);

            string defaultVersion = null;
            if (defaultVersionPerFlavor != null &&
                !string.IsNullOrEmpty(this.debianFlavor) &&
                defaultVersionPerFlavor.TryGetValue(this.debianFlavor, out var version))
            {
                defaultVersion = version;
            }

            this.logger.LogDebug(
                "Found {count} versions for {platformName} on ACR (default: {default}).",
                supportedVersions.Count,
                platformName,
                defaultVersion ?? "none");

            return PlatformVersionInfo.CreateAvailableOnAcr(supportedVersions, defaultVersion);
        }

        private List<string> GetTags(string repository)
        {
            try
            {
                return this.OciClient.GetAllTagsAsync(repository).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to get tags from ACR for {repository}.", repository);
                throw;
            }
        }

        private List<string> FilterVersionTags(List<string> allTags)
        {
            var prefix = $"{this.debianFlavor}-";
            return allTags
                .Where(t => t.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(t => t.Substring(prefix.Length))
                .ToList();
        }
    }
}
