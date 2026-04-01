// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Default implementation of <see cref="ISdkResolver"/> that resolves SDK binaries by
    /// trying multiple sources in priority order: MCR → External SDK provider → CDN fallback.
    /// </summary>
    public class SdkResolver : ISdkResolver
    {
        private readonly BuildScriptGeneratorOptions commonOptions;
        private readonly IMcrSdkProvider mcrSdkProvider;
        private readonly IExternalSdkProvider externalSdkProvider;
        private readonly ILogger<SdkResolver> logger;

        public SdkResolver(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IMcrSdkProvider mcrSdkProvider,
            IExternalSdkProvider externalSdkProvider,
            ILogger<SdkResolver> logger)
        {
            this.commonOptions = commonOptions.Value;
            this.mcrSdkProvider = mcrSdkProvider;
            this.externalSdkProvider = externalSdkProvider;
            this.logger = logger;
        }

        /// <inheritdoc />
        public bool TryFetchSdk(string platformName, string version, string debianFlavor)
        {
            // Try MCR SDK provider first (pulls SDK from a Docker image in MCR)
            if (this.commonOptions.EnableMcrSdkProvider)
            {
                this.logger.LogDebug(
                    "{platformName} version {version} is not installed. " +
                    "MCR SDK provider is enabled, trying to pull SDK from MCR.",
                    platformName,
                    version);

                try
                {
                    var isMcrFetchSuccess = this.mcrSdkProvider
                        .PullSdkAsync(platformName, version, debianFlavor).Result;

                    if (isMcrFetchSuccess)
                    {
                        this.logger.LogDebug(
                            "{platformName} version {version} fetched successfully using MCR SDK provider.",
                            platformName,
                            version);
                        return true;
                    }
                    else
                    {
                        this.logger.LogDebug(
                            "{platformName} version {version} could not be fetched using MCR SDK provider. " +
                            "Falling back to other SDK sources.",
                            platformName,
                            version);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(
                        ex,
                        "Error fetching {platformName} version {version} using MCR SDK provider. " +
                        "Falling back to other SDK sources.",
                        platformName,
                        version);
                }
            }

            // Try external SDK provider (blob storage via Unix domain socket)
            if (this.commonOptions.EnableExternalSdkProvider)
            {
                this.logger.LogDebug(
                    "{platformName} version {version} is not installed. " +
                    "External SDK provider is enabled, trying to fetch SDK using it.",
                    platformName,
                    version);

                try
                {
                    var blobName = BlobNameHelper.GetBlobNameForVersion(platformName, version, debianFlavor);
                    var isExternalFetchSuccess = this.externalSdkProvider
                        .RequestBlobAsync(platformName, blobName).Result;

                    if (isExternalFetchSuccess)
                    {
                        this.logger.LogDebug(
                            "{platformName} version {version} fetched successfully using external SDK provider.",
                            platformName,
                            version);
                        return true;
                    }
                    else
                    {
                        this.logger.LogDebug(
                            "{platformName} version {version} could not be fetched using external SDK provider.",
                            platformName,
                            version);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(
                        ex,
                        "Error fetching {platformName} version {version} using external SDK provider.",
                        platformName,
                        version);
                }
            }

            // No pre-fetch source succeeded; caller should fall back to CDN download
            this.logger.LogDebug(
                "{platformName} version {version} could not be fetched from any SDK source. " +
                "Falling back to CDN-based installation.",
                platformName,
                version);
            return false;
        }
    }
}
