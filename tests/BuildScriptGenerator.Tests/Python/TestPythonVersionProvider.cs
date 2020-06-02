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

        public TestPythonVersionProvider(string[] supportedPythonVersions, string defaultVersion)
        {
            _supportedPythonVersions = supportedPythonVersions;
            _defaultVersion = defaultVersion;
        }

        public PlatformVersionInfo GetVersionInfo()
        {
            return PlatformVersionInfo.CreateOnDiskVersionInfo(_supportedPythonVersions, _defaultVersion);
        }
    }
}
