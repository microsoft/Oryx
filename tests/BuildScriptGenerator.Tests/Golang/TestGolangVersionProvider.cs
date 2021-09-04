// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Golang;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Golang
{
    class TestGolangVersionProvider : IGolangVersionProvider
    {
        private readonly string[] _supportedGoVersions;
        private readonly string _defaultVersion;

        public TestGolangVersionProvider(string[] supportedGoVersions, string defaultVersion)
        {
            _supportedGoVersions = supportedGoVersions;
            _defaultVersion = defaultVersion;
        }

        public PlatformVersionInfo GetVersionInfo()
        {
            return PlatformVersionInfo.CreateOnDiskVersionInfo(_supportedGoVersions, _defaultVersion);
        }
    }
}
