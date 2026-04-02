// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeVersionProvider : INodeVersionProvider
    {
        private readonly BuildScriptGeneratorOptions options;
        private readonly NodeOnDiskVersionProvider onDiskVersionProvider;
        private readonly NodeSdkStorageVersionProvider sdkStorageVersionProvider;
        private readonly NodeExternalVersionProvider externalVersionProvider;
        private readonly NodeAcrVersionProvider acrVersionProvider;
        private readonly ILogger<NodeVersionProvider> logger;
        private PlatformVersionInfo versionInfo;

        public NodeVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            NodeOnDiskVersionProvider onDiskVersionProvider,
            NodeSdkStorageVersionProvider sdkStorageVersionProvider,
            NodeExternalVersionProvider externalVersionProvider,
            NodeAcrVersionProvider acrVersionProvider,
            ILogger<NodeVersionProvider> logger)
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
            INodeVersionProvider provider,
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