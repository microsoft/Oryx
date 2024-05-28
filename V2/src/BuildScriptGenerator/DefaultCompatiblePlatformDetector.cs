// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Common.Extensions;
using Microsoft.Oryx.Detector;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class DefaultCompatiblePlatformDetector : ICompatiblePlatformDetector
    {
        private readonly IEnumerable<IProgrammingPlatform> programmingPlatforms;
        private readonly ILogger<DefaultCompatiblePlatformDetector> logger;
        private readonly BuildScriptGeneratorOptions commonOptions;

        public DefaultCompatiblePlatformDetector(
            IEnumerable<IProgrammingPlatform> programmingPlatforms,
            ILogger<DefaultCompatiblePlatformDetector> logger,
            IOptions<BuildScriptGeneratorOptions> commonOptions)
        {
            this.programmingPlatforms = programmingPlatforms;
            this.logger = logger;
            this.commonOptions = commonOptions.Value;
        }

        /// <inheritdoc/>
        public IDictionary<IProgrammingPlatform, PlatformDetectorResult> GetCompatiblePlatforms(
            RepositoryContext ctx)
        {
            return this.GetCompatiblePlatforms(ctx, detectionResults: null, runDetection: true);
        }

        /// <inheritdoc/>
        public IDictionary<IProgrammingPlatform, PlatformDetectorResult> GetCompatiblePlatforms(
            RepositoryContext ctx,
            IEnumerable<PlatformDetectorResult> detectionResults)
        {
            return this.GetCompatiblePlatforms(ctx, detectionResults, runDetection: false);
        }

        private IDictionary<IProgrammingPlatform, PlatformDetectorResult> GetCompatiblePlatforms(
            RepositoryContext ctx,
            IEnumerable<PlatformDetectorResult> detectionResults,
            bool runDetection)
        {
            var userProvidedPlatformName = this.commonOptions.PlatformName;

            var resultPlatforms = new Dictionary<IProgrammingPlatform, PlatformDetectorResult>();
            if (!string.IsNullOrEmpty(this.commonOptions.PlatformName))
            {
                if (!this.IsCompatiblePlatform(
                    ctx,
                    userProvidedPlatformName,
                    detectionResults,
                    runDetection,
                    out var platformResult))
                {
                    throw new UnsupportedVersionException(
                        $"Couldn't detect a version for the platform '{userProvidedPlatformName}' in the repo.");
                }

                resultPlatforms.Add(platformResult.Item1, platformResult.Item2);

                if (!this.IsEnabledForMultiPlatformBuild(platformResult.Item1, ctx))
                {
                    return resultPlatforms;
                }
            }

            var enabledPlatforms = this.programmingPlatforms.Where(platform =>
            {
                if (!platform.IsEnabled(ctx))
                {
                    this.logger.LogDebug("{platformName} has been disabled.", platform.Name);
                    return false;
                }

                return true;
            });

            foreach (var platform in enabledPlatforms)
            {
                // If the user provided a platform name, it has already been processed, so skip processing again
                if (!string.IsNullOrEmpty(userProvidedPlatformName) &&
                     string.Equals(platform.Name, userProvidedPlatformName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                this.logger.LogDebug($"Detecting platform using '{platform.Name}'...");
                if (this.IsCompatiblePlatform(
                    ctx,
                    platform.Name,
                    detectionResults,
                    runDetection,
                    out var platformResult))
                {
                    resultPlatforms.Add(platformResult.Item1, platformResult.Item2);
                    if (!this.IsEnabledForMultiPlatformBuild(platform, ctx))
                    {
                        return resultPlatforms;
                    }
                }
            }

            return resultPlatforms;
        }

        private bool IsCompatiblePlatform(
            RepositoryContext ctx,
            string platformName,
            IEnumerable<PlatformDetectorResult> detectionResults,
            bool runDetection,
            out Tuple<IProgrammingPlatform, PlatformDetectorResult> platformResult)
        {
            platformResult = null;
            var selectedPlatform = this.programmingPlatforms
                                    .Where(p => string.Equals(platformName, p.Name, StringComparison.OrdinalIgnoreCase))
                                    .FirstOrDefault();
            if (selectedPlatform == null)
            {
                var platforms = string.Join(", ", this.programmingPlatforms.Select(p => p.Name));
                var exec = new UnsupportedPlatformException($"'{platformName}' platform is not supported. " +
                    $"Supported platforms are: {platforms}");
                this.logger.LogError(exec, $"Exception caught, provided platform '{platformName}' is not supported.");
                throw exec;
            }

            if (!selectedPlatform.IsEnabled(ctx))
            {
                var exc = new UnsupportedPlatformException($"Platform '{selectedPlatform.Name}' has been disabled.");
                this.logger.LogError(exc, $"Exception caught, platform '{selectedPlatform.Name}' has been disabled.");
                throw exc;
            }

            PlatformDetectorResult detectionResult = null;
            if (runDetection)
            {
                detectionResult = selectedPlatform.Detect(ctx);
            }
            else
            {
                if (detectionResults != null)
                {
                    detectionResult = detectionResults
                        .Where(result => result.Platform.EqualsIgnoreCase(selectedPlatform.Name))
                        .FirstOrDefault();
                }
            }

            if (detectionResult == null)
            {
                this.logger.LogError($"Platform '{platformName}' was not detected in the given repository.");
                return false;
            }
            else if (string.IsNullOrEmpty(detectionResult.PlatformVersion))
            {
                this.logger.LogError($"Platform '{platformName}' was detected in the given repository, but " +
                                 $"no compatible version was found.");
                return false;
            }

            platformResult = Tuple.Create(selectedPlatform, detectionResult);
            this.logger.LogDebug($"Detected platform '{platformName}' with version '{detectionResult.PlatformVersion}'.");

            return true;
        }

        private bool IsEnabledForMultiPlatformBuild(IProgrammingPlatform platform, RepositoryContext ctx)
        {
            if (this.commonOptions.EnableMultiPlatformBuild)
            {
                return platform.IsEnabledForMultiPlatformBuild(ctx);
            }

            return false;
        }
    }
}
