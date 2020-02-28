// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeVersionProvider : INodeVersionProvider
    {
        private readonly BuildScriptGeneratorOptions _options;
        private readonly NodeOnDiskVersionProvider _onDiskVersionProvider;
        private readonly NodeSdkStorageVersionProvider _sdkStorageVersionProvider;

        public NodeVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            NodeOnDiskVersionProvider onDiskVersionProvider,
            NodeSdkStorageVersionProvider sdkStorageVersionProvider)
        {
            _options = options.Value;
            _onDiskVersionProvider = onDiskVersionProvider;
            _sdkStorageVersionProvider = sdkStorageVersionProvider;
        }

        public PlatformVersionInfo GetVersionInfo()
        {
            if (_options.EnableDynamicInstall)
            {
                return _sdkStorageVersionProvider.GetVersionInfo();
            }

            return _onDiskVersionProvider.GetVersionInfo();
        }
    }
}