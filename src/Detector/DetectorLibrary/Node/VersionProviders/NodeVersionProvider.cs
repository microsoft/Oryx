// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.Detector.Node
{
    internal class NodeVersionProvider
    {
        private readonly DetectorOptions _options;
        private readonly NodeOnDiskVersionProvider _onDiskVersionProvider;
        private readonly NodeSdkStorageVersionProvider _sdkStorageVersionProvider;
        private readonly ILogger<NodeVersionProvider> _logger;
        private PlatformVersionInfo _versionInfo;

        public NodeVersionProvider(
            IOptions<DetectorOptions> options,
            NodeOnDiskVersionProvider onDiskVersionProvider,
            NodeSdkStorageVersionProvider sdkStorageVersionProvider,
            ILogger<NodeVersionProvider> logger)
        {
            _options = options.Value;
            _onDiskVersionProvider = onDiskVersionProvider;
            _sdkStorageVersionProvider = sdkStorageVersionProvider;
            _logger = logger;
        }

        public PlatformVersionInfo GetVersionInfo()
        {
            if (_versionInfo == null)
            {
                if (_options.EnableDynamicInstall)
                {
                    return _sdkStorageVersionProvider.GetVersionInfo();
                }

                _versionInfo = _onDiskVersionProvider.GetVersionInfo();
            }

            return _versionInfo;
        }
    }
}