// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Golang
{
    internal class GolangVersionProvider : IGolangVersionProvider
    {
        private readonly BuildScriptGeneratorOptions _options;
        private readonly GolangOnDiskVersionProvider _onDiskVersionProvider;
        private readonly GolangSdkStorageVersionProvider _sdkStorageVersionProvider;
        private readonly ILogger<GolangVersionProvider> _logger;
        private PlatformVersionInfo _versionInfo;

        public GolangVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            GolangOnDiskVersionProvider onDiskVersionProvider,
            GolangSdkStorageVersionProvider sdkStorageVersionProvider,
            ILogger<GolangVersionProvider> logger)
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
