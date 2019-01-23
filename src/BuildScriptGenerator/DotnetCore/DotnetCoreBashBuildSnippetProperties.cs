// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.DotnetCore
{
    /// <summary>
    /// Build script template for DotnetCore in Bash.
    /// </summary>
    public partial class DotnetCoreBashBuildSnippet
    {
        public DotnetCoreBashBuildSnippet(
            string projectFile,
            string publishDirectory)
        {
            ProjectFile = projectFile;
            PublishDirectory = publishDirectory;
        }

        public string ProjectFile { get; set; }

        public string PublishDirectory { get; set; }
    }
}