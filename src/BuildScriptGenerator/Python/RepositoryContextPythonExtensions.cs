// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public partial class RepositoryContext
    {
        /// <summary>
        /// Gets or sets a value indicating whether the detection and build of Python code
        /// in the repo should be enabled.
        /// Defaults to true.
        /// </summary>
        public bool EnablePython { get; set; } = true;

        /// <summary>
        /// Gets or sets the version of Python used in the repo.
        /// </summary>
        public string PythonVersion { get; set; }
    }
}
