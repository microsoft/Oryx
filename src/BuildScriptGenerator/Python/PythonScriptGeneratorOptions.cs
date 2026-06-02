// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    public class PythonScriptGeneratorOptions
    {
        public bool EnableCollectStatic { get; set; }

        public string VirtualEnvironmentName { get; set; }

        public string PythonVersion { get; set; }

        public string DefaultVersion { get; set; }

        /// <summary>
        /// Gets or sets custom build command that will run instead of the default
        /// pip/poetry/uv install in the generated build script.
        /// </summary>
        public string CustomBuildCommand { get; set; }

        public string CustomRequirementsTxtPath { get; set; }
    }
}