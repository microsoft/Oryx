// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Java
{
    internal class MavenVersionProvider : IMavenVersionProvider
    {
        private readonly BuildScriptGeneratorOptions _options;
        private readonly MavenOnDiskVersionProvider _onDiskVersionProvider;
        private readonly MavenSdkStorageVersionProvider _sdkStorageVersionProvider;
        private readonly ILogger<JavaVersionProvider> _logger;
        private PlatformVersionInfo _versionInfo;

        public MavenVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            MavenOnDiskVersionProvider onDiskVersionProvider,
            MavenSdkStorageVersionProvider sdkStorageVersionProvider,
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