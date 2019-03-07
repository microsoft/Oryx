// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    class TestVersionProvider :
        BuildScriptGenerator.Node.INodeVersionProvider,
        BuildScriptGenerator.Python.IPythonVersionProvider,
        BuildScriptGenerator.DotNetCore.IDotnetCoreVersionProvider
    {
        public TestVersionProvider(string[] supportedVersions, string[] supportedNpmVersions = null)
        {
            SupportedNodeVersions = SupportedPythonVersions = SupportedDotNetCoreVersions = supportedVersions;
            SupportedNpmVersions = supportedNpmVersions;
        }

        public IEnumerable<string> SupportedNodeVersions { get; }

        public IEnumerable<string> SupportedNpmVersions { get; }

        public IEnumerable<string> SupportedPythonVersions { get; }

        public IEnumerable<string> SupportedDotNetCoreVersions { get; }
    }
}