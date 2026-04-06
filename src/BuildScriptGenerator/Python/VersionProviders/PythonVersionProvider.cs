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
        private readonly PythonExternalAcrVersionProvider externalAcrVersionProvider;
        private readonly PythonAcrVersionProvider acrVersionProvider;
        private readonly ILogger<PythonVersionProvider> logger;
        private readonly IStandardOutputWriter outputWriter;
        private PlatformVersionInfo versionInfo;

        public PythonVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            PythonOnDiskVersionProvider onDiskVersionProvider,
            PythonSdkStorageVersionProvider sdkStorageVersionProvider,
            PythonExternalVersionProvider externalVersionProvider,
            PythonExternalAcrVersionProvider externalAcrVersionProvider,
            PythonAcrVersionProvider acrVersionProvider,
            ILogger<PythonVersionProvider> logger,
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

        // This method resolves the Python version info based on the enabled providers and their priority
        // It tries each provider in order and returns the first successful result.
        // Priority: External-ACR → External-SDK → Direct-ACR → CDN
        private PlatformVersionInfo ResolveDynamicVersionInfo()
        {
            // If external ACR provider is enabled.
            if (this.options.EnableExternalAcrSdkProvider)
            {
                var platformVersionInfo = this.TryGetVersionInfoFromExternalAcrVersionProvider();
                if (platformVersionInfo != null)
                {
                    this.outputWriter.WriteLine("Version resolved using external ACR SDK provider.");
                    return platformVersionInfo;
                }
            }

            // If external SDK provider is enabled.
            if (this.options.EnableExternalSdkProvider)
            {
                var platformVersionInfo = this.TryGetVersionInfoFromExternalVersionProvider();
                if (platformVersionInfo != null)
                {
                    this.outputWriter.WriteLine("Version resolved using external SDK provider(blob).");
                    return platformVersionInfo;
                }
            }

            // If direct ACR provider is enabled.
            if (this.options.EnableAcrSdkProvider)
            {
                var platformVersionInfo = this.TryGetVersionInfoFromAcrVersionProvider();
                if (platformVersionInfo != null)
                {
                    this.outputWriter.WriteLine("Version resolved using direct ACR SDK provider.");
                    return platformVersionInfo;
                }
            }

            this.outputWriter.WriteLine("Version resolved using blob SDK storage provider(CDN).");
            return this.sdkStorageVersionProvider.GetVersionInfo();
        }

        private PlatformVersionInfo TryGetVersionInfoFromExternalAcrVersionProvider()
        {
            try
            {
                var result = this.externalAcrVersionProvider.GetVersionInfo();
                if (result == null)
                {
                    this.logger.LogWarning(
                        "External ACR version provider returned no version info for python. Trying next provider.");
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