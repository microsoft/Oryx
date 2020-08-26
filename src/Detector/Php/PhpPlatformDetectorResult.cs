// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.Detector.Php
{
    /// <summary>
    /// Represents a <see cref="PlatformDetectorResult"/> returned by the <see cref="PhpDetector"/> and
    /// contains additional information related to the detected applications.
    /// </summary>
    public class PhpPlatformDetectorResult : PlatformDetectorResult
    {
        /// <summary>
        /// Gets or sets the version of php-composer that was detected.
        /// </summary>
        public string PhpComposerVersion { get; set; }
    }
}
