// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    /// <summary>
    /// Build script template for NodeJs in Bash.
    /// </summary>
    public partial class NodeBashBuildScript
    {
        public NodeBashBuildScript(string packageInstallCommand, string benvArgs)
        {
            PackageInstallCommand = packageInstallCommand;
            BenvArgs = benvArgs;
        }

        public string PackageInstallCommand { get; set; }

        public string BenvArgs { get; set; }
    }
}