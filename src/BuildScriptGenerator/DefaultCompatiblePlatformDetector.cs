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

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class DefaultCompatiblePlatformDetector : ICompatiblePlatformDetector
    {
        private readonly IEnumerable<IProgrammingPlatform> _programmingPlatforms;
        private readonly ILogger<DefaultCompatiblePlatformDetector> _logger;
        private readonly BuildScriptGeneratorOptions _commonOptions;

        public DefaultCompatiblePlatformDetector(
            IEnumerable<IProgrammingPlatform> programmingPlatforms,
            ILogger<DefaultCompatiblePlatformDetector> logger,
            IOptions<BuildScriptGeneratorOptions> commonOptions)
        {
            _programmingPlatforms = programmingPlatforms;
            _logger = logger;
            _commonOptions = commonOptions.Value;
        }

        /// <inheritdoc/>
        public IDictionary<IProgrammingPlatform, PlatformDetectorResult> GetCompatiblePlatforms(
            RepositoryContext ctx)
        {
            return GetCompatiblePlatforms(ctx, detectionResults: null, runDetection: true);
        }

        /// <inheritdoc/>
        public IDictionary<IProgrammingPlatform, PlatformDetectorResult> GetCompatiblePlatforms(
            RepositoryContext ctx,
            IEnumerable<PlatformDetectorResult> detectionResults)
        {
            return GetCompatiblePlatforms(ctx, detectionResults, runDetection: false);
        }

        private IDictionary<IProgrammingPlatform, PlatformDetectorResult> GetCompatiblePlatforms(
            RepositoryContext ctx,
            IEnumerable<PlatformDetectorResult> detectionResults,
            bool runDetection)
        {
            var userProvidedPlatformName = _commonOptions.PlatformName;

            var resultPlatforms = new Dictionary<IProgrammingPlatform, PlatformDetectorResult>();
            if (!string.IsNullOrEmpty(_commonOptions.PlatformName))
            {
                if (!IsCompatiblePlatform(
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

                if (!IsEnabledForMultiPlatformBuild(platformResult.Item1, ctx))
                {
                    return resultPlatforms;
                }
            }

            var enabledPlatforms = _programmingPlatforms.Where(platform =>
            {
                if (!platform.IsEnabled(ctx))
                {
                    _logger.LogDebug("{platformName} has been disabled.", platform.Name);
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

                _logger.LogDebug($"Detecting platform using '{platform.Name}'...");
                if (IsCompatiblePlatform(
                    ctx,
                    platform.Name,
                    detectionResults,
                    runDetection,
                    out var platformResult))
                {
                    resultPlatforms.Add(platformResult.Item1, platformResult.Item2);
                    if (!IsEnabledForMultiPlatformBuild(platform, ctx))
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
            var selectedPlatform = _programmingPlatforms
                                    .Where(p => string.Equals(platformName, p.Name, StringComparison.OrdinalIgnoreCase))
                                    .FirstOrDefault();
            if (selectedPlatform == null)
            {
                var platforms = string.Join(", ", _programmingPlatforms.Select(p => p.Name));
                var exec = new UnsupportedPlatformException($"'{platformName}' platform is not supported. " +
                    $"Supported platforms are: {platforms}");
                _logger.LogError(exec, $"Exception caught, provided platform '{platformName}' is not supported.");
                throw exec;
            }

            if (!selectedPlatform.IsEnabled(ctx))
            {
                var exc = new UnsupportedPlatformException($"Platform '{selectedPlatform.Name}' has been disabled.");
                _logger.LogError(exc, $"Exception caught, platform '{selectedPlatform.Name}' has been disabled.");
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
                _logger.LogError($"Platform '{platformName}' was not detected in the given repository.");
                return false;
            }
            else if (string.IsNullOrEmpty(detectionResult.PlatformVersion))
            {
                _logger.LogError($"Platform '{platformName}' was detected in the given repository, but " +
                                 $"no compatible version was found.");
                return false;
            }

            platformResult = Tuple.Create(selectedPlatform, detectionResult);
            _logger.LogDebug($"Detected platform '{platformName}' with version '{detectionResult.PlatformVersion}'.");

            return true;
        }

        private bool IsEnabledForMultiPlatformBuild(IProgrammingPlatform platform, RepositoryContext ctx)
        {
            if (_commonOptions.EnableMultiPlatformBuild)
            {
                return platform.IsEnabledForMultiPlatformBuild(ctx);
            }

            return false;
        }
    }
}
