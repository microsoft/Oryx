// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Options to create a detector.
    /// </summary>
    public class DetectorOptions
    {
        /// <summary>
        /// Gets or sets the source repo where the application is stored.
        /// </summary>
        public ISourceRepo SourceRepo { get; set; }

        /// <summary>
        /// Gets or sets the arguments to be passed into the detector.
        /// </summary>
        public string[] PassThruArguments { get; set; }
    }
}