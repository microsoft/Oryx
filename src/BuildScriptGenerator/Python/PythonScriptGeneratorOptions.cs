// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    public class PythonScriptGeneratorOptions
    {
        /// <summary>
        /// Represents a user provided version.
        /// </summary>
        public string PythonVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether "collectstatic" should be run when building Django apps.
        /// </summary>
        public bool DisableCollectStatic { get; set; }
    }
}