// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Ruby;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Ruby
{
    class TestRubyVersionProvider : IRubyVersionProvider
    {
        private readonly string[] _supportedRubyVersions;
        private readonly string _defaultVersion;

        public TestRubyVersionProvider(string[] supportedRubyVersions, string defaultVersion)
        {
            _supportedRubyVersions = supportedRubyVersions;
            _defaultVersion = defaultVersion;
        }

        public PlatformVersionInfo GetVersionInfo()
        {
            return PlatformVersionInfo.CreateOnDiskVersionInfo(_supportedRubyVersions, _defaultVersion);
        }
    }
}
