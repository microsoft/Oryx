// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    public class NodeScriptGeneratorOptions
    {
        public string NodeJsDefaultVersion { get; set; }

        public string NpmDefaultVersion { get; set; }

        public string InstalledNodeVersionsDir { get; set; }

        public string InstalledNpmVersionsDir { get; set; }
    }
}
