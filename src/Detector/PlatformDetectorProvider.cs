// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.Detector.DotNetCore;
using Microsoft.Oryx.Detector.Node;
using Microsoft.Oryx.Detector.Php;
using Microsoft.Oryx.Detector.Python;


namespace Microsoft.Oryx.Detector

{
    public class PlatformDetectorProvider : IPlatformDetectorProvider
    {
        internal IDictionary<PlatformName, IPlatformDetector> _platformDetectors;
        private readonly ILogger<PlatformDetectorProvider> _logger;

        internal PlatformDetectorProvider(
            ILogger<PlatformDetectorProvider> logger,
            NodePlatformDetector nodePlatformDetector,
            PhpPlatformDetector phpPlatformDetector,
            PythonPlatformDetector pythonPlatformDetector,
            DotNetCorePlatformDetector dotNetCorePlatformDetector)
        {
            _logger = logger;
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
            platformDetector = null;
            if ( ! _platformDetectors.TryGetValue(platformName, out IPlatformDetector detector))
            {
                _logger.LogError(platformName + "Platform Detector was not found. ");
                return false;
            }

            platformDetector = detector;

            return true;
        }

    }
}
