// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class DefaultCompatiblePlatformDetector : ICompatiblePlatformDetector
    {
        private readonly IEnumerable<IProgrammingPlatform> _programmingPlatforms;
        private readonly ILogger<DefaultCompatiblePlatformDetector> _logger;

        public DefaultCompatiblePlatformDetector(
            IEnumerable<IProgrammingPlatform> programmingPlatforms,
            ILogger<DefaultCompatiblePlatformDetector> logger)
        {
            _programmingPlatforms = programmingPlatforms;
            _logger = logger;
        }

        /// <inheritdoc/>
        public IDictionary<IProgrammingPlatform, string> GetCompatiblePlatforms(
            RepositoryContext ctx,
            string platformName = null,
            string platformVersion = null)
        {
            var resultPlatforms = new Dictionary<IProgrammingPlatform, string>();
            if (!string.IsNullOrEmpty(platformName))
            {
                if (IsCompatiblePlatform(ctx, platformName, platformVersion, out var platformResult))
                {
                    resultPlatforms.Add(platformResult.Item1, platformResult.Item2);
                    if (!IsEnabledForMultiPlatformBuild(platformResult.Item1, ctx))
                    {
                        return resultPlatforms;
                    }
                }
                else
                {
                    throw new UnsupportedVersionException(
                        $"Couldn't detect a version for the platform '{platformName}' in the repo.");
                }
            }

            var enabledPlatforms = _programmingPlatforms.Where(p =>
            {
                if (!p.IsEnabled(ctx))
                {
                    _logger.LogDebug("{platformName} has been disabled.", p.Name);
                    return false;
                }

                return true;
            });

            foreach (var platform in enabledPlatforms)
            {
                // If the user provided a platform name, it has already been processed, so skip processing again
                if (!string.IsNullOrEmpty(platformName) &&
                     string.Equals(platform.Name, platformName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                _logger.LogDebug($"Detecting platform using '{platform.Name}'...");
                if (IsCompatiblePlatform(ctx, platform.Name, out var platformResult))
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
            out Tuple<IProgrammingPlatform, string> platformResult)
        {
            return IsCompatiblePlatform(ctx, platformName, null, out platformResult);
        }

        private bool IsCompatiblePlatform(
            RepositoryContext ctx,
            string platformName,
            string platformVersion,
            out Tuple<IProgrammingPlatform, string> platformResult)
        {
            platformResult = null;
            var selectedPlatform = _programmingPlatforms
                                    .Where(p => string.Equals(platformName, p.Name, StringComparison.OrdinalIgnoreCase))
                                    .FirstOrDefault();
            if (selectedPlatform == null)
            {
                var languages = string.Join(", ", _programmingPlatforms.Select(p => p.Name));
                var exec = new UnsupportedPlatformException($"'{platformName}' platform is not supported. " +
                    $"Supported platforms are: {languages}");
                _logger.LogError(exec, $"Exception caught, provided platform '{platformName}' is not supported.");
                throw exec;
            }

            if (!selectedPlatform.IsEnabled(ctx))
            {
                var exc = new UnsupportedPlatformException($"Platform '{selectedPlatform.Name}' has been disabled.");
                _logger.LogError(exc, $"Exception caught, platform '{selectedPlatform.Name}' has been disabled.");
                throw exc;
            }

            if (string.IsNullOrEmpty(platformVersion))
            {
                var detectionResult = selectedPlatform.Detect(ctx);
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

                _logger.LogDebug($"No platform version found, " +
                                 $"setting to the detected version '{detectionResult.PlatformVersion}'.");
                platformVersion = detectionResult.PlatformVersion;
            }

            _logger.LogDebug($"Detected platform '{platformName}' with version '{platformVersion}'.");
            platformResult = Tuple.Create(selectedPlatform, platformVersion);
            return true;
        }

        private bool IsEnabledForMultiPlatformBuild(IProgrammingPlatform platform, RepositoryContext ctx)
        {
            if (ctx.DisableMultiPlatformBuild)
            {
                return false;
            }

            return platform.IsEnabledForMultiPlatformBuild(ctx);
        }
    }
}
