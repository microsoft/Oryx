// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Options to create the entrypoint scipt.
    /// </summary>
    public class RunScriptGeneratorOptions
    {
        /// <summary>
        /// Gets or sets the source repo where the application is stored.
        /// </summary>
        public ISourceRepo SourceRepo { get; set; }

        /// <summary>
        /// Gets or sets the platform version the application expects.
        /// </summary>
        public string PlatformVersion { get; set; }

        /// <summary>
        /// Gets or sets the arguments to be passed into the run script generator.
        /// </summary>
        public string[] PassThruArguments { get; set; }
    }
}