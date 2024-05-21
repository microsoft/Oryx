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

        public string CustomRequirementsTxtPath { get; set; }
    }
}