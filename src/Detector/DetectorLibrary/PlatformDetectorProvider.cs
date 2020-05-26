// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.DotNetCore;
using Microsoft.Oryx.Detector.Node;
using Microsoft.Oryx.Detector.Php;
using Microsoft.Oryx.Detector.Python;


namespace Microsoft.Oryx.Detector

{
    public class PlatformDetectorProvider : IPlatformDetectorProvider
    {
        internal IDictionary<PlatformName, IPlatformDetector> _platformDetectors;

        internal PlatformDetectorProvider(
            NodePlatformDetector nodePlatformDetector,
            PhpPlatformDetector phpPlatformDetector,
            PythonPlatformDetector pythonPlatformDetector,
            DotNetCorePlatformDetector dotNetCorePlatformDetector)
        {
            _platformDetectors = new Dictionary<PlatformName, IPlatformDetector>
            {
                { PlatformName.DotNetCore, dotNetCorePlatformDetector },
                { PlatformName.Php, phpPlatformDetector },
                { PlatformName.Python, pythonPlatformDetector },
                { PlatformName.Node, nodePlatformDetector }
            };

        }

        public IDictionary<PlatformName, IPlatformDetector> GetAllDetectors()
        {
            return _platformDetectors;
        }

        public bool TryGetDetector(PlatformName platformName, out IPlatformDetector platformDetector)
        {
            if ( ! _platformDetectors.TryGetValue(platformName, out IPlatformDetector detector))
            {
                throw new Exception(platformName + "Platform Detector not found. ");
            }

            platformDetector = detector;

            return true;
        }

    }
}
