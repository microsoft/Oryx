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
        private readonly DotNetCoreAcrVersionProvider acrVersionProvider;
        private readonly ILogger<DotNetCoreVersionProvider> logger;
        private string defaultRuntimeVersion;
        private Dictionary<string, string> supportedVersions;

        public DotNetCoreVersionProvider(
            IOptions<BuildScriptGeneratorOptions> cliOptions,
            DotNetCoreOnDiskVersionProvider onDiskVersionProvider,
            DotNetCoreSdkStorageVersionProvider sdkStorageVersionProvider,
            DotNetCoreExternalVersionProvider externalVersionProvider,
            DotNetCoreAcrVersionProvider acrVersionProvider,
            ILogger<DotNetCoreVersionProvider> logger)
        {
            this.cliOptions = cliOptions.Value;
            this.onDiskVersionProvider = onDiskVersionProvider;
            this.sdkStorageVersionProvider = sdkStorageVersionProvider;
            this.externalVersionProvider = externalVersionProvider;
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

        private string ResolveDynamicDefaultRuntimeVersion()
        {
            if (this.cliOptions.EnableExternalSdkProvider)
            {
                var result = this.TryGet(
                    () => this.externalVersionProvider.GetDefaultRuntimeVersion(),
                    "external SDK provider",
                    "sdkStorageVersionProvider");
                if (result != null)
                {
                    return result;
                }
            }

            if (this.cliOptions.EnableAcrSdkProvider)
            {
                var result = this.TryGet(
                    () => this.acrVersionProvider.GetDefaultRuntimeVersion(),
                    "ACR provider",
                    "blob storage");
                if (result != null)
                {
                    return result;
                }
            }

            return this.sdkStorageVersionProvider.GetDefaultRuntimeVersion();
        }

        private Dictionary<string, string> ResolveDynamicSupportedVersions()
        {
            if (this.cliOptions.EnableExternalSdkProvider)
            {
                var result = this.TryGet(
                    () => this.externalVersionProvider.GetSupportedVersions(),
                    "external SDK provider",
                    "sdkStorageVersionProvider");
                if (result != null)
                {
                    return result;
                }
            }

            if (this.cliOptions.EnableAcrSdkProvider)
            {
                var result = this.TryGet(
                    () => this.acrVersionProvider.GetSupportedVersions(),
                    "ACR provider",
                    "blob storage");
                if (result != null)
                {
                    return result;
                }
            }

            return this.sdkStorageVersionProvider.GetSupportedVersions();
        }

        private T TryGet<T>(
            System.Func<T> getter,
            string providerName,
            string fallbackName)
            where T : class
        {
            try
            {
                return getter();
            }
            catch (System.Exception ex)
            {
                this.logger.LogError(
                    $"Failed to get data from {providerName}. Falling back to {fallbackName}. Ex: {ex}");
                return null;
            }
        }
    }
}