// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    public class PythonScriptGeneratorOptions
    {
        public string PythonDefaultVersion { get; set; }

        public string InstalledPythonVersionsDir { get; set; }

        /// <summary>
        /// Gets or sets the user-provided list of python versions.
        /// </summary>
        public IList<string> SupportedPythonVersions { get; set; }
    }
}