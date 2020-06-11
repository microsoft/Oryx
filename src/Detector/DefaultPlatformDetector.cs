// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common;

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

        public IDictionary<PlatformName, string> GetAllDetectedPlatforms(RepositoryContext ctx)
        {
            var detectedPlatforms = new Dictionary<PlatformName, string>();

            foreach (var platformDetector in _platformDetectors)
            {
                PlatformName platformName = platformDetector.DetectorPlatformName;
                _logger.LogDebug($"Detecting '{platformName}' platform ...");
                if (IsDetectedPlatform(ctx, platformDetector, out Tuple<PlatformName, string> platformResult))
                {
                    detectedPlatforms.Add(platformResult.Item1, platformResult.Item2);
                }
            }

            return detectedPlatforms;
        }

        private bool IsDetectedPlatform(
            RepositoryContext ctx,
            IPlatformDetector platformDetector,
            out Tuple<PlatformName, string> platformResult)
        {
            platformResult = null;
            PlatformName platformName = platformDetector.DetectorPlatformName;
            PlatformDetectorResult detectionResult = platformDetector.Detect(ctx);
                
            if (detectionResult == null)
            {
                _logger.LogInformation($"Platform '{platformName}' was not detected in the given repository.");
                return false;
            }
            
            if (string.IsNullOrEmpty(detectionResult.PlatformVersion))
            {
                _logger.LogInformation($"Platform '{platformName}' was detected in the given repository, but " +
                                        $"no versions were detected.");
                platformResult = Tuple.Create(platformName, "Not Detected");
                return true;
            }

            string detectedVersion = detectionResult.PlatformVersion;

            platformResult = Tuple.Create(platformName, detectedVersion);
            _logger.LogInformation($"platform '{platformName}' was detected with version '{detectedVersion}'.");
            return true;
        }
    }
}