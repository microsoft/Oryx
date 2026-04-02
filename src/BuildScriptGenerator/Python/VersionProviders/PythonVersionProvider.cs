// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonVersionProvider : IPythonVersionProvider
    {
        private readonly BuildScriptGeneratorOptions options;
        private readonly PythonOnDiskVersionProvider onDiskVersionProvider;
        private readonly PythonSdkStorageVersionProvider sdkStorageVersionProvider;
        private readonly PythonExternalVersionProvider externalVersionProvider;
        private readonly PythonAcrVersionProvider acrVersionProvider;
        private readonly ILogger<PythonVersionProvider> logger;
        private PlatformVersionInfo versionInfo;

        public PythonVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            PythonOnDiskVersionProvider onDiskVersionProvider,
            PythonSdkStorageVersionProvider sdkStorageVersionProvider,
            PythonExternalVersionProvider externalVersionProvider,
            PythonAcrVersionProvider acrVersionProvider,
            ILogger<PythonVersionProvider> logger)
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
                    this.externalVersionProvider,
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
                    this.acrVersionProvider,
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
            IPythonVersionProvider provider,
            string providerName,
            string fallbackName)
        {
            try
            {
                return provider.GetVersionInfo();
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