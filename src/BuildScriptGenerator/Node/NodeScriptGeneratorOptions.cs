// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    public class NodeScriptGeneratorOptions
    {
        public string NodeJsDefaultVersion { get; set; }

        public string NpmDefaultVersion { get; set; }

        public IEnumerable<string> SupportedNodeVersions { get; set; }

        public IEnumerable<string> SupportedNpmVersions { get; set; }
    }
}
