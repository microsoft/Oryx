// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public partial class RepositoryContext
    {
        /// <summary>
        /// Gets or sets a value indicating whether the detection and build of Node.js code
        /// in the repo should be enabled.
        /// Defaults to true.
        /// </summary>
        public bool EnableNodeJs { get; set; } = true;

        /// <summary>
        /// Gets or sets the version of Node used in the repo.
        /// </summary>
        public string NodeVersion { get; set; }
    }
}
