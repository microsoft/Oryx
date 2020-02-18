// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonVersionProvider : IPythonVersionProvider
    {
        private readonly BuildScriptGeneratorOptions _options;
        private readonly PythonOnDiskVersionProvider _onDiskVersionProvider;
        private readonly PythonSdkStorageVersionProvider _sdkStorageVersionProvider;

        public PythonVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            PythonOnDiskVersionProvider onDiskVersionProvider,
            PythonSdkStorageVersionProvider sdkStorageVersionProvider)
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