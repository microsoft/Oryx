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

        /// <summary>
        /// Generates a build Bash script based on the application in source directory.
        /// </summary>
        /// <param name="ctx">The context for BuildScriptGenerator.</param>
        /// <returns>The build script snippet.</returns>
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

        /// <summary>
        /// Checks if the programming platform should be included in a build script.
        /// </summary>
        /// <param name="ctx">The repository context.</param>
        /// <returns>True if the programming platform should be included in a build script, False otherwise.</returns>
        public bool IsEnabled(RepositoryContext ctx)
        {
            return ctx.EnablePhp;
        }

        /// <summary>
        /// Checks if the programming platform wants to participate in a multi-platform build.
        /// </summary>
        /// <param name="ctx">The repository context.</param>
        /// <returns>True if the programming platform is enabled for multi-platform build, False otherwise.</returns>
        public bool IsEnabledForMultiPlatformBuild(RepositoryContext ctx)
        {
            return true;
        }

        /// <summary>
        /// Checks if the source repository seems to have artifacts from a previous build.
        /// </summary>
        /// <param name="repo">A source code repository.</param>
        /// <returns>True if the source repository have artifacts already, False otherwise.</returns>
        public bool IsCleanRepo(ISourceRepo repo)
        {
            return true;
        }

        /// <summary>
        /// Generates a bash script that can install the required runtime bits for the application's platforms.
        /// </summary>
        /// <param name="options">The runtime installation script generator options.</param>
        /// <exception cref="NotImplementedException">Thrown when it's not implemented.</exception>
        /// <returns>Message from exception.</returns>
        public string GenerateBashRunTimeInstallationScript(RunTimeInstallationScriptGeneratorOptions options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds the required tools and their versions to a map.
        /// </summary>
        /// <param name="sourceRepo">The source repository.</param>
        /// <param name="targetPlatformVersion">The version of target platform.</param>
        /// <param name="toolsToVersion">A dictionary with tools as keys and versions as values.</param>
        public void SetRequiredTools(ISourceRepo sourceRepo, string targetPlatformVersion, IDictionary<string, string> toolsToVersion)
        {
            Debug.Assert(toolsToVersion != null, $"{nameof(toolsToVersion)} must not be null");
            if (!string.IsNullOrWhiteSpace(targetPlatformVersion))
            {
                toolsToVersion[PhpConstants.PhpName] = targetPlatformVersion;
            }
        }

        /// <summary>
        /// Sets the version of PHP platform in BuildScriptGeneratorContext.
        /// </summary>
        /// <param name="context">The context of BuildScriptGenerator.</param>
        /// <param name="version">The version of the PHP platform.</param>
        public void SetVersion(BuildScriptGeneratorContext context, string version)
        {
            context.PhpVersion = version;
        }

        /// <summary>
        /// Gets list of directories which need to be excluded from being copied to the output directory.
        /// </summary>
        /// <param name="ctx">The context of BuildScriptGenerator.</param>
        /// <returns>A list of directories.</returns>
        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir(BuildScriptGeneratorContext ctx)
        {
            return Array.Empty<string>();
        }

        /// <summary>
        /// Gets list of directories which need to be excluded from being copied to the intermediate directory, if used.
        /// </summary>
        /// <param name="ctx">The context of BuildScriptGenerator.</param>
        /// <returns>A list of directories.</returns>
        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir(BuildScriptGeneratorContext ctx)
        {
            return Array.Empty<string>();
        }
    }
}