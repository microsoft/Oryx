// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class DotNetCoreVersionProvider : IDotNetCoreVersionProvider
    {
        private readonly BuildScriptGeneratorOptions _options;
        private readonly DotNetCoreOnDiskVersionProvider _onDiskVersionProvider;
        private readonly DotNetCoreSdkStorageVersionProvider _sdkStorageVersionProvider;
        private string _defaultRuntimeVersion;
        private Dictionary<string, string> _supportedVersions;

        public DotNetCoreVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            DotNetCoreOnDiskVersionProvider onDiskVersionProvider,
            DotNetCoreSdkStorageVersionProvider sdkStorageVersionProvider)
        {
            _options = options.Value;
            _onDiskVersionProvider = onDiskVersionProvider;
            _sdkStorageVersionProvider = sdkStorageVersionProvider;
        }

        public string GetDefaultRuntimeVersion()
        {
            if (string.IsNullOrEmpty(_defaultRuntimeVersion))
            {
                if (_options.EnableDynamicInstall)
                {
                    return _sdkStorageVersionProvider.GetDefaultRuntimeVersion();
                }

                _defaultRuntimeVersion = _onDiskVersionProvider.GetDefaultRuntimeVersion();
            }

            return _defaultRuntimeVersion;
        }

        public Dictionary<string, string> GetSupportedVersions()
        {
            if (_supportedVersions == null)
            {
                if (_options.EnableDynamicInstall)
                {
                    return _sdkStorageVersionProvider.GetSupportedVersions();
                }

                _supportedVersions = _onDiskVersionProvider.GetSupportedVersions();
            }

            return _supportedVersions;
        }
    }
}