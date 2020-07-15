// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector
{
    /// <summary>
    /// Settings used by the detectors when detecting applications.
    /// </summary>
    public class DetectorOptions
    {
        /// <summary>
        /// Gets or sets the relative path to the project file to be built.
        /// </summary>
        public string Project { get; set; }
    }
}