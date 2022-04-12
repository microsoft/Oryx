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
        private const string DefaultRuntimeImageTag = "dynamic";

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
            var buildImageTag = "azfunc-jamstack";
            var runImage = !string.IsNullOrEmpty(this.commonOptions.RuntimePlatformName) ?
                ConvertToRuntimeName(this.commonOptions.RuntimePlatformName) : string.Empty;
            var runImageTag = !string.IsNullOrEmpty(this.commonOptions.RuntimePlatformVersion) ?
                this.commonOptions.RuntimePlatformVersion : DefaultRuntimeImageTag;
            var compatiblePlatforms = this.GetCompatiblePlatforms(ctx);
            if (!compatiblePlatforms.Any())
            {
                throw new UnsupportedPlatformException(Labels.UnableToDetectPlatformMessage);
            }

            foreach (var platformAndDetectorResult in compatiblePlatforms)
            {
                var platform = platformAndDetectorResult.Key;
                if (string.IsNullOrEmpty(runImage))
                {
                    runImage = ConvertToRuntimeName(platform.Name);
                }

                // If the runtime image has been set manually or by the platform detection result, stop searching.
                if (!string.IsNullOrEmpty(runImage))
                {
                    break;
                }
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
