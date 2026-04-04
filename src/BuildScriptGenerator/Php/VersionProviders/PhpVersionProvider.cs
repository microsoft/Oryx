// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpVersionProvider : IPhpVersionProvider
    {
        private readonly BuildScriptGeneratorOptions options;
        private readonly PhpOnDiskVersionProvider onDiskVersionProvider;
        private readonly PhpSdkStorageVersionProvider sdkStorageVersionProvider;
        private readonly PhpExternalVersionProvider externalVersionProvider;
        private readonly PhpExternalAcrVersionProvider externalAcrVersionProvider;
        private readonly PhpAcrVersionProvider acrVersionProvider;
        private readonly ILogger<PhpVersionProvider> logger;
        private PlatformVersionInfo versionInfo;

        public PhpVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            PhpOnDiskVersionProvider onDiskVersionProvider,
            PhpSdkStorageVersionProvider sdkStorageVersionProvider,
            PhpExternalVersionProvider externalVersionProvider,
            PhpExternalAcrVersionProvider externalAcrVersionProvider,
            PhpAcrVersionProvider acrVersionProvider,
            ILogger<PhpVersionProvider> logger)
        {
            this.options = options.Value;
            this.onDiskVersionProvider = onDiskVersionProvider;
            this.sdkStorageVersionProvider = sdkStorageVersionProvider;
            this.externalVersionProvider = externalVersionProvider;
            this.externalAcrVersionProvider = externalAcrVersionProvider;
            this.acrVersionProvider = acrVersionProvider;
            this.logger = logger;
        }

        public PlatformVersionInfo GetVersionInfo()
        {
            if (this.versionInfo != null)
            {
                return this.versionInfo;
            }

            this.versionInfo = this.options.EnableDynamicInstall
                ? this.ResolveDynamicVersionInfo()
                : this.onDiskVersionProvider.GetVersionInfo();

            return this.versionInfo;
        }

        private PlatformVersionInfo ResolveDynamicVersionInfo()
        {
            // Priority: External-ACR → External-SDK → Direct-ACR → CDN

            // If external ACR provider is enabled.
            if (this.options.EnableExternalAcrSdkProvider)
            {
                var platformVersionInfo = this.TryGetVersionInfoFromExternalAcrVersionProvider();
                if (platformVersionInfo != null)
                {
                    return platformVersionInfo;
                }
            }

            if (this.options.EnableExternalSdkProvider)
            {
                var platformVersionInfo = this.TryGetVersionInfoFromExternalVersionProvider();
                if (platformVersionInfo != null)
                {
                    return platformVersionInfo;
                }
            }

            if (this.options.EnableAcrSdkProvider)
            {
                var platformVersionInfo = this.TryGetVersionInfoFromAcrVersionProvider();
                if (platformVersionInfo != null)
                {
                    return platformVersionInfo;
                }
            }

            return this.sdkStorageVersionProvider.GetVersionInfo();
        }

        private PlatformVersionInfo TryGetVersionInfoFromExternalAcrVersionProvider()
        {
            try
            {
                return this.externalAcrVersionProvider.GetVersionInfo();
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    $"Error while getting version info from external ACR provider. Ex: {ex}");
                return null;
            }
        }

        private PlatformVersionInfo TryGetVersionInfoFromExternalVersionProvider()
        {
            try
            {
                return this.externalVersionProvider.GetVersionInfo();
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    $"Error while getting version info from external blob provider. Ex: {ex}");
                return null;
            }
        }

        private PlatformVersionInfo TryGetVersionInfoFromAcrVersionProvider()
        {
            try
            {
                return this.acrVersionProvider.GetVersionInfo();
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    $"Error while getting version info from direct ACR provider. Ex: {ex}");
                return null;
            }
        }
    }
}