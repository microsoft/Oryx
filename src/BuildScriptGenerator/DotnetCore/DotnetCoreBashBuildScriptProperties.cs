// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.DotnetCore
{
    /// <summary>
    /// Build script template for DotnetCore in Bash.
    /// </summary>
    public partial class DotnetCoreBashBuildScript
    {
        public DotnetCoreBashBuildScript(
            string preBuildScriptPath,
            string benvArgs,
            string publishDirectory,
            string postBuildScriptPath)
        {
            PreBuildScriptPath = preBuildScriptPath;
            BenvArgs = benvArgs;
            PublishDirectory = publishDirectory;
            PostBuildScriptPath = postBuildScriptPath;
        }

        public string PreBuildScriptPath { get; set; }

        public string BenvArgs { get; set; }

        public string PublishDirectory { get; set; }

        public string PostBuildScriptPath { get; set; }
    }
}