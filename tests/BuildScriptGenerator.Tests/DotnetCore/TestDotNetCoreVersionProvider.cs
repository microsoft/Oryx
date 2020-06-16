// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.DotNetCore
{
    class TestDotNetCoreVersionProvider : IDotNetCoreVersionProvider
    {
        private readonly Dictionary<string, string> _supportedVersions;
        private readonly string _defaultVersion;

        public TestDotNetCoreVersionProvider(Dictionary<string, string> supportedVersions, string defaultVersion)
        {
            _supportedVersions = supportedVersions;
            _defaultVersion = defaultVersion;
        }

        public string GetDefaultRuntimeVersion()
        {
            return _defaultVersion;
        }

        public Dictionary<string, string> GetSupportedVersions()
        {
            return _supportedVersions;
        }
    }
}
