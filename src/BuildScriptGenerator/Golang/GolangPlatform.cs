// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.SourceRepo;
using Microsoft.Oryx.Common.Extensions;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Golang;

namespace Microsoft.Oryx.BuildScriptGenerator.Golang
{
    internal class GolangPlatform : IProgrammingPlatform
    {
        private readonly GolangScriptGeneratorOptions _goScriptGeneratorOptions;
        private readonly BuildScriptGeneratorOptions _commonOptions;
        private readonly IGolangVersionProvider _goVersionProvider;
        private readonly ILogger<GolangPlatform> _logger;
        private readonly IGolangPlatformDetector _detector;
        private readonly GolangPlatformInstaller _golangInstaller;

        public GolangPlatform(
            IOptions<GolangScriptGeneratorOptions> goScriptGeneratorOptions,
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IGolangVersionProvider goVersionProvider,
            ILogger<GolangPlatform> logger,
            IGolangPlatformDetector detector,
            GolangPlatformInstaller golangInstaller)
        {
            _goScriptGeneratorOptions = goScriptGeneratorOptions.Value;
            _commonOptions = commonOptions.Value;
            _goVersionProvider = goVersionProvider;
            _logger = logger;
            _detector = detector;
            _golangInstaller = golangInstaller;
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
                var versionInfo = _goVersionProvider.GetVersionInfo();
                return versionInfo.SupportedVersions;
            }
        }

        public PlatformDetectorResult Detect(RepositoryContext context)
        {
            var detectionResult = _detector.Detect(new DetectorContext
            {
                SourceRepo = new Detector.LocalSourceRepo(context.SourceRepo.RootPath),
            });

            if (detectionResult == null)
            {
                return null;
            }

            ResolveVersions(context, detectionResult);
            return detectionResult;
        }

        /// <inheritdoc/>
        /// TODO: write implementation
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

            // TODO: remove go.mod requirement
            if (!goPlatformDetectorResult.GoModExists)
            {
                throw new InvalidUsageException("No go.mod found at the root of the repo. Please provide a go.mod file containing the version.");
            }

            // build properties & script snippets
            var buildProperties = new Dictionary<string, string>();

            // Write platform name and version to the manifest file
            buildProperties[ManifestFilePropertyKeys.GolangVersion] = goPlatformDetectorResult.PlatformVersion;
            _logger.LogDebug($"Selected Go version: {goPlatformDetectorResult.PlatformVersion}");

            var scriptProps = new GolangBashBuildSnippetProperties
            {
                GoModExists = goPlatformDetectorResult.GoModExists
            };

            string script = TemplateHelper.Render(
                TemplateHelper.TemplateResource.GolangSnippet,
                scriptProps,
                _logger);

            return new BuildScriptSnippet
            {
                BashBuildScriptSnippet = script,
                BuildProperties = buildProperties
            };
        }

        public string GenerateBashRunTimeInstallationScript(RunTimeInstallationScriptGeneratorOptions options)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(RepositoryContext ctx)
        {
            return _commonOptions.EnableGolangBuild;
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
            return true;
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

            ResolveVersionsUsingHierarchicalRules(goPlatformDetectorResult);
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

            if (_commonOptions.EnableDynamicInstall)
            {
                _logger.LogDebug("Dynamic install is enabled.");

                var scriptBuilder = new StringBuilder();

                InstallGolang(golangPlatformDetectorResult.PlatformVersion, scriptBuilder);

                if (scriptBuilder.Length == 0)
                {
                    return null;
                }

                return scriptBuilder.ToString();
            }
            else
            {
                _logger.LogDebug("Dynamic install not enabled.");
                return null;
            }
        }

        public IDictionary<string, string> GetToolsToBeSetInPath(RepositoryContext context, PlatformDetectorResult detectorResult)
        {
            throw new NotImplementedException();
        }

        private void ResolveVersionsUsingHierarchicalRules(GolangPlatformDetectorResult detectorResult)
        {
            var goVersion = ResolveGoVersion(detectorResult.PlatformVersion);
            goVersion = GetMaxSatisfyingGoVersionAndVerify(goVersion);

            detectorResult.PlatformVersion = goVersion;

            string ResolveGoVersion(string detectedVersion)
            {
                // Explicitly specified version by user wins over detected version
                if (!string.IsNullOrEmpty(_goScriptGeneratorOptions.GolangVersion))
                {
                    return _goScriptGeneratorOptions.GolangVersion;
                }

                // If a version was detected, then use it.
                if (detectedVersion != null)
                {
                    return detectedVersion;
                }

                // Fallback to default version
                var versionInfo = _goVersionProvider.GetVersionInfo();
                return versionInfo.DefaultVersion;
            }
        }

        private string GetMaxSatisfyingGoVersionAndVerify(string version)
        {
            var versionInfo = _goVersionProvider.GetVersionInfo();
            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                version,
                versionInfo.SupportedVersions);

            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exc = new UnsupportedVersionException(
                    GolangConstants.PlatformName,
                    version,
                    versionInfo.SupportedVersions);
                _logger.LogError(
                    exc,
                    $"Exception caught, the version '{version}' is not supported for the Go platform.");
                throw exc;
            }

            return maxSatisfyingVersion;
        }

        private void InstallGolang(string golangVersion, StringBuilder scriptBuilder)
        {
            if (_golangInstaller.IsVersionAlreadyInstalled(golangVersion))
            {
                _logger.LogDebug(
                   "Golang version {version} is already installed. So skipping installing it again.",
                   golangVersion);
            }
            else
            {
                _logger.LogDebug(
                    "Golang version {version} is not installed. " +
                    "So generating an installation script snippet for it.",
                    golangVersion);

                var script = _golangInstaller.GetInstallerScriptSnippet(golangVersion);
                scriptBuilder.AppendLine(script);
            }
        }
    }
}
