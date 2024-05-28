// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector
{
    /// <summary>
    /// An abstraction to detect a platform and additional information of the application in the source directory.
    /// </summary>
    public interface IPlatformDetector
    {
        /// <summary>
        /// Detects platform of an application in a source directory.
        /// </summary>
        /// <param name="context">The <see cref="DetectorContext"/>.</param>
        /// <returns>An instance of <see cref="PlatformDetectorResult"/> if detection was
        /// successful, <c>null</c> otherwise.</returns>
        PlatformDetectorResult Detect(DetectorContext context);
    }
}
