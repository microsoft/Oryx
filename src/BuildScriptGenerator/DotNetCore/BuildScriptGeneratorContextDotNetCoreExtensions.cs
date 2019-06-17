// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public partial class BuildScriptGeneratorContext
    {
        /// <summary>
        /// Gets or sets a value indicating whether the detection and build of .NET core
        /// code in the repo should be enabled.
        /// Defaults to true.
        /// </summary>
        public bool EnableDotNetCore { get; set; } = true;

        /// <summary>
        /// Gets or sets the version of .NET Core used in the repo.
        /// </summary>
        public string DotNetCoreVersion { get; set; }
    }
}
