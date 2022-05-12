// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Resources;
using Microsoft.Oryx.Detector;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal class DefaultDockerfileGenerator : IDockerfileGenerator
    {
        private const string DynamicRuntimeImageTag = "dynamic";

        private readonly Dictionary<string, List<string>> supportedRuntimeVersions = new Dictionary<string, List<string>>()
        {
            { "dotnetcore", DotNetCoreSdkVersions.RuntimeVersions },
            { "node", NodeVersions.RuntimeVersions },
            { "php", PhpVersions.RuntimeVersions },
            { "python", PythonVersions.RuntimeVersions },
            { "ruby", RubyVersions.RuntimeVersions },
        };

        private readonly ICompatiblePlatformDetector platformDetector;
        private readonly ILogger<DefaultDockerfileGenerator> logger;
        private readonly BuildScriptGeneratorOptions commonOptions;

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
            var dockerfileBuildImageName = "cli";
            var dockerfileBuildImageTag = "stable";

            var dockerfileRuntimeImage = !string.IsNullOrEmpty(this.commonOptions.RuntimePlatformName) ?
                ConvertToRuntimeName(this.commonOptions.RuntimePlatformName) : string.Empty;
            var dockerfileRuntimeImageTag = !string.IsNullOrEmpty(this.commonOptions.RuntimePlatformVersion) ?
                this.commonOptions.RuntimePlatformVersion : string.Empty;
            var compatiblePlatforms = this.GetCompatiblePlatforms(ctx);
            if (!compatiblePlatforms.Any())
            {
                throw new UnsupportedPlatformException(Labels.UnableToDetectPlatformMessage);
            }

            foreach (var platformAndDetectorResult in compatiblePlatforms)
            {
                var platform = platformAndDetectorResult.Key;
                var platformDetectorResult = platformAndDetectorResult.Value;
                var detectedRuntimeName = ConvertToRuntimeName(platform.Name);

                // If the runtime platform name wasn't previously provided, see if we can use the current detected platform as the runtime image.
                if (string.IsNullOrEmpty(dockerfileRuntimeImage))
                {
                    // If the detected platform isn't in our currently list of runtime versions, skip it with a notice to the user.
                    if (!this.supportedRuntimeVersions.ContainsKey(detectedRuntimeName))
                    {
                        this.logger.LogDebug($"The detected platform {platform.Name} does not currently have a supported runtime image." +
                                               $"If this Dockerfile command or image is outdated, please provide the runtime platform name manually.");
                        continue;
                    }

                    dockerfileRuntimeImage = detectedRuntimeName;
                }

                // If the runtime platform version wasn't previously provided, see if we can detect one from the current detected platform.
                // Note: we first need to ensure that the current detected platform is the same as the runtime platform name previously set or pvodied.
                if (!string.IsNullOrEmpty(dockerfileRuntimeImage) && dockerfileRuntimeImage.Equals(detectedRuntimeName, StringComparison.OrdinalIgnoreCase) &&
                     string.IsNullOrEmpty(dockerfileRuntimeImageTag))
                {
                    dockerfileRuntimeImageTag = this.ConvertToRuntimeVersion(dockerfileRuntimeImage, platformDetectorResult.PlatformVersion);
                }

                // If the runtime image has been set manually or by the platform detection result, stop searching.
                if (!string.IsNullOrEmpty(dockerfileRuntimeImage))
                {
                    break;
                }
            }

            var properties = new DockerfileProperties()
            {
                RuntimeImageName = dockerfileRuntimeImage,
                RuntimeImageTag = dockerfileRuntimeImageTag,
                BuildImageName = dockerfileBuildImageName,
                BuildImageTag = dockerfileBuildImageTag,
            };

            return TemplateHelper.Render(
                TemplateHelper.TemplateResource.Dockerfile,
                properties,
                this.logger);
        }

        /// <summary>
        /// Converts the name of the platform to the expected runtime image name. For example, "dotnet" --> "dotnetcore".
        /// </summary>
        /// <param name="platformName">The name of the platform detected or provided.</param>
        /// <returns>The converted platform runtime image name.</returns>
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

        /// <summary>
        /// Determines the version of the runtime image to use in the Dockerfile using the following logic:
        /// (1) If both the platform name and version are provided, attempt to find the maximum satisfying version
        /// from the supported runtime version list that meets the version spec "~{VERSION}". If no version can be found,
        /// default to using the latest version in the list of supported runtime versions.
        /// (2) If only the platform name is provided, default to using the latest version in the list of supported runtime versions.
        /// (3) If neither the platform name nor version are provided, default to using the "dynamic" runtime tag.
        /// </summary>
        /// <param name="platformName">The name of the platform detected or provided.</param>
        /// <param name="platformVersion">The version of the platform detected or provided.</param>
        /// <returns>The converted platform runtime image version.</returns>
        private string ConvertToRuntimeVersion(string platformName, string platformVersion)
        {
            if (!string.IsNullOrEmpty(platformName))
            {
                var runtimeVersions = this.supportedRuntimeVersions[platformName];
                if (runtimeVersions == null || !runtimeVersions.Any())
                {
                    return DynamicRuntimeImageTag;
                }

                this.logger.LogDebug($"Supported runtime image tags for platform {platformName}: {string.Join(',', runtimeVersions)}");
                if (!string.IsNullOrEmpty(platformVersion))
                {
                    // We need to check if the detected platform version is in the form of a version spec or not.
                    if (SemanticVersionResolver.IsValidVersion(platformVersion))
                    {
                        // If it's a valid version, add a semver range specifier.
                        platformVersion = $"~{platformVersion}";
                    }

                    return SemanticVersionResolver.GetMaxSatisfyingVersion(platformVersion, runtimeVersions) ?? runtimeVersions.LastOrDefault();
                }

                return runtimeVersions.LastOrDefault();
            }

            return DynamicRuntimeImageTag;
        }

        private IDictionary<IProgrammingPlatform, PlatformDetectorResult> GetCompatiblePlatforms(DockerfileContext ctx)
        {
            return this.platformDetector.GetCompatiblePlatforms(ctx);
        }
    }
}
