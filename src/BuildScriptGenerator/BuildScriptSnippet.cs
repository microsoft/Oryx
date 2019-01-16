// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Snippet that can be used to build a particular language.
    /// </summary>
    public class BuildScriptSnippet
    {
        /// <summary>
        /// Gets or sets a list of tools and their versions required for the build script to run.
        /// </summary>
        public IDictionary<string, string> RequiredToolsVersion { get; set; }

        /// <summary>
        /// Gets or sets the build script snippet, written in bash.
        /// </summary>
        public string BashBuildScriptSnippet { get; set; }
    }
}