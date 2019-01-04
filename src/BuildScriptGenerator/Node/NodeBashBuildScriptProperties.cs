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
        public NodeBashBuildScript(
            string preBuildScriptPath,
            string benvArgs,
            string packageInstallCommand,
            string runBuildCommand,
            string runBuildAzureCommand,
            string postBuildScriptPath)
        {
            BenvArgs = benvArgs;
            PreBuildScriptPath = preBuildScriptPath;
            PostBuildScriptPath = postBuildScriptPath;
            PackageInstallCommand = packageInstallCommand;
            NpmRunBuildCommand = runBuildCommand;
            NpmRunBuildAzureCommand = runBuildAzureCommand;
        }

        public string PreBuildScriptPath { get; set; }

        public string BenvArgs { get; set; }

        public string PostBuildScriptPath { get; set; }

        public string PackageInstallCommand { get; set; }

        public string NpmRunBuildCommand { get; set; }

        public string NpmRunBuildAzureCommand { get; set; }
    }
}