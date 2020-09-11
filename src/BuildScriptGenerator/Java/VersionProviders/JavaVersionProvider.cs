// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Java
{
    internal class JavaVersionProvider : IJavaVersionProvider
    {
        private readonly BuildScriptGeneratorOptions _options;
        private readonly JavaOnDiskVersionProvider _onDiskVersionProvider;
        private readonly JavaSdkStorageVersionProvider _sdkStorageVersionProvider;
        private readonly ILogger<JavaVersionProvider> _logger;
        private PlatformVersionInfo _versionInfo;

        public JavaVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            JavaOnDiskVersionProvider onDiskVersionProvider,
            JavaSdkStorageVersionProvider sdkStorageVersionProvider,
            ILogger<JavaVersionProvider> logger)
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