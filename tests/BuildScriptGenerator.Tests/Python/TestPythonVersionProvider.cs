// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Python;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Python
{
    class TestPythonVersionProvider : IPythonVersionProvider
    {
        private readonly string[] _supportedPythonVersions;
        private readonly string _defaultVersion;
        private readonly PlatformVersionSourceType _sourceType;

        public TestPythonVersionProvider(string[] supportedPythonVersions, string defaultVersion)
        {
            _supportedPythonVersions = supportedPythonVersions;
            _defaultVersion = defaultVersion;
            _sourceType = PlatformVersionSourceType.OnDisk;
        }

        public TestPythonVersionProvider(
            string[] supportedPythonVersions,
            string defaultVersion,
            PlatformVersionSourceType sourceType)
        {
            _supportedPythonVersions = supportedPythonVersions;
            _defaultVersion = defaultVersion;
            _sourceType = sourceType;
        }

        public PlatformVersionInfo GetVersionInfo()
        {
            return _sourceType == PlatformVersionSourceType.AvailableViaExternalAcrProvider
                ? PlatformVersionInfo.CreateAvailableViaExternalAcrProvider(_supportedPythonVersions, _defaultVersion)
                : PlatformVersionInfo.CreateOnDiskVersionInfo(_supportedPythonVersions, _defaultVersion);
        }
    }
}
