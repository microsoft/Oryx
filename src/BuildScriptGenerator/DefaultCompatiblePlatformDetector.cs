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
    public class DefaultCompatiblePlatformDetector : ICompatiblePlatformDetector
    {
        private readonly IEnumerable<IProgrammingPlatform> _programmingPlatforms;
        private readonly ILogger<DefaultCompatiblePlatformDetector> _logger;
        private readonly BuildScriptGeneratorOptions _commonOptions;
        private readonly IConfiguration _configuration;

        public DefaultCompatiblePlatformDetector(
            IEnumerable<IProgrammingPlatform> programmingPlatforms,
            ILogger<DefaultCompatiblePlatformDetector> logger,
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IConfiguration configuration)
        {
            _programmingPlatforms = programmingPlatforms;
            _logger = logger;
            _commonOptions = commonOptions.Value;
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public IDictionary<IProgrammingPlatform, string> GetCompatiblePlatforms(RepositoryContext ctx)
        {
            var userProvidedPlatformName = _commonOptions.PlatformName;

            var resultPlatforms = new Dictionary<IProgrammingPlatform, string>();
            if (!string.IsNullOrEmpty(_commonOptions.PlatformName))
            {
                if (!IsCompatiblePlatform(ctx, userProvidedPlatformName, out var platformResult))
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

            var platformVersionFromOptions = GetPlatformVersion(platformName);

            string detectedPlatformVersion = null;
            if (string.IsNullOrEmpty(platformVersionFromOptions))
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
                detectedPlatformVersion = detectionResult.LanguageVersion;

                platformResult = Tuple.Create(selectedPlatform, detectedPlatformVersion);
                _logger.LogDebug($"Detected platform '{platformName}' with version '{detectedPlatformVersion}'.");
            }
            else
            {
                platformResult = Tuple.Create(selectedPlatform, platformVersionFromOptions);
            }

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

        /// <summary>
        /// Gets the platform version in a hierarchical fasion
        /// 1. --platform nodejs --platform-version 4.0
        /// 2. NODE_VERSION=4.0 from environment variables
        /// 3. NODE_VERSION=4.0 from build.env file
        /// </summary>
        /// <param name="platformName">Platform for which we want to get the version in a hierarchical way.</param>
        /// <returns></returns>
        private string GetPlatformVersion(string platformName)
        {
            platformName = platformName == "nodejs" ? "node" : platformName;

            return _configuration[$"{platformName}_version"];
        }
    }
}
