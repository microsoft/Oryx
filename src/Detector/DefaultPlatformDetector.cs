// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.Detector
{
    public class DefaultPlatformDetector : IDetector
    {
        private readonly IEnumerable<IPlatformDetector> _platformDetectors;
        private readonly ILogger<DefaultPlatformDetector> _logger;

        public DefaultPlatformDetector(
            IEnumerable<IPlatformDetector> platformDetectors,
            ILogger<DefaultPlatformDetector> logger)
        {
            _platformDetectors = platformDetectors;
            _logger = logger;
        }

        public IEnumerable<PlatformDetectorResult> GetAllDetectedPlatforms(DetectorContext context)
        {
            var detectedPlatforms = new List<PlatformDetectorResult>();

            foreach (var platformDetector in _platformDetectors)
            {
                _logger.LogDebug($"Detecting platform using '{platformDetector.GetType()}'...");

                if (IsDetectedPlatform(
                    context,
                    platformDetector,
                    out PlatformDetectorResult platformResult))
                {
                    detectedPlatforms.Add(platformResult);
                }
            }

            return detectedPlatforms;
        }

        private bool IsDetectedPlatform(
            DetectorContext context,
            IPlatformDetector platformDetector,
            out PlatformDetectorResult platformResult)
        {
            platformResult = platformDetector.Detect(context);

            if (platformResult == null)
            {
                _logger.LogInformation(
                    $"Platform '{platformResult.Platform}' was not detected in the given repository.");
                return false;
            }

            if (string.IsNullOrEmpty(platformResult.PlatformVersion))
            {
                _logger.LogInformation(
                    $"Platform '{platformResult.Platform}' was detected in the given repository, " +
                    $"but no versions were detected.");
            }

            _logger.LogInformation(
                $"Platform '{platformResult.Platform}' was detected with version '{platformResult.PlatformVersion}'.");
            return true;
        }
    }
}