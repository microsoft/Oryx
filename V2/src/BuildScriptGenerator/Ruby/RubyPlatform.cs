// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Ruby;

namespace Microsoft.Oryx.BuildScriptGenerator.Ruby
{
    /// <summary>
    /// RUBY platform.
    /// </summary>
    internal class RubyPlatform : IProgrammingPlatform
    {
        private readonly RubyScriptGeneratorOptions rubyScriptGeneratorOptions;
        private readonly BuildScriptGeneratorOptions commonOptions;
        private readonly IRubyVersionProvider rubyVersionProvider;
        private readonly ILogger<RubyPlatform> logger;
        private readonly IRubyPlatformDetector detector;
        private readonly RubyPlatformInstaller rubyInstaller;
        private readonly TelemetryClient telemetryClient;

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
            RubyPlatformInstaller rubyInstaller,
            TelemetryClient telemetryClient)
        {
            this.rubyScriptGeneratorOptions = rubyScriptGeneratorOptions.Value;
            this.commonOptions = commonOptions.Value;
            this.rubyVersionProvider = rubyVersionProvider;
            this.logger = logger;
            this.detector = detector;
            this.rubyInstaller = rubyInstaller;
            this.telemetryClient = telemetryClient;
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
                var versionInfo = this.rubyVersionProvider.GetVersionInfo();
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
                buildProperties[ManifestFilePropertyKeys.Frameworks] = "jekyll";
                this.logger.LogInformation("Detected the following frameworks: jekyll");
                Console.WriteLine("Detected the following frameworks: jekyll");
            }

            // Write the platform name and version to the manifest file
            buildProperties[ManifestFilePropertyKeys.RubyVersion] = rubyPlatformDetectorResult.PlatformVersion;

            this.logger.LogDebug("Selected Ruby version: {rubyVer}", rubyPlatformDetectorResult.PlatformVersion);

            var scriptProps = new RubyBashBuildSnippetProperties
            {
                UseBundlerToInstallDependencies = true,
                BundlerVersion = rubyPlatformDetectorResult.BundlerVersion,
                GemfileExists = rubyPlatformDetectorResult.GemfileExists,
                ConfigYmlFileExists = rubyPlatformDetectorResult.ConfigYmlFileExists,
                CustomBuildCommand = this.rubyScriptGeneratorOptions.CustomBuildCommand,
            };

            string script = TemplateHelper.Render(
                TemplateHelper.TemplateResource.RubyBuildSnippet,
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
            return this.commonOptions.EnableRubyBuild;
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

            if (this.commonOptions.EnableDynamicInstall)
            {
                this.logger.LogDebug("Dynamic install is enabled.");

                var scriptBuilder = new StringBuilder();

                this.InstallRuby(rubyPlatformDetectorResult.PlatformVersion, scriptBuilder);

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

            this.ResolveVersionsUsingHierarchicalRules(rubyPlatformDetectorResult);
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
            if (this.rubyInstaller.IsVersionAlreadyInstalled(rubyVersion))
            {
                this.logger.LogDebug(
                   "Ruby version {version} is already installed. So skipping installing it again.",
                   rubyVersion);
            }
            else
            {
                this.logger.LogDebug(
                    "Ruby version {version} is not installed. " +
                    "So generating an installation script snippet for it.",
                    rubyVersion);

                var script = this.rubyInstaller.GetInstallerScriptSnippet(rubyVersion);
                scriptBuilder.AppendLine(script);
            }
        }

        private void ResolveVersionsUsingHierarchicalRules(RubyPlatformDetectorResult detectorResult)
        {
            var rubyVersion = ResolveRubyVersion(detectorResult.PlatformVersion);
            rubyVersion = this.GetMaxSatisfyingRubyVersionAndVerify(rubyVersion);

            detectorResult.PlatformVersion = rubyVersion;

            string ResolveRubyVersion(string detectedVersion)
            {
                // Explicitly specified version by user wins over detected version
                if (!string.IsNullOrEmpty(this.rubyScriptGeneratorOptions.RubyVersion))
                {
                    return this.rubyScriptGeneratorOptions.RubyVersion;
                }

                // If a version was detected, then use it.
                if (detectedVersion != null)
                {
                    return detectedVersion;
                }

                // Explicitly specified default version by user wins over detected default
                if (!string.IsNullOrEmpty(this.rubyScriptGeneratorOptions.DefaultVersion))
                {
                    return this.rubyScriptGeneratorOptions.DefaultVersion;
                }

                // Fallback to default version detection
                var versionInfo = this.rubyVersionProvider.GetVersionInfo();
                return versionInfo.DefaultVersion;
            }
        }

        private string GetMaxSatisfyingRubyVersionAndVerify(string version)
        {
            var versionInfo = this.rubyVersionProvider.GetVersionInfo();
            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                version,
                versionInfo.SupportedVersions);

            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exc = new UnsupportedVersionException(
                    RubyConstants.PlatformName,
                    version,
                    versionInfo.SupportedVersions);
                this.logger.LogError(
                    exc,
                    $"Exception caught, the version '{version}' is not supported for the Ruby platform.");
                throw exc;
            }

            return maxSatisfyingVersion;
        }
    }
}