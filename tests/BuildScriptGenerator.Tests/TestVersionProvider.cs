// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    class TestVersionProvider : BuildScriptGenerator.DotNetCore.IDotNetCoreVersionProvider
    {
        public TestVersionProvider(string[] supportedVersions)
        {
            SupportedDotNetCoreVersions = supportedVersions;
        }

        public IEnumerable<string> SupportedDotNetCoreVersions { get; }
    }
}