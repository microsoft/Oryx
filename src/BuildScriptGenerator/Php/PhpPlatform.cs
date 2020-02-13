// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.SourceRepo;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    /// <summary>
    /// PHP platform.
    /// </summary>
    internal class PhpPlatform : IProgrammingPlatform
    {
        private readonly PhpScriptGeneratorOptions _phpScriptGeneratorOptions;
        private readonly IPhpVersionProvider _phpVersionProvider;
        private readonly ILogger<PhpPlatform> _logger;
        private readonly PhpLanguageDetector _detector;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhpPlatform"/> class.
        /// </summary>
        /// <param name="phpScriptGeneratorOptions">The options of phpScriptGenerator.</param>
        /// <param name="phpVersionProvider">The PHP version provider.</param>
        /// <param name="logger">The logger of PHP platform.</param>
        /// <param name="detector">The detector of PHP platform.</param>
        public PhpPlatform(
            IOptions<PhpScriptGeneratorOptions> phpScriptGeneratorOptions,
            IPhpVersionProvider phpVersionProvider,
            ILogger<PhpPlatform> logger,
            PhpLanguageDetector detector)
        {
            _phpScriptGeneratorOptions = phpScriptGeneratorOptions.Value;
            _phpVersionProvider = phpVersionProvider;
            _logger = logger;
            _detector = detector;
        }

        /// <summary>
        /// Gets the name of PHP platform which this generator will create builds for.
        /// </summary>
        public string Name => PhpConstants.PhpName;

        /// <summary>
        /// Gets the list of versions that the script generator supports.
        /// </summary>
        public IEnumerable<string> SupportedVersions => _phpVersionProvider.SupportedPhpVersions;

        /// <summary>
        /// Detects the programming platform name and version required by the application in source directory.
        /// </summary>
        /// <param name="context">The repository context.</param>
        /// <returns>The results of language detector operations.</returns>
        public LanguageDetectorResult Detect(RepositoryContext context)
        {
            return _detector.Detect(context);
        }

        /// <inheritdoc/>
        public BuildScriptSnippet GenerateBashBuildScriptSnippet(BuildScriptGeneratorContext ctx)
        {
            var buildProperties = new Dictionary<string, string>();

            // Write the version to the manifest file
            var key = $"{PhpConstants.PhpName}_version";
            buildProperties[key] = ctx.PhpVersion;

            _logger.LogDebug("Selected PHP version: {phpVer}", ctx.PhpVersion);
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
                        _logger.LogDependencies(this.Name, ctx.PhpVersion, depSpecs.Select(kv => kv.Key + kv.Value));
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
            return ctx.EnablePhp;
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
        public void SetRequiredTools(ISourceRepo sourceRepo, string targetPlatformVersion, IDictionary<string, string> toolsToVersion)
        {
            Debug.Assert(toolsToVersion != null, $"{nameof(toolsToVersion)} must not be null");
            if (!string.IsNullOrWhiteSpace(targetPlatformVersion))
            {
                toolsToVersion[PhpConstants.PhpName] = targetPlatformVersion;
            }
        }

        /// <inheritdoc/>
        public void SetVersion(BuildScriptGeneratorContext context, string version)
        {
            context.PhpVersion = version;
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
    }
}