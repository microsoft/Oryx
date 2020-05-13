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
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class DefaultPlatformDetector : IDetector
    {
        private readonly IEnumerable<IProgrammingPlatform> _programmingPlatforms;
        private readonly ILogger<DefaultPlatformDetector> _logger;
        private readonly DetectorOptions _detectorOptions;
        private readonly IConfiguration _configuration;

        public DefaultPlatformDetector(
            IEnumerable<IProgrammingPlatform> programmingPlatforms,
            ILogger<DefaultPlatformDetector> logger,
            IOptions<DetectorOptions> detectorOptions,
            IConfiguration configuration)
        {
            _programmingPlatforms = programmingPlatforms;
            _logger = logger;
            _detectorOptions = detectorOptions.Value;
            _configuration = configuration;
        }

        public IDictionary<IProgrammingPlatform, string> GetAllDetectedPlatforms(RepositoryContext ctx)
        {
            var detectedPlatforms = new Dictionary<IProgrammingPlatform, string>();

            foreach (var platform in _programmingPlatforms)
            {
                _logger.LogDebug($"Detecting platform using '{platform.Name}'...");
                if (IsDetectedPlatform(ctx, platform.Name, out var platformResult))
                {
                    detectedPlatforms.Add(platformResult.Item1, platformResult.Item2);
                }
            }

            return detectedPlatforms;
        }

        private bool IsDetectedPlatform(
            RepositoryContext ctx,
            string platformName,
            out Tuple<IProgrammingPlatform, string> platformResult)
        {
            // logic here make sure:
            var selectedPlatform = _programmingPlatforms
                .FirstOrDefault(platform => string
                    .Equals(platformName, platform.Name, StringComparison.OrdinalIgnoreCase));

            if (selectedPlatform == null)
            {
                var languages = string.Join(", ", _programmingPlatforms.Select(platform => platform.Name));
                var exec = new UnsupportedPlatformException($"'{platformName}' platform is not supported. " +
                                                            $"Supported platforms are: {languages}");
                _logger.LogError(exec, $"Exception caught, provided platform '{platformName}' is not supported.");
                throw exec;
            }

            var detectionResult = selectedPlatform.Detect(ctx);
            platformResult = null;

            if (detectionResult == null) {
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

            platformResult = Tuple.Create(selectedPlatform, detectedVersion);
            _logger.LogDebug($"Detected platform '{platformName}' with version '{detectedVersion}'.");

            return true;
        }
    }
}