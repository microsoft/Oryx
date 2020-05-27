// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.Detector
{
    internal interface IPlatformDetectorProvider
    {
        bool TryGetDetector(PlatformName platformName, out IPlatformDetector platformDetector);

        IDictionary<PlatformName, IPlatformDetector> GetAllDetectors();
    }
}
