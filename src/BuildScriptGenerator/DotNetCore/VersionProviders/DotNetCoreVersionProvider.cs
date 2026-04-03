// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class DotNetCoreVersionProvider : IDotNetCoreVersionProvider
    {
        private readonly BuildScriptGeneratorOptions cliOptions;
        private readonly DotNetCoreOnDiskVersionProvider onDiskVersionProvider;
        private readonly DotNetCoreSdkStorageVersionProvider sdkStorageVersionProvider;
        private readonly DotNetCoreExternalVersionProvider externalVersionProvider;
        private readonly DotNetCoreExternalAcrVersionProvider externalAcrVersionProvider;
        private readonly DotNetCoreAcrVersionProvider acrVersionProvider;
        private readonly ILogger<DotNetCoreVersionProvider> logger;
        private string defaultRuntimeVersion;
        private Dictionary<string, string> supportedVersions;

        public DotNetCoreVersionProvider(
            IOptions<BuildScriptGeneratorOptions> cliOptions,
            DotNetCoreOnDiskVersionProvider onDiskVersionProvider,
            DotNetCoreSdkStorageVersionProvider sdkStorageVersionProvider,
            DotNetCoreExternalVersionProvider externalVersionProvider,
            DotNetCoreExternalAcrVersionProvider externalAcrVersionProvider,
            DotNetCoreAcrVersionProvider acrVersionProvider,
            ILogger<DotNetCoreVersionProvider> logger)
        {
            this.cliOptions = cliOptions.Value;
            this.onDiskVersionProvider = onDiskVersionProvider;
            this.sdkStorageVersionProvider = sdkStorageVersionProvider;
            this.externalVersionProvider = externalVersionProvider;
            this.externalAcrVersionProvider = externalAcrVersionProvider;
            this.acrVersionProvider = acrVersionProvider;
            this.logger = logger;
        }

        public string GetDefaultRuntimeVersion()
        {
            if (string.IsNullOrEmpty(this.defaultRuntimeVersion))
            {
                this.defaultRuntimeVersion = this.cliOptions.EnableDynamicInstall
                    ? this.ResolveDynamicDefaultRuntimeVersion()
                    : this.onDiskVersionProvider.GetDefaultRuntimeVersion();
            }

            this.logger.LogDebug("Default runtime version is {defaultRuntimeVersion}", this.defaultRuntimeVersion);

            return this.defaultRuntimeVersion;
        }

        public Dictionary<string, string> GetSupportedVersions()
        {
            if (this.supportedVersions == null)
            {
                this.supportedVersions = this.cliOptions.EnableDynamicInstall
                    ? this.ResolveDynamicSupportedVersions()
                    : this.onDiskVersionProvider.GetSupportedVersions();

                // A temporary fix to make building netcoreapp1.0 versions using the 1.1 SDK
                // This SDK has 2 runtimes: 1.1.13 and 1.0.16
                this.supportedVersions[DotNetCoreRunTimeVersions.NetCoreApp10] =
                    DotNetCoreSdkVersions.DotNetCore11SdkVersion;
            }

            this.logger.LogDebug("Got the list of supported versions");

            return this.supportedVersions;
        }

        // Priority: External-ACR → External-blob → Direct-ACR → CDN
        private string ResolveDynamicDefaultRuntimeVersion()
        {
            if (this.cliOptions.EnableExternalAcrSdkProvider)
            {
                try
                {
                    return this.externalAcrVersionProvider.GetDefaultRuntimeVersion();
                }
                catch (System.Exception ex)
                {
                    this.logger.LogError(
                        $"Failed to get default runtime version from external ACR provider. Falling back. Ex: {ex}");
                }
            }

            if (this.cliOptions.EnableExternalSdkProvider)
            {
                try
                {
                    return this.externalVersionProvider.GetDefaultRuntimeVersion();
                }
                catch (System.Exception ex)
                {
                    this.logger.LogError(
                        $"Failed to get default runtime version from external SDK provider. Falling back. Ex: {ex}");
                }
            }

            if (this.cliOptions.EnableAcrSdkProvider)
            {
                try
                {
                    return this.acrVersionProvider.GetDefaultRuntimeVersion();
                }
                catch (System.Exception ex)
                {
                    this.logger.LogError(
                        $"Failed to get default runtime version from direct ACR provider. Falling back to CDN. Ex: {ex}");
                }
            }

            return this.sdkStorageVersionProvider.GetDefaultRuntimeVersion();
        }

        private Dictionary<string, string> ResolveDynamicSupportedVersions()
        {
            if (this.cliOptions.EnableExternalAcrSdkProvider)
            {
                try
                {
                    return this.externalAcrVersionProvider.GetSupportedVersions();
                }
                catch (System.Exception ex)
                {
                    this.logger.LogError(
                        $"Failed to get supported versions from external ACR provider. Falling back. Ex: {ex}");
                }
            }

            if (this.cliOptions.EnableExternalSdkProvider)
            {
                try
                {
                    return this.externalVersionProvider.GetSupportedVersions();
                }
                catch (System.Exception ex)
                {
                    this.logger.LogError(
                        $"Failed to get supported versions from external SDK provider. Falling back. Ex: {ex}");
                }
            }

            if (this.cliOptions.EnableAcrSdkProvider)
            {
                try
                {
                    return this.acrVersionProvider.GetSupportedVersions();
                }
                catch (System.Exception ex)
                {
                    this.logger.LogError(
                        $"Failed to get supported versions from direct ACR provider. Falling back to CDN. Ex: {ex}");
                }
            }

            return this.sdkStorageVersionProvider.GetSupportedVersions();
        }
    }
}