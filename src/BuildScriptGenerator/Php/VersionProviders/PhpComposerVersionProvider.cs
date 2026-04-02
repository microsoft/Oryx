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
        private readonly PhpComposerOnDiskVersionProvider onDiskVersionProvider;
        private readonly PhpComposerSdkStorageVersionProvider sdkStorageVersionProvider;
        private readonly PhpComposerExternalVersionProvider externalVersionProvider;
        private readonly PhpComposerAcrVersionProvider acrVersionProvider;
        private readonly ILogger<PhpComposerVersionProvider> logger;
        private PlatformVersionInfo versionInfo;

        public PhpComposerVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            PhpComposerOnDiskVersionProvider onDiskVersionProvider,
            PhpComposerSdkStorageVersionProvider sdkStorageVersionProvider,
            PhpComposerExternalVersionProvider externalVersionProvider,
            PhpComposerAcrVersionProvider acrVersionProvider,
            ILogger<PhpComposerVersionProvider> logger)
        {
            this.options = options.Value;
            this.onDiskVersionProvider = onDiskVersionProvider;
            this.sdkStorageVersionProvider = sdkStorageVersionProvider;
            this.externalVersionProvider = externalVersionProvider;
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
            if (this.options.EnableExternalSdkProvider)
            {
                var result = this.TryGetVersionInfo(
                    () => this.externalVersionProvider.GetVersionInfo(),
                    "external SDK provider",
                    "sdkStorageVersionProvider");
                if (result != null)
                {
                    return result;
                }
            }

            // ACR-based version discovery
            if (this.options.EnableAcrSdkProvider)
            {
                var acrResult = this.TryGetVersionInfo(
                    () => this.acrVersionProvider.GetVersionInfo(),
                    "ACR provider",
                    "blob storage");
                if (acrResult != null)
                {
                    return acrResult;
                }
            }

            return this.sdkStorageVersionProvider.GetVersionInfo();
        }

        private PlatformVersionInfo TryGetVersionInfo(
            Func<PlatformVersionInfo> getVersionInfo,
            string providerName,
            string fallbackName)
        {
            try
            {
                return getVersionInfo();
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    $"Failed to get version info from {providerName}. Falling back to {fallbackName}. Ex: {ex}");
                return null;
            }
        }
    }
}