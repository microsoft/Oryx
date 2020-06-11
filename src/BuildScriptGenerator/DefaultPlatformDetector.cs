// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Detector;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class DefaultPlatformDetector
    {
        private readonly IEnumerable<IBuildScriptGenerationDetector> _detectors;
        private readonly ILogger<DefaultPlatformDetector> _logger;

        public DefaultPlatformDetector(
            IEnumerable<IBuildScriptGenerationDetector> detectors,
            ILogger<DefaultPlatformDetector> logger)
        {
            _detectors = detectors;
            _logger = logger;
        }

        public IDictionary<PlatformName, string> GetAllDetectedPlatformsAndResolveVersion(RepositoryContext ctx)
        {
            IDictionary<PlatformName, string> detectionResults = null;
            foreach (IBuildScriptGenerationDetector detector in _detectors)
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
