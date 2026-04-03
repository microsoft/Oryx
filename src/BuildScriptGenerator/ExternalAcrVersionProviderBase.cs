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
    /// Base class for version providers that discover available SDK versions from ACR via external socket provider.
    /// Parallel to <see cref="ExternalSdkStorageVersionProviderBase"/> (blob storage via socket) and
    /// <see cref="AcrVersionProviderBase"/> (direct OCI API).
    /// </summary>
    /// <remarks>
    /// Flow: Oryx → socket → external host → ACR tag listing.
    /// </remarks>
    public class ExternalAcrVersionProviderBase
    {
        private readonly ILogger logger;
        private readonly BuildScriptGeneratorOptions commonOptions;
        private readonly IExternalAcrSdkProvider externalAcrProvider;

        public ExternalAcrVersionProviderBase(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IExternalAcrSdkProvider externalAcrSdkProvider,
            ILoggerFactory loggerFactory)
        {
            this.commonOptions = commonOptions.Value;
            this.logger = loggerFactory.CreateLogger(this.GetType());
            this.externalAcrProvider = externalAcrSdkProvider;
        }

        /// <summary>
        /// Gets the list of available versions and default version for <paramref name="platformName"/>
        /// from ACR via external socket provider.
        /// </summary>
        protected PlatformVersionInfo GetAvailableVersionsFromExternalAcr(string platformName)
        {
            this.logger.LogInformation(
                "Getting available versions for platform {PlatformName} from ACR via external provider.",
                platformName);

            var debianFlavor = this.commonOptions.DebianFlavor;

            var versions = this.externalAcrProvider
                .GetVersionsAsync(platformName, debianFlavor)
                .GetAwaiter()
                .GetResult();

            var supportedVersions = versions?.ToList() ?? new List<string>();

            var defaultVersion = this.GetDefaultVersion(platformName, debianFlavor);

            this.logger.LogInformation(
                "Found {Count} versions for {PlatformName} from ACR via external provider (default: {Default}).",
                supportedVersions.Count,
                platformName,
                defaultVersion ?? "none");

            return PlatformVersionInfo.CreateAvailableOnAcr(supportedVersions, defaultVersion);
        }

        private string GetDefaultVersion(string platformName, string debianFlavor)
        {
            try
            {
                return this.externalAcrProvider
                    .GetDefaultVersionAsync(platformName, debianFlavor)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(
                    ex,
                    "Failed to get default version for {PlatformName} from ACR via external provider.",
                    platformName);
                return null;
            }
        }
    }
}
