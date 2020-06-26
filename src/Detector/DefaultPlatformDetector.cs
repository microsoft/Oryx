// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.Detector
{
    public class DefaultPlatformDetector : IDetector
    {
        private readonly IEnumerable<IPlatformDetector> _platformDetectors;
        private readonly ILogger<DefaultPlatformDetector> _logger;
        private readonly IOptions<DetectorOptions> _detectorOptions;

        public DefaultPlatformDetector(
            IEnumerable<IPlatformDetector> platformDetectors,
            ILogger<DefaultPlatformDetector> logger,
            IOptions<DetectorOptions> detectorOptions)
        {
            _platformDetectors = platformDetectors;
            _logger = logger;
            _detectorOptions = detectorOptions;
        }

        public IDictionary<PlatformName, PlatformDetectorResult> GetAllDetectedPlatforms(DetectorContext context)
        {
            var detectedPlatforms = new Dictionary<PlatformName, PlatformDetectorResult>();

            foreach (var platformDetector in _platformDetectors)
            {
                PlatformName platformName = platformDetector.DetectorPlatformName;
                _logger.LogDebug($"Detecting '{platformName}' platform ...");
                if (IsDetectedPlatform(
                    context,
                    platformDetector,
                    out Tuple<PlatformName, PlatformDetectorResult> platformResult))
                {
                    detectedPlatforms.Add(platformResult.Item1, platformResult.Item2);
                }
            }

            return detectedPlatforms;
        }

        private bool IsDetectedPlatform(
            DetectorContext context,
            IPlatformDetector platformDetector,
            out Tuple<PlatformName, PlatformDetectorResult> platformResult)
        {
            platformResult = null;
            var platformName = platformDetector.DetectorPlatformName;
            var detectionResult = platformDetector.Detect(context);

            if (detectionResult == null)
            {
                _logger.LogInformation($"Platform '{platformName}' was not detected in the given repository.");
                return false;
            }

            if (string.IsNullOrEmpty(detectionResult.PlatformVersion))
            {
                _logger.LogInformation($"Platform '{platformName}' was detected in the given repository, but " +
                                        $"no versions were detected.");
            }

            platformResult = Tuple.Create(platformName, detectionResult);
            _logger.LogInformation(
                $"Platform '{platformName}' was detected with version '{detectionResult.PlatformVersion}'.");
            return true;
        }
    }
}