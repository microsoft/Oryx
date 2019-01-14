// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator.Node;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    class TestNodeVersionProvider : INodeVersionProvider
    {
        public TestNodeVersionProvider(string[] supportedNodeVersions, string[] supportedNpmVersions)
        {
            SupportedNodeVersions = supportedNodeVersions;
            SupportedNpmVersions = supportedNpmVersions;
        }

        public IEnumerable<string> SupportedNodeVersions { get; }

        public IEnumerable<string> SupportedNpmVersions { get; }
    }
}