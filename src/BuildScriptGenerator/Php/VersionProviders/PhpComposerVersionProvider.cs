// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpComposerVersionProvider : IPhpComposerVersionProvider
    {
        private readonly BuildScriptGeneratorOptions options;
        private readonly PhpComposerOnDiskVersionProvider phpComposerOnDiskVersionProvider;
        private readonly PhpComposerSdkStorageVersionProvider phpComposerSdkStorageVersionProvider;
        private readonly PhpComposerExternalVersionProvider phpComposerExternalVersionProvider;
        private readonly PhpComposerExternalAcrVersionProvider phpComposerExternalAcrVersionProvider;
        private readonly PhpComposerAcrVersionProvider phpComposerAcrVersionProvider;
        private readonly ILogger<PhpComposerVersionProvider> logger;
        private PlatformVersionInfo versionInfo;

        public PhpComposerVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            PhpComposerOnDiskVersionProvider phpComposerOnDiskVersionProvider,
            PhpComposerSdkStorageVersionProvider phpComposerSdkStorageVersionProvider,
            PhpComposerExternalVersionProvider phpComposerExternalVersionProvider,
            PhpComposerExternalAcrVersionProvider phpComposerExternalAcrVersionProvider,
            PhpComposerAcrVersionProvider phpComposerAcrVersionProvider,
            ILogger<PhpComposerVersionProvider> logger)
        {
            this.options = options.Value;
            this.phpComposerOnDiskVersionProvider = phpComposerOnDiskVersionProvider;
            this.phpComposerSdkStorageVersionProvider = phpComposerSdkStorageVersionProvider;
            this.phpComposerExternalVersionProvider = phpComposerExternalVersionProvider;
            this.phpComposerExternalAcrVersionProvider = phpComposerExternalAcrVersionProvider;
            this.phpComposerAcrVersionProvider = phpComposerAcrVersionProvider;
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
                : this.phpComposerOnDiskVersionProvider.GetVersionInfo();

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

            return this.phpComposerSdkStorageVersionProvider.GetVersionInfo();
        }

        private PlatformVersionInfo TryGetVersionInfoFromExternalAcrVersionProvider()
        {
            try
            {
                return this.phpComposerExternalAcrVersionProvider.GetVersionInfo();
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
                return this.phpComposerExternalVersionProvider.GetVersionInfo();
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
                return this.phpComposerAcrVersionProvider.GetVersionInfo();
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