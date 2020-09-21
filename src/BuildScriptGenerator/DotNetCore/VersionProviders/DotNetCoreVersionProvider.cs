// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class DotNetCoreVersionProvider : IDotNetCoreVersionProvider
    {
        private readonly BuildScriptGeneratorOptions _cliOptions;
        private readonly DotNetCoreOnDiskVersionProvider _onDiskVersionProvider;
        private readonly DotNetCoreSdkStorageVersionProvider _sdkStorageVersionProvider;
        private readonly ILogger<DotNetCoreVersionProvider> _logger;
        private string _defaultRuntimeVersion;
        private Dictionary<string, string> _supportedVersions;

        public DotNetCoreVersionProvider(
            IOptions<BuildScriptGeneratorOptions> cliOptions,
            DotNetCoreOnDiskVersionProvider onDiskVersionProvider,
            DotNetCoreSdkStorageVersionProvider sdkStorageVersionProvider,
            ILogger<DotNetCoreVersionProvider> logger)
        {
            _cliOptions = cliOptions.Value;
            _onDiskVersionProvider = onDiskVersionProvider;
            _sdkStorageVersionProvider = sdkStorageVersionProvider;
            _logger = logger;
        }

        public string GetDefaultRuntimeVersion()
        {
            if (string.IsNullOrEmpty(_defaultRuntimeVersion))
            {
                _defaultRuntimeVersion = _cliOptions.EnableDynamicInstall ?
                    _sdkStorageVersionProvider.GetDefaultRuntimeVersion() :
                    _onDiskVersionProvider.GetDefaultRuntimeVersion();
            }

            _logger.LogDebug("Default runtime version is {defaultRuntimeVersion}", _defaultRuntimeVersion);

            return _defaultRuntimeVersion;
        }

        public Dictionary<string, string> GetSupportedVersions()
        {
            if (_supportedVersions == null)
            {
                _supportedVersions = _cliOptions.EnableDynamicInstall ?
                    _sdkStorageVersionProvider.GetSupportedVersions() :
                    _onDiskVersionProvider.GetSupportedVersions();

                // A temporary fix to make building netcoreapp1.0 versions using the 1.1 SDK
                // This SDK has 2 runtimes: 1.1.13 and 1.0.16
                _supportedVersions[DotNetCoreRunTimeVersions.NetCoreApp10] 
                = DotNetCoreSdkVersions.DotNetCore11SdkVersion;
            }

            _logger.LogDebug("Got the list of supported versions");

            return _supportedVersions;
        }
    }
}