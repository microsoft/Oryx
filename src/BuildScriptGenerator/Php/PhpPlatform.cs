// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.SourceRepo;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    /// <summary>
    /// PHP platform.
    /// </summary>
    internal class PhpPlatform : IProgrammingPlatform
    {
        private readonly PhpScriptGeneratorOptions _phpScriptGeneratorOptions;
        private readonly BuildScriptGeneratorOptions _commonOptions;
        private readonly IPhpVersionProvider _phpVersionProvider;
        private readonly ILogger<PhpPlatform> _logger;
        private readonly PhpPlatformDetector _detector;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhpPlatform"/> class.
        /// </summary>
        /// <param name="phpScriptGeneratorOptions">The options of phpScriptGenerator.</param>
        /// <param name="phpVersionProvider">The PHP version provider.</param>
        /// <param name="logger">The logger of PHP platform.</param>
        /// <param name="detector">The detector of PHP platform.</param>
        public PhpPlatform(
            IOptions<PhpScriptGeneratorOptions> phpScriptGeneratorOptions,
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IPhpVersionProvider phpVersionProvider,
            ILogger<PhpPlatform> logger,
            PhpPlatformDetector detector)
        {
            _phpScriptGeneratorOptions = phpScriptGeneratorOptions.Value;
            _commonOptions = commonOptions.Value;
            _phpVersionProvider = phpVersionProvider;
            _logger = logger;
            _detector = detector;
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
                var versionInfo = _phpVersionProvider.GetVersionInfo();
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
            PlatformDetectorResult detectionResult;
            if (TryGetExplicitVersion(out var explicitVersion))
            {
                detectionResult = new PlatformDetectorResult
                {
                    Platform = PhpConstants.PlatformName,
                    PlatformVersion = explicitVersion,
                };
            }
            else
            {
                detectionResult = _detector.Detect(context);
            }

            if (detectionResult == null)
            {
                return null;
            }

            var version = ResolveVersion(detectionResult.PlatformVersion);
            detectionResult.PlatformVersion = version;
            return detectionResult;
        }

        /// <inheritdoc/>
        public BuildScriptSnippet GenerateBashBuildScriptSnippet(
            BuildScriptGeneratorContext ctx,
            PlatformDetectorResult detectorResult)
        {
            var buildProperties = new Dictionary<string, string>();

            // Write the version to the manifest file
            buildProperties[ManifestFilePropertyKeys.PhpVersion] = detectorResult.PlatformVersion;

            _logger.LogDebug("Selected PHP version: {phpVer}", detectorResult.PlatformVersion);
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
                        _logger.LogDependencies(
                            this.Name,
                            detectorResult.PlatformVersion,
                            depSpecs.Select(kv => kv.Key + kv.Value));
                    }
                }
                catch (Exception exc)
                {
                    // Leave malformed composer.json files for Composer to handle.
                    // This prevents Oryx from erroring out when Composer itself might be able to tolerate the file.
                    _logger.LogWarning(exc, $"Exception caught while trying to deserialize {PhpConstants.ComposerFileName.Hash()}");
                }
            }

            var props = new PhpBashBuildSnippetProperties { ComposerFileExists = composerFileExists };
            string snippet = TemplateHelper.Render(TemplateHelper.TemplateResource.PhpBuildSnippet, props, _logger);
            return new BuildScriptSnippet { BashBuildScriptSnippet = snippet, BuildProperties = buildProperties };
        }

        /// <inheritdoc/>
        public bool IsEnabled(RepositoryContext ctx)
        {
            return _commonOptions.EnablePhpBuild;
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

        public string GetInstallerScriptSnippet(
            BuildScriptGeneratorContext scriptGeneratorContext,
            PlatformDetectorResult detectorResult)
        {
            return null;
        }

        public string ResolveVersion(string versionToResolve)
        {
            var resolvedVersion = GetVersionUsingHierarchicalRules(versionToResolve);
            resolvedVersion = GetMaxSatisfyingVersionAndVerify(resolvedVersion);
            return resolvedVersion;
        }

        private string GetMaxSatisfyingVersionAndVerify(string version)
        {
            var versionInfo = _phpVersionProvider.GetVersionInfo();
            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                version,
                versionInfo.SupportedVersions);

            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exc = new UnsupportedVersionException(
                    PhpConstants.PlatformName,
                    version,
                    versionInfo.SupportedVersions);
                _logger.LogError(
                    exc,
                    $"Exception caught, the version '{version}' is not supported for the PHP platform.");
                throw exc;
            }

            return maxSatisfyingVersion;
        }

        private string GetVersionUsingHierarchicalRules(string detectedVersion)
        {
            if (!string.IsNullOrEmpty(_phpScriptGeneratorOptions.PhpVersion))
            {
                return _phpScriptGeneratorOptions.PhpVersion;
            }

            if (detectedVersion != null)
            {
                return detectedVersion;
            }

            var versionInfo = _phpVersionProvider.GetVersionInfo();
            return versionInfo.DefaultVersion;
        }
        
        private bool TryGetExplicitVersion(out string explicitVersion)
        {
            explicitVersion = null;

            var platformName = _commonOptions.PlatformName;
            if (platformName.EqualsIgnoreCase(PhpConstants.PlatformName))
            {
                if (string.IsNullOrWhiteSpace(_phpScriptGeneratorOptions.PhpVersion))
                {
                    return false;
                }

                explicitVersion = _phpScriptGeneratorOptions.PhpVersion;
                return true;
            }

            return false;
        }
    }
}