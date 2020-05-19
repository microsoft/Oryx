// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.Detector.Python
{
    internal class PythonVersionProvider
    {
        private readonly DetectorOptions _options;
        private readonly PythonOnDiskVersionProvider _onDiskVersionProvider;
        private readonly PythonSdkStorageVersionProvider _sdkStorageVersionProvider;
        private readonly ILogger<PythonVersionProvider> _logger;
        private PlatformVersionInfo _versionInfo;

        public PythonVersionProvider(
            IOptions<DetectorOptions> options,
            PythonOnDiskVersionProvider onDiskVersionProvider,
            PythonSdkStorageVersionProvider sdkStorageVersionProvider,
            ILogger<PythonVersionProvider> logger)
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