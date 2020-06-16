// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Resources;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal class DefaultDockerfileGenerator : IDockerfileGenerator
    {
        private readonly ICompatiblePlatformDetector _platformDetector;
        private readonly ILogger<DefaultDockerfileGenerator> _logger;
        private readonly BuildScriptGeneratorOptions _commonOptions;
        private readonly IDictionary<string, IList<string>> _slimPlatformVersions =
            new Dictionary<string, IList<string>>()
            {
                { "dotnet", new List<string>() { "2.1" } },
                { "nodejs",   new List<string>() { "8", "10", "12" } },
                { "python", new List<string>() { "3.7", "3.8" } },
            };

        public DefaultDockerfileGenerator(
            ICompatiblePlatformDetector platformDetector,
            ILogger<DefaultDockerfileGenerator> logger,
            IOptions<BuildScriptGeneratorOptions> commonOptions)
        {
            _platformDetector = platformDetector;
            _logger = logger;
            _commonOptions = commonOptions.Value;
        }

        public string GenerateDockerfile(DockerfileContext ctx)
        {
            var buildImageTag = "lts-versions";
            var runImage = string.Empty;
            var runImageTag = string.Empty;
            var compatiblePlatforms = GetCompatiblePlatforms(ctx);
            if (!compatiblePlatforms.Any())
            {
                throw new UnsupportedPlatformException(Labels.UnableToDetectPlatformMessage);
            }

            foreach (var platformAndDetectorResult in compatiblePlatforms)
            {
                var platform = platformAndDetectorResult.Key;
                var detectorResult = platformAndDetectorResult.Value;
                if (!_slimPlatformVersions.ContainsKey(platform.Name) ||
                    (!_slimPlatformVersions[platform.Name].Any(v => detectorResult.PlatformVersion.StartsWith(v)) &&
                     !_slimPlatformVersions[platform.Name].Any(v => v.StartsWith(detectorResult.PlatformVersion))))
                {
                    buildImageTag = "latest";
                    runImageTag = GenerateRuntimeTag(detectorResult.PlatformVersion);
                }
                else
                {
                    runImageTag = _slimPlatformVersions[platform.Name]
                        .Where(v => detectorResult.PlatformVersion.StartsWith(v)).FirstOrDefault();
                }

                runImage = ConvertToRuntimeName(platform.Name);
            }

            var properties = new DockerfileProperties()
            {
                RuntimeImageName = runImage,
                RuntimeImageTag = runImageTag,
                BuildImageTag = buildImageTag,
            };

            return TemplateHelper.Render(
                TemplateHelper.TemplateResource.Dockerfile,
                properties,
                _logger);
        }

        private IDictionary<IProgrammingPlatform, PlatformDetectorResult> GetCompatiblePlatforms(DockerfileContext ctx)
        {
            return _platformDetector.GetCompatiblePlatforms(ctx);
        }

        /// <summary>
        /// For runtime images, the tag follows the format `{MAJOR}.{MINOR}`, so we need to correctly format the
        /// version that is returned from the detector to ensure we are pulling from a valid tag.
        /// </summary>
        /// <param name="version">The version of the platform returned from the detector.</param>
        /// <returns>A formatted version tag to pull the runtime image from.</returns>
        private string GenerateRuntimeTag(string version)
        {
            var split = version.Split('.');
            if (split.Length < 3)
            {
                return version;
            }

            return $"{split[0]}.{split[1]}";
        }

        private string ConvertToRuntimeName(string platformName)
        {
            if (string.Equals(platformName, DotNetCoreConstants.PlatformName, StringComparison.OrdinalIgnoreCase))
            {
                platformName = "dotnetcore";
            }

            if (string.Equals(platformName, NodeConstants.PlatformName, StringComparison.OrdinalIgnoreCase))
            {
                platformName = "node";
            }

            return platformName;
        }
    }
}
