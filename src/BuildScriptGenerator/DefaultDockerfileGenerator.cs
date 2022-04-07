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
using Microsoft.Oryx.Detector;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal class DefaultDockerfileGenerator : IDockerfileGenerator
    {
        private readonly ICompatiblePlatformDetector platformDetector;
        private readonly ILogger<DefaultDockerfileGenerator> logger;
        private readonly BuildScriptGeneratorOptions commonOptions;
        private readonly IDictionary<string, IList<string>> slimPlatformVersions =
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
            this.platformDetector = platformDetector;
            this.logger = logger;
            this.commonOptions = commonOptions.Value;
        }

        public string GenerateDockerfile(DockerfileContext ctx)
        {
            var buildImageTag = "lts-versions";
            var runImage = string.Empty;
            var runImageTag = string.Empty;
            var compatiblePlatforms = this.GetCompatiblePlatforms(ctx);
            if (!compatiblePlatforms.Any())
            {
                throw new UnsupportedPlatformException(Labels.UnableToDetectPlatformMessage);
            }

            foreach (var platformAndDetectorResult in compatiblePlatforms)
            {
                var platform = platformAndDetectorResult.Key;
                var detectorResult = platformAndDetectorResult.Value;
                if (!this.slimPlatformVersions.ContainsKey(platform.Name) ||
                    (!this.slimPlatformVersions[platform.Name].Any(v => detectorResult.PlatformVersion.StartsWith(v)) &&
                     !this.slimPlatformVersions[platform.Name].Any(v => v.StartsWith(detectorResult.PlatformVersion))))
                {
                    buildImageTag = "latest";
                    runImageTag = GenerateRuntimeTag(detectorResult.PlatformVersion);
                }
                else
                {
                    runImageTag = this.slimPlatformVersions[platform.Name]
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
                this.logger);
        }

        /// <summary>
        /// For runtime images, the tag follows the format `{MAJOR}.{MINOR}`, so we need to correctly format the
        /// version that is returned from the detector to ensure we are pulling from a valid tag.
        /// </summary>
        /// <param name="version">The version of the platform returned from the detector.</param>
        /// <returns>A formatted version tag to pull the runtime image from.</returns>
        private static string GenerateRuntimeTag(string version)
        {
            var split = version.Split('.');
            if (split.Length < 3)
            {
                return version;
            }

            return $"{split[0]}.{split[1]}";
        }

        private static string ConvertToRuntimeName(string platformName)
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

        private IDictionary<IProgrammingPlatform, PlatformDetectorResult> GetCompatiblePlatforms(DockerfileContext ctx)
        {
            return this.platformDetector.GetCompatiblePlatforms(ctx);
        }
    }
}
