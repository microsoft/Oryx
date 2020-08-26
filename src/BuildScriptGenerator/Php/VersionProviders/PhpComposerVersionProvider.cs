// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpComposerVersionProvider : IPhpComposerVersionProvider
    {
        private readonly BuildScriptGeneratorOptions _options;
        private readonly PhpComposerOnDiskVersionProvider _onDiskVersionProvider;
        private readonly PhpComposerSdkStorageVersionProvider _sdkStorageVersionProvider;
        private readonly ILogger<PhpComposerVersionProvider> _logger;
        private PlatformVersionInfo _versionInfo;

        public PhpComposerVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            PhpComposerOnDiskVersionProvider onDiskVersionProvider,
            PhpComposerSdkStorageVersionProvider sdkStorageVersionProvider,
            ILogger<PhpComposerVersionProvider> logger)
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