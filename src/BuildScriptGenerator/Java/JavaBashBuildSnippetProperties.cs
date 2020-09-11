// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Java
{
    /// <summary>
    /// Build script template for JavaJs in Bash.
    /// </summary>
    internal class JavaBashBuildSnippetProperties
    {
        public bool UsesMaven { get; set; }

        public bool UsesMavenWrapperTool { get; set; }

        public string Command { get; set; }
    }
}