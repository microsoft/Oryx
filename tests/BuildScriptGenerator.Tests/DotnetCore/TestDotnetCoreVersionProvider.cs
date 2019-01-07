// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator.DotnetCore;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.DotnetCore
{
    internal class TestDotnetCoreVersionProvider : IDotnetCoreVersionProvider
    {
        public TestDotnetCoreVersionProvider(string[] supportedVersions)
        {
            SupportedVersions = supportedVersions;
        }

        public IEnumerable<string> SupportedVersions { get; }
    }
}
