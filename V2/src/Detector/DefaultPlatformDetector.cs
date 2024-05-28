// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.Detector
{
    /// <summary>
    /// The default implementation of <see cref="IDetector"/> which invokes the
    /// <see cref="IDetector.GetAllDetectedPlatforms(DetectorContext)"/> on each of the registered
    /// <see cref="IPlatformDetector"/> and returns back a list of <see cref="PlatformDetectorResult"/>.
    /// </summary>
    public class DefaultPlatformDetector : IDetector
    {
        private readonly IEnumerable<IPlatformDetector> platformDetectors;
        private readonly ILogger<DefaultPlatformDetector> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPlatformDetector"/> class.
        /// </summary>
        /// <param name="platformDetectors">List of <see cref="IPlatformDetector"/>.</param>
        /// <param name="logger">The <see cref="ILogger{DefaultPlatformDetector}"/>.</param>
        public DefaultPlatformDetector(
            IEnumerable<IPlatformDetector> platformDetectors,
            ILogger<DefaultPlatformDetector> logger)
        {
            this.platformDetectors = platformDetectors;
            this.logger = logger;
        }

        /// <inheritdoc />
        public IEnumerable<PlatformDetectorResult> GetAllDetectedPlatforms(DetectorContext context)
        {
            var detectedPlatforms = new List<PlatformDetectorResult>();

            foreach (var platformDetector in this.platformDetectors)
            {
                this.logger.LogDebug($"Detecting platform using '{platformDetector.GetType()}'...");

                if (this.IsDetectedPlatform(
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
                this.logger.LogInformation("Could not detect any platform in the given repository.");
                return false;
            }

            if (string.IsNullOrEmpty(platformResult.PlatformVersion))
            {
                this.logger.LogInformation(
                    $"Platform '{platformResult.Platform}' was detected in the given repository, " +
                    $"but no versions were detected.");
            }

            this.logger.LogInformation(
                $"Platform '{platformResult.Platform}' was detected with version '{platformResult.PlatformVersion}'.");
            return true;
        }
    }
}
