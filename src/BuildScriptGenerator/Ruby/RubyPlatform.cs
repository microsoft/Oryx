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
using Microsoft.Oryx.Detector.Ruby;

namespace Microsoft.Oryx.BuildScriptGenerator.Ruby
{
    /// <summary>
    /// RUBY platform.
    /// </summary>
    internal class RubyPlatform : IProgrammingPlatform
    {
        private readonly RubyScriptGeneratorOptions _rubyScriptGeneratorOptions;
        private readonly BuildScriptGeneratorOptions _commonOptions;
        private readonly IRubyVersionProvider _rubyVersionProvider;
        private readonly ILogger<RubyPlatform> _logger;
        private readonly IRubyPlatformDetector _detector;
        private readonly RubyPlatformInstaller _rubyInstaller;

        /// <summary>
        /// Initializes a new instance of the <see cref="RubyPlatform"/> class.
        /// </summary>
        /// <param name="rubyScriptGeneratorOptions">The options of RubyScriptGenerator.</param>
        /// <param name="rubyVersionProvider">The Ruby version provider.</param>
        /// <param name="logger">The logger of Ruby platform.</param>
        /// <param name="detector">The detector of Ruby platform.</param>
        /// <param name="commonOptions">The <see cref="BuildScriptGeneratorOptions"/>.</param>
        /// <param name="rubyInstaller">The <see cref="RubyPlatformInstaller"/>.</param>
        public RubyPlatform(
            IOptions<RubyScriptGeneratorOptions> rubyScriptGeneratorOptions,
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IRubyVersionProvider rubyVersionProvider,
            ILogger<RubyPlatform> logger,
            IRubyPlatformDetector detector,
            RubyPlatformInstaller rubyInstaller)
        {
            _rubyScriptGeneratorOptions = rubyScriptGeneratorOptions.Value;
            _commonOptions = commonOptions.Value;
            _rubyVersionProvider = rubyVersionProvider;
            _logger = logger;
            _detector = detector;
            _rubyInstaller = rubyInstaller;
        }

        /// <summary>
        /// Gets the name of RUBY platform which this generator will create builds for.
        /// </summary>
        public string Name => RubyConstants.PlatformName;

        /// <summary>
        /// Gets the list of versions that the script generator supports.
        /// </summary>
        public IEnumerable<string> SupportedVersions
        {
            get
            {
                var versionInfo = _rubyVersionProvider.GetVersionInfo();
                return versionInfo.SupportedVersions;
            }
        }

