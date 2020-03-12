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
        private readonly BuildScriptGeneratorOptions _cliOptions;
        private readonly DotNetCoreOnDiskVersionProvider _onDiskVersionProvider;
        private readonly DotNetCoreSdkStorageVersionProvider _sdkStorageVersionProvider;
        private string _defaultRuntimeVersion;
        private Dictionary<string, string> _supportedVersions;

        public DotNetCoreVersionProvider(
            IOptions<BuildScriptGeneratorOptions> cliOptions,
            DotNetCoreOnDiskVersionProvider onDiskVersionProvider,
            DotNetCoreSdkStorageVersionProvider sdkStorageVersionProvider)
        {
            _cliOptions = cliOptions.Value;
            _onDiskVersionProvider = onDiskVersionProvider;
            _sdkStorageVersionProvider = sdkStorageVersionProvider;
        }

        public string GetDefaultRuntimeVersion()
        {
            if (string.IsNullOrEmpty(_defaultRuntimeVersion))
            {
                _defaultRuntimeVersion = _cliOptions.EnableDynamicInstall ?
                    _sdkStorageVersionProvider.GetDefaultRuntimeVersion() :
                    _onDiskVersionProvider.GetDefaultRuntimeVersion();
            }

            return _defaultRuntimeVersion;
        }

        public Dictionary<string, string> GetSupportedVersions()
        {
            if (_supportedVersions == null)
            {
                _supportedVersions = _cliOptions.EnableDynamicInstall ?
                    _sdkStorageVersionProvider.GetSupportedVersions() :
                    _onDiskVersionProvider.GetSupportedVersions();
            }

            return _supportedVersions;
        }
    }
}