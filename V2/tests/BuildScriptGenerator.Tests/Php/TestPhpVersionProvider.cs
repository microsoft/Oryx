// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Php
{
    class TestPhpVersionProvider : IPhpVersionProvider
    {
        private readonly string[] _supportedPhpVersions;
        private readonly string _defaultVersion;

        public TestPhpVersionProvider(string[] supportedPhpVersions, string defaultVersion)
        {
            _supportedPhpVersions = supportedPhpVersions;
            _defaultVersion = defaultVersion;
        }

        public PlatformVersionInfo GetVersionInfo()
        {
            var version = _defaultVersion;
            if (version == null)
            {
                version = PhpVersions.Php73Version;
            }

            return PlatformVersionInfo.CreateOnDiskVersionInfo(_supportedPhpVersions, version);
        }
    }
}
