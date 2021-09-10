// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
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
        private readonly ILogger<HugoPlatform> _logger;
        private readonly HugoPlatformInstaller _platformInstaller;
        private readonly BuildScriptGeneratorOptions _commonOptions;
        private readonly HugoScriptGeneratorOptions _hugoScriptGeneratorOptions;
        private readonly IHugoPlatformDetector _detector;

        public HugoPlatform(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IOptions<HugoScriptGeneratorOptions> hugoScriptGeneratorOptions,
            ILogger<HugoPlatform> logger,
            HugoPlatformInstaller platformInstaller,
            IHugoPlatformDetector detector)
        {
            _logger = logger;
            _platformInstaller = platformInstaller;
            _commonOptions = commonOptions.Value;
            _hugoScriptGeneratorOptions = hugoScriptGeneratorOptions.Value;
            _detector = detector;
        }

        /// <inheritdoc/>
        public string Name => HugoConstants.PlatformName;

        /// <inheritdoc/>
        public IEnumerable<string> SupportedVersions => new[] { HugoConstants.Version };

        /// <inheritdoc/>
        public PlatformDetectorResult Detect(RepositoryContext context)
        {
            PlatformDetectorResult detectionResult;

            try
            {
                detectionResult = _detector.Detect(new DetectorContext
                {
                    SourceRepo = new Detector.LocalSourceRepo(context.SourceRepo.RootPath),
                });
            }
            catch (FailedToParseFileException ex)
            {
                // Make sure to log exception which might contain the exact exception details from the parser which
                // we can look up in appinsights and tell user if required.
                _logger.LogError(ex, ex.Message);

                throw new InvalidUsageException(ex.Message);
            }

            if (detectionResult == null)
            {
                return null;
            }

            ResolveVersions(context, detectionResult);
            return detectionResult;
        }

        /// <inheritdoc/>
        public BuildScriptSnippet GenerateBashBuildScriptSnippet(
            BuildScriptGeneratorContext context,
            PlatformDetectorResult detectorResult)
        {
            var manifestFileProperties = new Dictionary<string, string>();
            manifestFileProperties[ManifestFilePropertyKeys.HugoVersion] = detectorResult.PlatformVersion;

            string script = TemplateHelper.Render(
                TemplateHelper.TemplateResource.HugoSnippet,
                model: null,
                _logger);

            return new BuildScriptSnippet
            {
                BashBuildScriptSnippet = script,
                BuildProperties = manifestFileProperties,
                CustomRunBuildCommand = _hugoScriptGeneratorOptions.CustomRunBuildCommand,
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
            if (_commonOptions.EnableDynamicInstall)
            {
                _logger.LogDebug("Dynamic install is enabled.");

                if (_platformInstaller.IsVersionAlreadyInstalled(detectorResult.PlatformVersion))
                {
                    _logger.LogDebug(
                       "Hugo version {version} is already installed. So skipping installing it again.",
                       detectorResult.PlatformVersion);
                }
                else
                {
                    _logger.LogDebug(
                        "Hugo version {version} is not installed. " +
                        "So generating an installation script snippet for it.",
                        detectorResult.PlatformVersion);

                    installationScriptSnippet = _platformInstaller.GetInstallerScriptSnippet(
                        detectorResult.PlatformVersion);
                }
            }
            else
            {
                _logger.LogDebug("Dynamic install not enabled.");
            }

            return installationScriptSnippet;
        }

        /// <inheritdoc/>
        public string GetMaxSatisfyingVersionAndVerify(string version)
        {
            return version;
        }

        /// <inheritdoc/>
        public bool IsCleanRepo(ISourceRepo repo)
        {
            return true;
        }

        /// <inheritdoc/>
        public bool IsEnabled(RepositoryContext ctx)
        {
            return _commonOptions.EnableHugoBuild;
        }

        /// <inheritdoc/>
        public bool IsEnabledForMultiPlatformBuild(RepositoryContext ctx)
        {
            return true;
        }

        /// <inheritdoc/>
        public void ResolveVersions(RepositoryContext context, PlatformDetectorResult detectorResult)
        {
            var resolvedVersion = GetVersionUsingHierarchicalRules(detectorResult.PlatformVersion);
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
            if (!string.IsNullOrEmpty(_hugoScriptGeneratorOptions.HugoVersion))
            {
                return _hugoScriptGeneratorOptions.HugoVersion;
            }

            // If a version was detected, then use it.
            if (detectedVersion != null)
            {
                return detectedVersion;
            }

            // Fallback to default version
            return HugoConstants.Version;
        }
    }
}
