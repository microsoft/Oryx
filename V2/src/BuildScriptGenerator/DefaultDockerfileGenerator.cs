// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Common.Extensions;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Resources;
using Microsoft.Oryx.Detector;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal class DefaultDockerfileGenerator : IDockerfileGenerator
    {
        private const string DefaultOsType = "debian-buster";
        private const string DefaultCliImageTag = "debian-buster-stable";
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
        private readonly TelemetryClient telemetryClient;

        public DefaultDockerfileGenerator(
            ICompatiblePlatformDetector platformDetector,
            ILogger<DefaultDockerfileGenerator> logger,
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            TelemetryClient telemetryClient)
        {
            this.platformDetector = platformDetector;
            this.logger = logger;
            this.commonOptions = commonOptions.Value;
            this.telemetryClient = telemetryClient;
        }

        public string GenerateDockerfile(DockerfileContext ctx)
        {
            using (var timedEvent = this.telemetryClient.LogTimedEvent("GenerateDockerfile"))
            {
                var createScriptArguments = new Dictionary<string, string>();
                var dockerfileBuildImageName = "cli";
                var dockerfileBuildImageTag = DefaultCliImageTag;

                if (!string.IsNullOrEmpty(this.commonOptions.BuildImage))
                {
                    var buildImageSplit = this.commonOptions.BuildImage.Split(':');
                    dockerfileBuildImageName = buildImageSplit[0];
                    dockerfileBuildImageTag = buildImageSplit[1];
                }

                var dockerfileRuntimeImage = !string.IsNullOrEmpty(this.commonOptions.RuntimePlatformName) ?
                    ConvertToRuntimeName(this.commonOptions.RuntimePlatformName) : string.Empty;
                var dockerfileRuntimeImageTag = !string.IsNullOrEmpty(this.commonOptions.RuntimePlatformVersion) ?
                    this.commonOptions.RuntimePlatformVersion : string.Empty;

                if (!string.IsNullOrEmpty(this.commonOptions.BindPort))
                {
                    createScriptArguments.Add("bindPort", this.commonOptions.BindPort);
                }

                if (!string.IsNullOrEmpty(this.commonOptions.BindPort2))
                {
                    createScriptArguments.Add("bindPort2", this.commonOptions.BindPort2);
                }

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
                    // Note: we first need to ensure that the current detected platform is the same as the runtime platform name previously set or provided.
                    if (!string.IsNullOrEmpty(dockerfileRuntimeImage) && dockerfileRuntimeImage.Equals(detectedRuntimeName, StringComparison.OrdinalIgnoreCase) &&
                         string.IsNullOrEmpty(dockerfileRuntimeImageTag))
                    {
                        // Only try to get the runtime tag if the provided/detected platform has supported runtime images.
                        if (this.supportedRuntimeVersions.ContainsKey(dockerfileRuntimeImage))
                        {
                            dockerfileRuntimeImageTag = this.ConvertToRuntimeVersion(dockerfileRuntimeImage, platformDetectorResult.PlatformVersion);
                        }
                    }

                    // If the runtime image has been set manually or by the platform detection result, stop searching.
                    if (!string.IsNullOrEmpty(dockerfileRuntimeImage))
                    {
                        break;
                    }
                }

                // If the user didn't provided a custom build image, attempt to update the build image tag to
                // accurately reflect the OS flavor used by the provided/found for the runtime image.
                if (string.IsNullOrEmpty(this.commonOptions.BuildImage) && this.supportedRuntimeVersions.ContainsKey(dockerfileRuntimeImage))
                {
                    var runtimeDictionary = this.supportedRuntimeVersions[dockerfileRuntimeImage];
                    var fullRuntimeTag = runtimeDictionary.FirstOrDefault(x => dockerfileRuntimeImageTag == x ||
                                                                               dockerfileRuntimeImageTag == ParseRuntimeImageTag(x).Version);
                    if (fullRuntimeTag != null)
                    {
                        dockerfileRuntimeImageTag = fullRuntimeTag;
                        var osType = ParseRuntimeImageTag(dockerfileRuntimeImageTag).OsType;
                        if (string.IsNullOrEmpty(osType))
                        {
                            osType = DefaultOsType;
                        }

                        dockerfileBuildImageTag = $"{osType}-stable";
                    }
                }

                var formattedCreateScriptArguments = createScriptArguments.Any() ?
                    string.Join(' ', createScriptArguments.Select(arg => $"-{arg.Key} {arg.Value}")) : string.Empty;

                var properties = new DockerfileProperties()
                {
                    RuntimeImageName = dockerfileRuntimeImage,
                    RuntimeImageTag = dockerfileRuntimeImageTag,
                    BuildImageName = dockerfileBuildImageName,
                    BuildImageTag = dockerfileBuildImageTag,
                    CreateScriptArguments = formattedCreateScriptArguments,
                };

                this.ValidateDockerfileImages(properties);

                var generatedDockerfile = TemplateHelper.Render(
                    TemplateHelper.TemplateResource.Dockerfile,
                    properties,
                    this.logger,
                    this.telemetryClient);

                // Remove the Container Registry Analysis snippet, if it exists in the template.
                var pattern = "# DisableDockerDetector \".*?\"\n";
                generatedDockerfile = Regex.Replace(generatedDockerfile, pattern, string.Empty);

                var buildEventProps = new Dictionary<string, string>()
                {
                    { "runtimeImageName", properties.RuntimeImageName },
                    { "runtimeImageTag", properties.RuntimeImageTag },
                    { "buildImageName", properties.BuildImageName },
                    { "buildImageTag", properties.BuildImageTag },
                };
                timedEvent.SetProperties(buildEventProps);

                return generatedDockerfile;
            }
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
        /// When looking for a satisfying runtime version, format it so that tags '8' and '8.1' are treated as
        /// '8.99999.99999' and '8.1.99999', respectively. This allows the tags to be treated as the "maximum
        /// version within the provided major or minor version that Oryx supports" during version comparison.
        /// </summary>
        /// <param name="runtimeVersions">The list of runtime versions supported for a platform.</param>
        /// <returns>A list of tuples containing the original runtime version and its formatted version.</returns>
        private static IEnumerable<(string OriginalRuntimeVersion, string FormattedRuntimeVersion)> FormatRuntimeVersions(IEnumerable<string> runtimeVersions)
        {
            return runtimeVersions.Select(v => FormatRuntimeVersion(v));
        }

        /// <summary>
        /// When looking for a satisfying runtime version, format it so that tags '8' and '8.1' are treated as
        /// '8.99999.99999' and '8.1.99999', respectively. This allows the tags to be treated as the "maximum
        /// version within the provided major or minor version that Oryx supports" during version comparison.
        /// </summary>
        /// <param name="runtimeVersion">The runtime version supported for a platform.</param>
        /// <returns>A tuple containing the original runtime version and its formatted version.</returns>
        private static (string OriginalRuntimeVersion, string FormattedRuntimeVersion) FormatRuntimeVersion(string runtimeVersion)
        {
            var formattedVersion = ParseRuntimeImageTag(runtimeVersion).Version;
            var segments = runtimeVersion.Split('.');
            for (int i = 0; i < 3 - segments.Length; i++)
            {
                formattedVersion += ".99999";
            }

            return (runtimeVersion, formattedVersion);
        }

        /// <summary>
        /// Parses the version from the provided runtime image tag, assuming that the tag is in the format
        /// {version}-{osType}. For example, 18-debian-bullseye and 8.0-debian-bookworm would return 18 and 8.0, respectively.
        /// </summary>
        /// <param name="runtimeImageTag">The runtime image tag to parse the version from. For example, 18-debian-bullseye.</param>
        /// <returns>The version used in the runtime image tag.</returns>
        private static (string Version, string OsType) ParseRuntimeImageTag(string runtimeImageTag)
        {
            var split = runtimeImageTag.Split('-');
            if (split.Length < 2)
            {
                return (split[0], string.Empty);
            }

            return (split[0], runtimeImageTag.Substring(split[0].Length + 1));
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
            if (!string.IsNullOrEmpty(platformName) && this.supportedRuntimeVersions.ContainsKey(platformName))
            {
                var runtimeVersions = this.supportedRuntimeVersions[platformName]
                    .Where(v => !string.Equals(v, DynamicRuntimeImageTag, StringComparison.OrdinalIgnoreCase));
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

                    var formattedRuntimeVersions = FormatRuntimeVersions(runtimeVersions);
                    var satisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(platformVersion, formattedRuntimeVersions.Select(v => v.FormattedRuntimeVersion));
                    if (satisfyingVersion == null)
                    {
                        return runtimeVersions.LastOrDefault();
                    }

                    return formattedRuntimeVersions
                        .Where(v => string.Equals(v.FormattedRuntimeVersion, satisfyingVersion))
                        .Select(v => v.OriginalRuntimeVersion)
                        .FirstOrDefault();
                }

                return runtimeVersions.LastOrDefault();
            }

            return DynamicRuntimeImageTag;
        }

        private IDictionary<IProgrammingPlatform, PlatformDetectorResult> GetCompatiblePlatforms(DockerfileContext ctx)
        {
            return this.platformDetector.GetCompatiblePlatforms(ctx);
        }

        private void ValidateDockerfileImages(DockerfileProperties dockerfileProperties)
        {
            if (string.IsNullOrEmpty(dockerfileProperties.BuildImageName))
            {
                this.LogAndThrowInvalidDockerfileImageException(
                    "Provided build image name parsed from the provided --build-image argument is empty. " +
                    "Please provide a valid value for --build-image, or remove the argument to use the default " +
                    "'cli:stable' build image from mcr.microsoft.com/oryx.");
            }

            if (string.IsNullOrEmpty(dockerfileProperties.BuildImageTag))
            {
                this.LogAndThrowInvalidDockerfileImageException(
                    "Provided build image tag parsed from the provided --build-image argument is empty. " +
                    "Please provide a valid value for --build-image, or remove the argument to use the default " +
                    "'cli:stable' build image from mcr.microsoft.com/oryx.");
            }

            if (string.IsNullOrEmpty(dockerfileProperties.RuntimeImageName))
            {
                this.LogAndThrowInvalidDockerfileImageException(
                    "Either the value provided to the --runtime-platform argument is empty, or the platform " +
                    "discovered by Oryx does not have any available or supported runtime images. Please view the following " +
                    "document for more information on runtimes supported by Oryx: https://aka.ms/oryx-runtime-images");
            }

            if (string.IsNullOrEmpty(dockerfileProperties.RuntimeImageTag))
            {
                this.LogAndThrowInvalidDockerfileImageException(
                    "Either the value provided to the --runtime-platform-version argument is empty, or the version " +
                    "discovered by Oryx is not available or supported for the given platform. Please view the following " +
                    "document for more information on runtime versions supported by Oryx: https://aka.ms/oryx-runtime-images");
            }
        }

        private void LogAndThrowInvalidDockerfileImageException(string message)
        {
            var exc = new InvalidDockerfileImageException(message);
            this.logger.LogError(exc, message);
            throw exc;
        }
    }
}
