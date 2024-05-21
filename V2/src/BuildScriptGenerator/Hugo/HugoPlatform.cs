// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Exceptions;
using Microsoft.Oryx.Detector.Hugo;

namespace Microsoft.Oryx.BuildScriptGenerator.Hugo
{
    internal class HugoPlatform : IProgrammingPlatform
    {
        private readonly ILogger<HugoPlatform> logger;
        private readonly HugoPlatformInstaller platformInstaller;
        private readonly BuildScriptGeneratorOptions commonOptions;
        private readonly HugoScriptGeneratorOptions hugoScriptGeneratorOptions;
        private readonly IHugoPlatformDetector detector;
        private readonly TelemetryClient telemetryClient;

        public HugoPlatform(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IOptions<HugoScriptGeneratorOptions> hugoScriptGeneratorOptions,
            ILogger<HugoPlatform> logger,
            HugoPlatformInstaller platformInstaller,
            IHugoPlatformDetector detector,
            TelemetryClient telemetryClient)
        {
            this.logger = logger;
            this.platformInstaller = platformInstaller;
            this.commonOptions = commonOptions.Value;
            this.hugoScriptGeneratorOptions = hugoScriptGeneratorOptions.Value;
            this.detector = detector;
            this.telemetryClient = telemetryClient;
        }

        /// <inheritdoc/>
        public string Name => HugoConstants.PlatformName;

        /// <inheritdoc/>
        public IEnumerable<string> SupportedVersions => new[] { HugoConstants.Version };

        public static string GetMaxSatisfyingVersionAndVerify(string version)
        {
            return version;
        }

        /// <inheritdoc/>
        public PlatformDetectorResult Detect(RepositoryContext context)
        {
            PlatformDetectorResult detectionResult;

            try
            {
                detectionResult = this.detector.Detect(new DetectorContext
                {
                    SourceRepo = new Detector.LocalSourceRepo(context.SourceRepo.RootPath),
                });
            }
            catch (FailedToParseFileException ex)
            {
                // Make sure to log exception which might contain the exact exception details from the parser which
                // we can look up in appinsights and tell user if required.
                this.logger.LogError(ex, ex.Message);

                throw new InvalidUsageException(ex.Message);
            }

            if (detectionResult == null)
            {
                return null;
            }

            this.ResolveVersions(context, detectionResult);
            return detectionResult;
        }

        /// <inheritdoc/>
        public BuildScriptSnippet GenerateBashBuildScriptSnippet(
            BuildScriptGeneratorContext context,
            PlatformDetectorResult detectorResult)
        {
            var manifestFileProperties = new Dictionary<string, string>();
            manifestFileProperties[ManifestFilePropertyKeys.HugoVersion] = detectorResult.PlatformVersion;
            manifestFileProperties[ManifestFilePropertyKeys.Frameworks] = "hugo";
            this.logger.LogInformation("Detected the following frameworks: hugo");
            Console.WriteLine("Detected the following frameworks: hugo");

            string script = TemplateHelper.Render(
                TemplateHelper.TemplateResource.HugoSnippet,
                model: null,
                this.logger,
                this.telemetryClient);

            return new BuildScriptSnippet
            {
                BashBuildScriptSnippet = script,
                BuildProperties = manifestFileProperties,
            };
        }

        /// <inheritdoc/>
        public string GenerateBashRunTimeInstallationScript(RunTimeInstallationScriptGeneratorOptions options)
        {
            return null;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return Array.Empty<string>();
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return Array.Empty<string>();
        }

        /// <inheritdoc/>
        public string GetInstallerScriptSnippet(
            BuildScriptGeneratorContext context,
            PlatformDetectorResult detectorResult)
        {
            string installationScriptSnippet = null;
            if (this.commonOptions.EnableDynamicInstall)
            {
                this.logger.LogDebug("Dynamic install is enabled.");

                if (this.platformInstaller.IsVersionAlreadyInstalled(detectorResult.PlatformVersion))
                {
                    this.logger.LogDebug(
                       "Hugo version {version} is already installed. So skipping installing it again.",
                       detectorResult.PlatformVersion);
                }
                else
                {
                    this.logger.LogDebug(
                        "Hugo version {version} is not installed. " +
                        "So generating an installation script snippet for it.",
                        detectorResult.PlatformVersion);

                    installationScriptSnippet = this.platformInstaller.GetInstallerScriptSnippet(
                        detectorResult.PlatformVersion);
                }
            }
            else
            {
                this.logger.LogDebug("Dynamic install not enabled.");
            }

            return installationScriptSnippet;
        }

        /// <inheritdoc/>
        public bool IsCleanRepo(ISourceRepo repo)
        {
            return true;
        }

        /// <inheritdoc/>
        public bool IsEnabled(RepositoryContext ctx)
        {
            return this.commonOptions.EnableHugoBuild;
        }

        /// <inheritdoc/>
        public bool IsEnabledForMultiPlatformBuild(RepositoryContext ctx)
        {
            return true;
        }

        /// <inheritdoc/>
        public void ResolveVersions(RepositoryContext context, PlatformDetectorResult detectorResult)
        {
            var resolvedVersion = this.GetVersionUsingHierarchicalRules(detectorResult.PlatformVersion);
            resolvedVersion = GetMaxSatisfyingVersionAndVerify(resolvedVersion);
            detectorResult.PlatformVersion = resolvedVersion;
        }

        /// <inheritdoc/>
        public IDictionary<string, string> GetToolsToBeSetInPath(
            RepositoryContext context,
            PlatformDetectorResult detectorResult)
        {
            var tools = new Dictionary<string, string>();
            tools[HugoConstants.PlatformName] = detectorResult.PlatformVersion;
            return tools;
        }

        private string GetVersionUsingHierarchicalRules(string detectedVersion)
        {
            // Explicitly specified version by user wins over detected version
            if (!string.IsNullOrEmpty(this.hugoScriptGeneratorOptions.HugoVersion))
            {
                return this.hugoScriptGeneratorOptions.HugoVersion;
            }

            // If a version was detected, then use it.
            if (detectedVersion != null)
            {
                return detectedVersion;
            }

            // Explicitly specified default version by user wins over detected default
            if (!string.IsNullOrEmpty(this.hugoScriptGeneratorOptions.DefaultVersion))
            {
                return this.hugoScriptGeneratorOptions.DefaultVersion;
            }

            // Fallback to default version
            return HugoConstants.Version;
        }
    }
}
