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
        private readonly ILogger<NodeVersionProvider> logger;
        private PlatformVersionInfo versionInfo;

        public NodeVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            NodeOnDiskVersionProvider onDiskVersionProvider,
            NodeSdkStorageVersionProvider sdkStorageVersionProvider,
            NodeExternalVersionProvider externalVersionProvider,
            ILogger<NodeVersionProvider> logger)
        {
            this.options = options.Value;
            this.onDiskVersionProvider = onDiskVersionProvider;
            this.sdkStorageVersionProvider = sdkStorageVersionProvider;
            this.externalVersionProvider = externalVersionProvider;
            this.logger = logger;
        }

        public PlatformVersionInfo GetVersionInfo()
        {
            if (this.versionInfo == null)
            {
                if (this.options.EnableDynamicInstall)
                {
                    if (this.options.EnableExternalSdkProvider)
                    {
                        try
                        {
                            return this.externalVersionProvider.GetVersionInfo();
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogError($"Failed to get version info from external SDK provider. Falling back to http based sdkStorageVersionProvider. Ex: {ex}");
                        }
                    }

                    return this.sdkStorageVersionProvider.GetVersionInfo();
                }

                this.versionInfo = this.onDiskVersionProvider.GetVersionInfo();
            }

            return this.versionInfo;
        }
    }
}