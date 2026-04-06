// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Linq;
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
        private readonly IStandardOutputWriter outputWriter;
        private PlatformVersionInfo versionInfo;

        public PhpVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            PhpOnDiskVersionProvider onDiskVersionProvider,
            PhpSdkStorageVersionProvider sdkStorageVersionProvider,
            PhpExternalVersionProvider externalVersionProvider,
            PhpExternalAcrVersionProvider externalAcrVersionProvider,
            PhpAcrVersionProvider acrVersionProvider,
            ILogger<PhpVersionProvider> logger,
            IStandardOutputWriter outputWriter)
        {
            this.options = options.Value;
            this.onDiskVersionProvider = onDiskVersionProvider;
            this.sdkStorageVersionProvider = sdkStorageVersionProvider;
            this.externalVersionProvider = externalVersionProvider;
            this.externalAcrVersionProvider = externalAcrVersionProvider;
            this.acrVersionProvider = acrVersionProvider;
            this.logger = logger;
            this.outputWriter = outputWriter;
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

        // This method resolves the PHP version info based on the enabled providers and their priority
        // It tries each provider in order and returns the first successful result.
        // Priority: External-ACR → External-SDK → Direct-ACR → CDN
        private PlatformVersionInfo ResolveDynamicVersionInfo()
        {
            // If external ACR provider is enabled.
            if (this.options.EnableExternalAcrSdkProvider)
            {
                var platformVersionInfo = this.TryGetVersionInfoFromExternalAcrVersionProvider();
                if (this.HasSupportedVersions(platformVersionInfo))
                {
                    this.outputWriter.WriteLine("Version resolved using external ACR SDK provider.");
                    return platformVersionInfo;
                }
            }

            if (this.options.EnableExternalSdkProvider)
            {
                var platformVersionInfo = this.TryGetVersionInfoFromExternalVersionProvider();
                if (this.HasSupportedVersions(platformVersionInfo))
                {
                    this.outputWriter.WriteLine("Version resolved using external SDK provider(blob).");
                    return platformVersionInfo;
                }
            }

            if (this.options.EnableAcrSdkProvider)
            {
                var platformVersionInfo = this.TryGetVersionInfoFromAcrVersionProvider();
                if (this.HasSupportedVersions(platformVersionInfo))
                {
                    this.outputWriter.WriteLine("Version resolved using direct ACR SDK provider.");
                    return platformVersionInfo;
                }
            }

            this.outputWriter.WriteLine("Version resolved using blob SDK storage provider(CDN).");
            return this.sdkStorageVersionProvider.GetVersionInfo();
        }

        private bool HasSupportedVersions(PlatformVersionInfo versionInfo)
        {
            if (versionInfo?.SupportedVersions == null)
            {
                return false;
            }

            return versionInfo.SupportedVersions.Any();
        }

        private PlatformVersionInfo TryGetVersionInfoFromExternalAcrVersionProvider()
        {
            try
            {
                var result = this.externalAcrVersionProvider.GetVersionInfo();
                if (result == null)
                {
                    this.logger.LogWarning(
                        "External ACR version provider returned no version info for php. Trying next provider.");
                }

                return result;
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