// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Ruby
{
    internal class RubyVersionProvider : IRubyVersionProvider
    {
        private readonly BuildScriptGeneratorOptions _options;
        private readonly RubyOnDiskVersionProvider _onDiskVersionProvider;
        private readonly RubySdkStorageVersionProvider _sdkStorageVersionProvider;
        private readonly ILogger<RubyVersionProvider> _logger;
        private PlatformVersionInfo _versionInfo;

        public RubyVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            RubyOnDiskVersionProvider onDiskVersionProvider,
            RubySdkStorageVersionProvider sdkStorageVersionProvider,
            ILogger<RubyVersionProvider> logger)
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