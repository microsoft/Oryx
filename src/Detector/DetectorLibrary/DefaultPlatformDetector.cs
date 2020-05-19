// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Detector.Exceptions;

namespace Microsoft.Oryx.Detector
{
    public class DefaultPlatformDetector : IDetector
    {
        private readonly Dictionary<PlatformName, IPlatformDetector> _platformDetectors;
        private readonly ILogger<DefaultPlatformDetector> _logger;
        private readonly IOptions<DetectorOptions> _detectorOptions;
        private readonly IConfiguration _configuration;

        public DefaultPlatformDetector(
            Dictionary<PlatformName, IPlatformDetector> platformDetectors,
            ILogger<DefaultPlatformDetector> logger,
            IOptions<DetectorOptions> detectorOptions,
            IConfiguration configuration)
        {
            _platformDetectors = platformDetectors;
            _logger = logger;
            _detectorOptions = detectorOptions;
            _configuration = configuration;
        }

        public IDictionary<PlatformName, string> GetAllDetectedPlatforms(RepositoryContext ctx)
        {
            var detectedPlatforms = new Dictionary<PlatformName, string>();

            foreach (KeyValuePair<PlatformName, IPlatformDetector> platformDetector in _platformDetectors)
            {
                _logger.LogDebug($"Detecting platform using '{platformDetector.Key}'...");
                if (IsDetectedPlatform(ctx, platformDetector.Key, out var platformResult))
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

            if (_platformDetectors.TryGetValue(platformName, out IPlatformDetector platformDetector))
            {
                PlatformDetectorResult detectionResult = platformDetector.Detect(ctx);
                platformResult = null;

                if (detectionResult == null)
                {
                    _logger.LogError($"Platform '{platformName}' was not detected in the given repository.");
                    return false;
                }
                else if (string.IsNullOrEmpty(detectionResult.PlatformVersion))
                {
                    _logger.LogError($"Platform '{platformName}' was detected in the given repository, but " +
                                         $"no such version was found.");
                    return false;
                }

                var detectedVersion = detectionResult.PlatformVersion;

                platformResult = Tuple.Create(platformName, detectedVersion);
                _logger.LogDebug($"Detected platform '{platformName}' with version '{detectedVersion}'.");
            }
            else
            {
                string languages = string.Join(", ", Enum.GetValues(typeof(PlatformName)));
                var exec = new UnsupportedPlatformException($"'{platformName}' platform is not supported. " +
                                                            $"Supported platforms are: {languages}");
                _logger.LogError(exec, $"Exception caught, provided platform '{platformName}' is not supported.");
                throw exec;
            }

            return true;
        }
    }
}