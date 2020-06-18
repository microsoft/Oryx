// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector
{
    /// <summary>
    /// Options to create a detector.
    /// </summary>
    public class DetectorOptions
    {
        /// <summary>
        /// Gets or sets the source repo where the application is stored.
        /// </summary>
        public string SourceDir { get; set; }

        /// <summary>
        /// Output metatdata information of the detected platforms in JSON format.
        /// </summary>
        public bool OutputJson { get; set; }

        /// <summary>
        /// Gets or sets the relative path to the project to be built.
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// Gets or sets the arguments to be passed into the detector.
        /// </summary>
        public string[] PassThruArguments { get; set; }
    }
}