using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Oryx.Detector
{
    internal interface IPlatformDetectorProvider
    {
        bool TryGetDetector(PlatformName platformName, out IPlatformDetector platformDetector);

        IDictionary<PlatformName, IPlatformDetector> GetAllDetectors();
    }
}
