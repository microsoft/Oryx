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
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Common.Extensions;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.SourceRepo;
using Microsoft.Oryx.Common.Extensions;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Php;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    /// <summary>
    /// PHP platform.
    /// </summary>
    internal class PhpPlatform : IProgrammingPlatform
    {
        private readonly PhpScriptGeneratorOptions phpScriptGeneratorOptions;
        private readonly BuildScriptGeneratorOptions commonOptions;
        private readonly IPhpVersionProvider phpVersionProvider;
        private readonly IPhpComposerVersionProvider phpComposerVersionProvider;
        private readonly ILogger<PhpPlatform> logger;
        private readonly IPhpPlatformDetector detector;
        private readonly PhpPlatformInstaller phpInstaller;
        private readonly PhpComposerInstaller phpComposerInstaller;
        private readonly IExternalSdkProvider externalSdkProvider;
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhpPlatform"/> class.
        /// </summary>
        /// <param name="phpScriptGeneratorOptions">The options of phpScriptGenerator.</param>
        /// <param name="phpVersionProvider">The PHP version provider.</param>
        /// <param name="logger">The logger of PHP platform.</param>
        /// <param name="detector">The detector of PHP platform.</param>
        /// <param name="commonOptions">The <see cref="BuildScriptGeneratorOptions"/>.</param>
        /// <param name="phpComposerInstaller">The <see cref="PhpComposerInstaller"/>.</param>
        /// <param name="phpInstaller">The <see cref="PhpPlatformInstaller"/>.</param>
        /// <param name="phpComposerVersionProvider">The <see cref="IPhpComposerVersionProvider"/>.</param>
        public PhpPlatform(
            IOptions<PhpScriptGeneratorOptions> phpScriptGeneratorOptions,
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IPhpVersionProvider phpVersionProvider,
            IPhpComposerVersionProvider phpComposerVersionProvider,
            ILogger<PhpPlatform> logger,
            IPhpPlatformDetector detector,
            PhpPlatformInstaller phpInstaller,
            PhpComposerInstaller phpComposerInstaller,
            IExternalSdkProvider externalSdkProvider,
            TelemetryClient telemetryClient)
        {
            this.phpScriptGeneratorOptions = phpScriptGeneratorOptions.Value;
            this.commonOptions = commonOptions.Value;
            this.phpVersionProvider = phpVersionProvider;
            this.phpComposerVersionProvider = phpComposerVersionProvider;
            this.logger = logger;
            this.detector = detector;
            this.phpInstaller = phpInstaller;
            this.phpComposerInstaller = phpComposerInstaller;
            this.externalSdkProvider = externalSdkProvider;
            this.telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Gets the name of PHP platform which this generator will create builds for.
        /// </summary>
        public string Name => PhpConstants.PlatformName;

        /// <summary>
        /// Gets the list of versions that the script generator supports.
        /// </summary>
        public IEnumerable<string> SupportedVersions
        {
            get
            {
                var versionInfo = this.phpVersionProvider.GetVersionInfo();
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
            var phpPlatformDetectorResult = detectorResult as PhpPlatformDetectorResult;
            if (phpPlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(PhpPlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            var buildProperties = new Dictionary<string, string>();

            // Write the platform name and version to the manifest file
            buildProperties[ManifestFilePropertyKeys.PhpVersion] = phpPlatformDetectorResult.PlatformVersion;

            this.logger.LogDebug("Selected PHP version: {phpVer}", phpPlatformDetectorResult.PlatformVersion);
            bool composerFileExists = false;

            if (ctx.SourceRepo.FileExists(PhpConstants.ComposerFileName))
            {
                composerFileExists = true;

                try
                {
                    dynamic composerFile = ctx.SourceRepo.ReadJsonObjectFromFile(PhpConstants.ComposerFileName);
                    if (composerFile?.require != null)
                    {
                        Newtonsoft.Json.Linq.JObject deps = composerFile?.require;
                        var depSpecs = deps.ToObject<IDictionary<string, string>>();
                        this.telemetryClient.LogDependencies(
                            this.Name,
                            phpPlatformDetectorResult.PlatformVersion,
                            depSpecs.Select(kv => kv.Key + kv.Value));
                    }
                }
                catch (Exception exc)
                {
                    // Leave malformed composer.json files for Composer to handle.
                    // This prevents Oryx from erroring out when Composer itself might be able to tolerate the file.
                    this.logger.LogWarning(exc, $"Exception caught while trying to deserialize {PhpConstants.ComposerFileName.Hash()}");
                }
            }

            var props = new PhpBashBuildSnippetProperties
            {
                ComposerFileExists = composerFileExists,
            };
            string snippet = TemplateHelper.Render(TemplateHelper.TemplateResource.PhpBuildSnippet, props, this.logger, this.telemetryClient);
            return new BuildScriptSnippet { BashBuildScriptSnippet = snippet, BuildProperties = buildProperties };
        }

        /// <inheritdoc/>
        public bool IsEnabled(RepositoryContext ctx)
        {
            return this.commonOptions.EnablePhpBuild;
        }

        /// <inheritdoc/>
        public bool IsEnabledForMultiPlatformBuild(RepositoryContext ctx)
        {
            return true;
        }

        /// <inheritdoc/>
        public bool IsCleanRepo(ISourceRepo repo)
        {
            return true;
        }

        /// <inheritdoc/>
        public string GenerateBashRunTimeInstallationScript(RunTimeInstallationScriptGeneratorOptions options)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir(BuildScriptGeneratorContext ctx)
        {
            return Array.Empty<string>();
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir(BuildScriptGeneratorContext ctx)
        {
            return Array.Empty<string>();
        }

        /// <inheritdoc/>
        public void ResolveVersions(RepositoryContext context, PlatformDetectorResult detectorResult)
        {
            var phpPlatformDetectorResult = detectorResult as PhpPlatformDetectorResult;
            if (phpPlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(PhpPlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            this.ResolveVersionsUsingHierarchicalRules(phpPlatformDetectorResult);
        }

        /// <inheritdoc/>
        public string GetInstallerScriptSnippet(
            BuildScriptGeneratorContext context,
            PlatformDetectorResult detectorResult)
        {
            var phpPlatformDetectorResult = detectorResult as PhpPlatformDetectorResult;
            if (phpPlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(PhpPlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            if (this.commonOptions.EnableDynamicInstall)
            {
                this.logger.LogDebug("Dynamic install is enabled.");

                var scriptBuilder = new StringBuilder();

                this.InstallPhp(phpPlatformDetectorResult.PlatformVersion, scriptBuilder);

                this.InstallPhpComposer(phpPlatformDetectorResult.PhpComposerVersion, scriptBuilder);

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
        public IDictionary<string, string> GetToolsToBeSetInPath(
            RepositoryContext context,
            PlatformDetectorResult detectorResult)
        {
            var phpPlatformDetectorResult = detectorResult as PhpPlatformDetectorResult;
            if (phpPlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(PhpPlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            var tools = new Dictionary<string, string>();
            tools[PhpConstants.PlatformName] = phpPlatformDetectorResult.PlatformVersion;
            tools["composer"] = phpPlatformDetectorResult.PhpComposerVersion;
            return tools;
        }

        public string GetMaxSatisfyingPhpComposerVersionAndVerify(string version)
        {
            var versionInfo = this.phpComposerVersionProvider.GetVersionInfo();
            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                version,
                versionInfo.SupportedVersions);

            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exception = new UnsupportedVersionException(
                    PhpConstants.PhpComposerName,
                    version,
                    versionInfo.SupportedVersions);
                this.logger.LogError(
                    exception,
                    $"Exception caught, the version '{version}' is not supported for the Node platform.");
                throw exception;
            }

            return maxSatisfyingVersion;
        }

        private void InstallPhp(string phpVersion, StringBuilder scriptBuilder)
        {
            string script = null;
            if (this.phpInstaller.IsVersionAlreadyInstalled(phpVersion))
            {
                this.logger.LogDebug("PHP version {version} is already installed. So skipping installing it again.", phpVersion);
                return;
            }
            else
            {
                if (this.commonOptions.EnableExternalSdkProvider)
                {
                    this.logger.LogDebug("Php version {version} is not installed. External SDK provider is enabled so trying to fetch SDK using it.", phpVersion);

                    try
                    {
                        var blobName = BlobNameHelper.GetBlobNameForVersion("php", phpVersion, this.commonOptions.DebianFlavor);
                        var isExternalFetchSuccess = this.externalSdkProvider.RequestBlobAsync(this.Name, blobName).Result;
                        if (isExternalFetchSuccess)
                        {
                            this.logger.LogDebug("Php version {version} is fetched successfully using external SDK provider. So generating an installation script snippet which skips platform binary download.", phpVersion);

                            script = this.phpInstaller.GetInstallerScriptSnippet(phpVersion, skipSdkBinaryDownload: true);
                        }
                        else
                        {
                            this.logger.LogDebug("Php version {version} is not fetched successfully using external SDK provider. So generating an installation script snippet for it.", phpVersion);
                            script = this.phpInstaller.GetInstallerScriptSnippet(phpVersion);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, "Error while fetching php version {version} using external SDK provider.", phpVersion);
                        script = this.phpInstaller.GetInstallerScriptSnippet(phpVersion);
                    }
                }
                else
                {
                    this.logger.LogDebug("Php version {version} is not installed. So generating an installation script snippet for it.", phpVersion);
                    script = this.phpInstaller.GetInstallerScriptSnippet(phpVersion);
                }

                scriptBuilder.AppendLine(script);
            }
        }

        private void InstallPhpComposer(string phpComposerVersion, StringBuilder scriptBuilder)
        {
            // Install PHP Composer
            string script = null;
            if (string.IsNullOrEmpty(phpComposerVersion))
            {
                phpComposerVersion = PhpVersions.ComposerDefaultVersion;
            }

            if (this.phpComposerInstaller.IsVersionAlreadyInstalled(phpComposerVersion))
            {
                this.logger.LogDebug("PHP Composer version {version} is already installed. So skipping installing it again.", phpComposerVersion);
                return;
            }
            else
            {
                if (this.commonOptions.EnableExternalSdkProvider)
                {
                    this.logger.LogDebug("Php Composer version {version} is not installed. External SDK provider is enabled so trying to fetch SDK using it.", phpComposerVersion);

                    try
                    {
                        var blobName = BlobNameHelper.GetBlobNameForVersion("php-composer", phpComposerVersion, this.commonOptions.DebianFlavor);
                        var isExternalFetchSuccess = this.externalSdkProvider.RequestBlobAsync(this.Name, blobName).Result;
                        if (isExternalFetchSuccess)
                        {
                            this.logger.LogDebug("Php composer version {version} is fetched successfully using external SDK provider. So generating an installation script snippet which skips platform binary download.", phpComposerVersion);

                            script = this.phpComposerInstaller.GetInstallerScriptSnippet(phpComposerVersion, skipSdkBinaryDownload: true);
                        }
                        else
                        {
                            this.logger.LogDebug("Php comose version {version} is not fetched successfully using external SDK provider. So generating an installation script snippet for it.", phpComposerVersion);
                            script = this.phpComposerInstaller.GetInstallerScriptSnippet(phpComposerVersion);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, "Error while fetching php composer version {version} using external SDK provider.", phpComposerVersion);
                        script = this.phpComposerInstaller.GetInstallerScriptSnippet(phpComposerVersion);
                    }
                }
                else
                {
                    this.logger.LogDebug("Php composer version {version} is not installed. So generating an installation script snippet for it.", phpComposerVersion);
                    script = this.phpComposerInstaller.GetInstallerScriptSnippet(phpComposerVersion);
                }
            }

            scriptBuilder.AppendLine(script);
        }

        private void ResolveVersionsUsingHierarchicalRules(PhpPlatformDetectorResult detectorResult)
        {
            var phpVersion = ResolvePhpVersion(detectorResult.PlatformVersion);
            phpVersion = this.GetMaxSatisfyingPhpVersionAndVerify(phpVersion);

            var phpComposerVersion = ResolvePhpComposerVersion(detectorResult.PhpComposerVersion);
            phpComposerVersion = this.GetMaxSatisfyingPhpComposerVersionAndVerify(phpComposerVersion);

            detectorResult.PlatformVersion = phpVersion;
            detectorResult.PhpComposerVersion = phpComposerVersion;

            string ResolvePhpVersion(string detectedVersion)
            {
                // Explicitly specified version by user wins over detected version
                if (!string.IsNullOrEmpty(this.phpScriptGeneratorOptions.PhpVersion))
                {
                    return this.phpScriptGeneratorOptions.PhpVersion;
                }

                // If a version was detected, then use it.
                if (detectedVersion != null)
                {
                    return detectedVersion;
                }

                // Explicitly specified default version by user wins over detected default
                if (!string.IsNullOrEmpty(this.phpScriptGeneratorOptions.PhpDefaultVersion))
                {
                    return this.phpScriptGeneratorOptions.PhpDefaultVersion;
                }

                // Fallback to default version detection
                var versionInfo = this.phpVersionProvider.GetVersionInfo();
                return versionInfo.DefaultVersion;
            }

            string ResolvePhpComposerVersion(string detectedVersion)
            {
                // Explicitly specified version by user wins over detected version
                if (!string.IsNullOrEmpty(this.phpScriptGeneratorOptions.PhpComposerVersion))
                {
                    return this.phpScriptGeneratorOptions.PhpComposerVersion;
                }

                // If a version was detected, then use it.
                if (detectedVersion != null)
                {
                    return detectedVersion;
                }

                // Explicitly specified default version by user wins over detected default
                if (!string.IsNullOrEmpty(this.phpScriptGeneratorOptions.PhpComposerDefaultVersion))
                {
                    return this.phpScriptGeneratorOptions.PhpComposerDefaultVersion;
                }

                // Fallback to default version detection
                return PhpVersions.ComposerDefaultVersion;
            }
        }

        private string GetMaxSatisfyingPhpVersionAndVerify(string version)
        {
            var supportedVersions = this.SupportedVersions;
            var nonPreviewRuntimeVersions = supportedVersions.Where(v => !v.Any(c => char.IsLetter(c)));
            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                version,
                nonPreviewRuntimeVersions);

            // Check if a preview version is available
            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                // Preview versions: '7.4.0RC4', '7.4.0beta2', etc
                var previewRuntimeVersions = supportedVersions
                    .Where(v => v.Any(c => char.IsLetter(c)))
                    .Where(v => v.StartsWith(version))
                    .OrderByDescending(v => v);
                if (previewRuntimeVersions.Any())
                {
                    maxSatisfyingVersion = previewRuntimeVersions.First();
                }
            }

            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exc = new UnsupportedVersionException(
                    PhpConstants.PlatformName,
                    version,
                    supportedVersions);
                this.logger.LogError(
                    exc,
                    $"Exception caught, the version '{version}' is not supported for the PHP platform.");
                throw exc;
            }

            return maxSatisfyingVersion;
        }
    }
}