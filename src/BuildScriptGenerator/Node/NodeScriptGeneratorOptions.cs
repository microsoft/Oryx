// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    public class NodeScriptGeneratorOptions
    {
        /// <summary>
        /// Gets or sets the custom 'npm run build' command that is run after 'npm install' is run in the generated
        /// build script.
        /// </summary>
        public string CustomRunBuildCommand { get; set; }

        /// <summary>
        /// Gets or sets custom build command or script that will run without 'npm install' in the generated
        /// build script.
        /// </summary>
        public string CustomBuildCommand { get; set; }

        public bool PruneDevDependencies { get; set; }

        public string NpmRegistryUrl { get; set; }

        public string NodeVersion { get; set; }

        public string DefaultVersion { get; set; }

        public bool EnableNodeMonorepoBuild { get; set; }

        public string YarnTimeOutConfig { get; set; }
    }
}