// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeBashRunScriptProperties
    {
        public string AppDirectory { get; set; }

        public int BindPort { get; set; }

        public string StartupCommand { get; set; }

        public string ToolsVersions { get; set; }
    }
}