        /// <summary>
        /// Detects the programming platform name and version required by the application in source directory.
        /// </summary>
        /// <param name="context">The repository context.</param>
        /// <returns>The results of language detector operations.</returns>
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
        public BuildScriptSnippet GenerateBashBuildScriptSnippet(
            BuildScriptGeneratorContext ctx,
            PlatformDetectorResult detectorResult)
        {
            var rubyPlatformDetectorResult = detectorResult as RubyPlatformDetectorResult;
            if (rubyPlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(RubyPlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            if (!rubyPlatformDetectorResult.GemfileExists && !rubyPlatformDetectorResult.ConfigYmlFileExists)
            {
                throw new InvalidUsageException($"No Gemfile found at the root of the repo. Please provide a Gemfile. " +
                $"For Jekyll apps, make sure it contains a '{RubyConstants.ConfigYmlFileName}' file and set it as a static web app");
            }

            var buildProperties = new Dictionary<string, string>();
            if (RubyConstants.ConfigYmlFileName != null)
            {
                buildProperties["Frameworks"] = "jekyll";
                _logger.LogInformation("Detected the the following framework(s): jekyll");
            }

            // Write the platform name and version to the manifest file
            buildProperties[ManifestFilePropertyKeys.RubyVersion] = rubyPlatformDetectorResult.PlatformVersion;

            _logger.LogDebug("Selected Ruby version: {rubyVer}", rubyPlatformDetectorResult.PlatformVersion);

            var scriptProps = new RubyBashBuildSnippetProperties
            {
                UseBundlerToInstallDependencies = true,
                BundlerVersion = rubyPlatformDetectorResult.BundlerVersion,
                GemfileExists = rubyPlatformDetectorResult.GemfileExists,
                ConfigYmlFileExists = rubyPlatformDetectorResult.ConfigYmlFileExists,
                CustomBuildCommand = _rubyScriptGeneratorOptions.CustomBuildCommand,
            };

            string script = TemplateHelper.Render(
                TemplateHelper.TemplateResource.RubyBuildSnippet,
                scriptProps,
                _logger);

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
            return _commonOptions.EnableRubyBuild;
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

        public string GetInstallerScriptSnippet(BuildScriptGeneratorContext context, PlatformDetectorResult detectorResult)
        {
            var rubyPlatformDetectorResult = detectorResult as RubyPlatformDetectorResult;
            if (rubyPlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(RubyPlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            if (_commonOptions.EnableDynamicInstall)
            {
                _logger.LogDebug("Dynamic install is enabled.");

                var scriptBuilder = new StringBuilder();

                InstallRuby(rubyPlatformDetectorResult.PlatformVersion, scriptBuilder);

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

        /// <inheritdoc/>
        public void ResolveVersions(RepositoryContext context, PlatformDetectorResult detectorResult)
        {
            var rubyPlatformDetectorResult = detectorResult as RubyPlatformDetectorResult;
            if (rubyPlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(RubyPlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            ResolveVersionsUsingHierarchicalRules(rubyPlatformDetectorResult);
        }

        /// <inheritdoc/>
        public IDictionary<string, string> GetToolsToBeSetInPath(
            RepositoryContext context,
            PlatformDetectorResult detectorResult)
        {
            var rubyPlatformDetectorResult = detectorResult as RubyPlatformDetectorResult;
            if (rubyPlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(RubyPlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            var tools = new Dictionary<string, string>();
            tools[RubyConstants.PlatformName] = rubyPlatformDetectorResult.PlatformVersion;
            return tools;
        }

        private void InstallRuby(string rubyVersion, StringBuilder scriptBuilder)
        {
            if (_rubyInstaller.IsVersionAlreadyInstalled(rubyVersion))
            {
                _logger.LogDebug(
                   "Ruby version {version} is already installed. So skipping installing it again.",
                   rubyVersion);
            }
            else
            {
                _logger.LogDebug(
                    "Ruby version {version} is not installed. " +
                    "So generating an installation script snippet for it.",
                    rubyVersion);

                var script = _rubyInstaller.GetInstallerScriptSnippet(rubyVersion);
                scriptBuilder.AppendLine(script);
            }
        }

        private void ResolveVersionsUsingHierarchicalRules(RubyPlatformDetectorResult detectorResult)
        {
            var rubyVersion = ResolveRubyVersion(detectorResult.PlatformVersion);
            rubyVersion = GetMaxSatisfyingRubyVersionAndVerify(rubyVersion);

            detectorResult.PlatformVersion = rubyVersion;

            string ResolveRubyVersion(string detectedVersion)
            {
                // Explicitly specified version by user wins over detected version
                if (!string.IsNullOrEmpty(_rubyScriptGeneratorOptions.RubyVersion))
                {
                    return _rubyScriptGeneratorOptions.RubyVersion;
                }

                // If a version was detected, then use it.
                if (detectedVersion != null)
                {
                    return detectedVersion;
                }

                // Fallback to default version
                var versionInfo = _rubyVersionProvider.GetVersionInfo();
                return versionInfo.DefaultVersion;
            }
        }

        private string GetMaxSatisfyingRubyVersionAndVerify(string version)
        {
            var versionInfo = _rubyVersionProvider.GetVersionInfo();
            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                version,
                versionInfo.SupportedVersions);

            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exc = new UnsupportedVersionException(
                    RubyConstants.PlatformName,
                    version,
                    versionInfo.SupportedVersions);
                _logger.LogError(
                    exc,
                    $"Exception caught, the version '{version}' is not supported for the Ruby platform.");
                throw exc;
            }

            return maxSatisfyingVersion;
        }
    }
}