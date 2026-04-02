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
        private readonly PhpAcrVersionProvider acrVersionProvider;
        private readonly ILogger<PhpVersionProvider> logger;
        private PlatformVersionInfo versionInfo;

        public PhpVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            PhpOnDiskVersionProvider onDiskVersionProvider,
            PhpSdkStorageVersionProvider sdkStorageVersionProvider,
            PhpExternalVersionProvider externalVersionProvider,
            PhpAcrVersionProvider acrVersionProvider,
            ILogger<PhpVersionProvider> logger)
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
                try
                {
                    return this.externalVersionProvider.GetVersionInfo();
                }
                catch (Exception ex)
                {
                    this.logger.LogError(
                        $"Failed to get version info from external SDK provider. Falling back. Ex: {ex}");
                }
            }

            if (this.options.EnableAcrSdkProvider)
            {
                try
                {
                    return this.acrVersionProvider.GetVersionInfo();
                }
                catch (Exception ex)
                {
                    this.logger.LogError(
                        $"Failed to get version info from ACR provider. Falling back to blob storage. Ex: {ex}");
                }
            }

            return this.sdkStorageVersionProvider.GetVersionInfo();
        }
    }
}