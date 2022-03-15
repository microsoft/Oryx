// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector
{
    /// <summary>
    /// The information used by the detectors to detect applications.
    /// </summary>
    public class DetectorContext
    {
        /// <summary>
        /// Gets or sets the property which represents the source directory having the application(s).
        /// </summary>
        public ISourceRepo SourceRepo { get; set; }
    }
}
