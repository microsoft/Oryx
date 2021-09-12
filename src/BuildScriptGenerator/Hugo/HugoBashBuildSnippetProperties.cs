// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.Hugo
{
    /// <summary>
    /// Build script template for Hugo in Bash.
    /// </summary>
    public class HugoBashBuildSnippetProperties
    {
        public string CustomRunBuildCommand { get; set; }

        /// <summary>
        /// Gets or sets a list of commands for the build.
        /// </summary>
        public IDictionary<string, string> HugoBuildProperties { get; set; }

    }
}