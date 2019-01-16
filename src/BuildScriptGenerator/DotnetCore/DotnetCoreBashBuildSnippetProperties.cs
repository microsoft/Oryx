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
            string publishDirectory)
        {
            PublishDirectory = publishDirectory;
        }

        public string PublishDirectory { get; set; }
    }
}