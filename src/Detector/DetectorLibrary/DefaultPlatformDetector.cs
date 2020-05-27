// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.Detector
{
    internal class DefaultPlatformDetector : IDetector
    {
        private readonly IPlatformDetectorProvider _platformDetectorProvider;
        private readonly ILogger<DefaultPlatformDetector> _logger;
        private readonly IOptions<DetectorOptions> _detectorOptions;

        public DefaultPlatformDetector(
            IPlatformDetectorProvider platformDetectors,
            ILogger<DefaultPlatformDetector> logger,
            IOptions<DetectorOptions> detectorOptions)
        {
            _platformDetectorProvider = platformDetectors;
            _logger = logger;
            _detectorOptions = detectorOptions;
        }

        public IDictionary<PlatformName, string> GetAllDetectedPlatforms(RepositoryContext ctx)
        {
            var detectedPlatforms = new Dictionary<PlatformName, string>();

            foreach (PlatformName platformName in Enum.GetValues(typeof(PlatformName)))
            {
                _logger.LogDebug($"Detecting '{platformName}' platform ...");
                if (IsDetectedPlatform(ctx, platformName, out var platformResult))
                {
                    detectedPlatforms.Add(platformResult.Item1, platformResult.Item2);
                }
            }

            return detectedPlatforms;
        }

        private bool IsDetectedPlatform(
            RepositoryContext ctx,
            PlatformName platformName,
            out Tuple<PlatformName, string> platformResult)
        {
            platformResult = null;
            if (_platformDetectorProvider.TryGetDetector(platformName, out IPlatformDetector platformDetector))
            {
                PlatformDetectorResult detectionResult = platformDetector.Detect(ctx);
                
                if (detectionResult == null)
                {
                    _logger.LogInformation($"Platform '{platformName}' was not detected in the given repository.");
                    return false;
                }
                else if (string.IsNullOrEmpty(detectionResult.PlatformVersion))
                {
                    _logger.LogInformation($"Platform '{platformName}' was detected in the given repository, but " +
                                         $"no versions were detected.");
                    return false;
                }

                var detectedVersion = detectionResult.PlatformVersion;

                platformResult = Tuple.Create(platformName, detectedVersion);
                _logger.LogInformation($"platform '{platformName}' was detected with version '{detectedVersion}'.");
            }

            return true;
        }
    }
}