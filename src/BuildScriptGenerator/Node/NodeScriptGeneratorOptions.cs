// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    public class NodeScriptGeneratorOptions
    {
        /// <summary>
        /// Represents user provided version.
        /// </summary>
        public string NodeVersion { get; set; }

        /// <summary>
        /// Gets or sets the custom 'npm run build' command that is run after 'npm install' is run in the generated
        /// build script.
        /// </summary>
        public string CustomNpmRunBuildCommand { get; set; }
    }
}