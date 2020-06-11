// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Detector;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class DefaultAllPlatformDetector
    {
        private readonly IEnumerable<IPlatformDetector> _detectors;
        private readonly ILogger<DefaultAllPlatformDetector> _logger;

        public DefaultAllPlatformDetector(
            IEnumerable<IPlatformDetector> detectors,
            ILogger<DefaultAllPlatformDetector> logger)
        {
            _detectors = detectors;
            _logger = logger;
        }

        public IDictionary<PlatformName, string> GetAllDetectedPlatforms(RepositoryContext ctx)
        {
            IDictionary<PlatformName, string> detectionResults = null;
            foreach (IPlatformDetector detector in _detectors)
            {
                PlatformDetectorResult detectionResult = detector.Detect(ctx);
                if (detectionResult != null)
                {
                    detectionResults.Add(detector.DetectorPlatformName, detectionResult.PlatformVersion);
                }
            }

            return detectionResults;
        }
    }
}
