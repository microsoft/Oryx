// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Golang;

namespace Microsoft.Oryx.BuildScriptGenerator.Golang
{
    internal class GolangPlatform : IProgrammingPlatform
    {
        private readonly GolangScriptGeneratorOptions goScriptGeneratorOptions;
        private readonly BuildScriptGeneratorOptions commonOptions;
        private readonly IGolangVersionProvider goVersionProvider;
        private readonly ILogger<GolangPlatform> logger;
        private readonly IGolangPlatformDetector detector;
        private readonly GolangPlatformInstaller golangInstaller;
        private readonly TelemetryClient telemetryClient;

        public GolangPlatform(
            IOptions<GolangScriptGeneratorOptions> goScriptGeneratorOptions,
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IGolangVersionProvider goVersionProvider,
            ILogger<GolangPlatform> logger,
            IGolangPlatformDetector detector,
            GolangPlatformInstaller golangInstaller,
            TelemetryClient telemetryClient)
        {
            this.goScriptGeneratorOptions = goScriptGeneratorOptions.Value;
            this.commonOptions = commonOptions.Value;
            this.goVersionProvider = goVersionProvider;
            this.logger = logger;
            this.detector = detector;
            this.golangInstaller = golangInstaller;
            this.telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Gets the name of Go platform which this generator will create builds for.
        /// </summary>
        public string Name => GolangConstants.PlatformName;

        /// <summary>
        /// Gets the list of versions that the script generator supports.
        /// </summary>
        public IEnumerable<string> SupportedVersions
        {
            get
            {
                var versionInfo = this.goVersionProvider.GetVersionInfo();
                return versionInfo.SupportedVersions;
            }
        }

        public PlatformDetectorResult Detect(RepositoryContext context)
        {
            var detectionResult = this.detector.Detect(new DetectorContext
            {
                SourceRepo = new Detector.LocalSourceRepo(context.SourceRepo.RootPath),
            });

            if (detectionResult == null)
            {
                return null;
            }

            this.ResolveVersions(context, detectionResult);
            return detectionResult;
        }

        /// <inheritdoc/>
        public BuildScriptSnippet GenerateBashBuildScriptSnippet(
            BuildScriptGeneratorContext ctx,
            PlatformDetectorResult detectorResult)
        {
            // confirm go detector not null
            var goPlatformDetectorResult = detectorResult as GolangPlatformDetectorResult;
            if (goPlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(GolangPlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            if (!goPlatformDetectorResult.GoModExists)
            {
                throw new InvalidUsageException("No go.mod found at the root of the repo. Please provide a go.mod file containing the version.");
            }

            // build properties & script snippets
            var buildProperties = new Dictionary<string, string>();

            // Write platform name and version to the manifest file
            buildProperties[ManifestFilePropertyKeys.GolangVersion] = goPlatformDetectorResult.PlatformVersion;
            this.logger.LogDebug($"Selected Go version: {goPlatformDetectorResult.PlatformVersion}");
            var scriptProps = new GolangBashBuildSnippetProperties(
                goPlatformDetectorResult.GoModExists,
                goPlatformDetectorResult.PlatformVersion);

            string script = TemplateHelper.Render(
                TemplateHelper.TemplateResource.GolangSnippet,
                scriptProps,
                this.logger,
                this.telemetryClient);

            return new BuildScriptSnippet
            {
                BashBuildScriptSnippet = script,
                BuildProperties = buildProperties,
            };
        }

        public string GenerateBashRunTimeInstallationScript(RunTimeInstallationScriptGeneratorOptions options)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(RepositoryContext ctx)
        {
            return this.commonOptions.EnableGolangBuild;
        }

        public bool IsCleanRepo(ISourceRepo repo)
        {
            return true;
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir(BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return Array.Empty<string>();
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir(BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return Array.Empty<string>();
        }

        public bool IsEnabledForMultiPlatformBuild(RepositoryContext ctx)
        {
            return false;
        }

        /// <inheritdoc/>
        public void ResolveVersions(RepositoryContext context, PlatformDetectorResult detectorResult)
        {
            var goPlatformDetectorResult = detectorResult as GolangPlatformDetectorResult;
            if (goPlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(GolangPlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            this.ResolveVersionsUsingHierarchicalRules(goPlatformDetectorResult);
        }

        public string GetInstallerScriptSnippet(BuildScriptGeneratorContext context, PlatformDetectorResult detectorResult)
        {
            var golangPlatformDetectorResult = detectorResult as GolangPlatformDetectorResult;
            if (golangPlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(GolangPlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            if (this.commonOptions.EnableDynamicInstall)
            {
                this.logger.LogDebug("Dynamic install is enabled.");

                var scriptBuilder = new StringBuilder();

                this.InstallGolang(golangPlatformDetectorResult.PlatformVersion, scriptBuilder);

                if (scriptBuilder.Length == 0)
                {
                    return null;
                }

                return scriptBuilder.ToString();
            }
            else
            {
                this.logger.LogDebug("Dynamic install not enabled.");
                return null;
            }
        }

        public IDictionary<string, string> GetToolsToBeSetInPath(RepositoryContext context, PlatformDetectorResult detectorResult)
        {
            var golangPlatformDetectorResult = detectorResult as GolangPlatformDetectorResult;
            if (golangPlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(GolangPlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            var tools = new Dictionary<string, string>();
            tools[GolangConstants.PlatformName] = golangPlatformDetectorResult.PlatformVersion;
            return tools;
        }

        private void ResolveVersionsUsingHierarchicalRules(GolangPlatformDetectorResult detectorResult)
        {
            var goVersion = ResolveGoVersion(detectorResult.PlatformVersion);
            goVersion = this.GetMaxSatisfyingGoVersionAndVerify(goVersion);

            detectorResult.PlatformVersion = goVersion;

            string ResolveGoVersion(string detectedVersion)
            {
                // Explicitly specified version by user wins over detected version
                if (!string.IsNullOrEmpty(this.goScriptGeneratorOptions.GolangVersion))
                {
                    return this.goScriptGeneratorOptions.GolangVersion;
                }

                // If a version was detected, then use it.
                if (detectedVersion != null)
                {
                    return detectedVersion;
                }

                // Explicitly specified default version by user wins over detected default
                if (!string.IsNullOrEmpty(this.goScriptGeneratorOptions.DefaultVersion))
                {
                    return this.goScriptGeneratorOptions.DefaultVersion;
                }

                // Fallback to default version detection
                var versionInfo = this.goVersionProvider.GetVersionInfo();
                return versionInfo.DefaultVersion;
            }
        }

        private string GetMaxSatisfyingGoVersionAndVerify(string version)
        {
            var versionInfo = this.goVersionProvider.GetVersionInfo();
            if (!versionInfo.SupportedVersions.Contains(version))
            {
                var exc = new UnsupportedVersionException(
                    GolangConstants.PlatformName,
                    version,
                    versionInfo.SupportedVersions);
                this.logger.LogError(
                    exc,
                    $"Exception caught, the version '{version}' is not supported for the Go platform.");
                throw exc;
            }

            return version;
        }

        private void InstallGolang(string golangVersion, StringBuilder scriptBuilder)
        {
            if (this.golangInstaller.IsVersionAlreadyInstalled(golangVersion))
            {
                this.logger.LogDebug(
                   "Golang version {version} is already installed. So skipping installing it again.",
                   golangVersion);
            }
            else
            {
                this.logger.LogDebug(
                    "Golang version {version} is not installed. " +
                    "So generating an installation script snippet for it.",
                    golangVersion);

                var script = this.golangInstaller.GetInstallerScriptSnippet(golangVersion);
                scriptBuilder.AppendLine(script);
            }
        }
    }
}
