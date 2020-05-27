// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.Detector
{
    internal interface IPlatformDetectorProvider
    {
        /// <summary>
        /// Returns if the user sucessfully gets the detector of supplied platform name.
        /// </summary>
        /// <param name="platformName">The platform name. </param>
        bool TryGetDetector(PlatformName platformName, out IPlatformDetector platformDetector);

        /// <summary>
        /// Returns a dictionary containing all platform names as keys
        /// and platform detectors as values.
        /// </summary>
        IDictionary<PlatformName, IPlatformDetector> GetAllDetectors();
    }
}
