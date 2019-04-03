// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Finds and resolves scripts generators based on user input and invokes one of them to generate a script.
    /// </summary>
    internal class DefaultBuildScriptGenerator : IBuildScriptGenerator
    {
        private readonly IEnumerable<IProgrammingPlatform> _programmingPlatforms;
        private readonly ILogger<DefaultBuildScriptGenerator> _logger;
        private readonly IEnvironmentSettingsProvider _environmentSettingsProvider;

        public DefaultBuildScriptGenerator(
            IEnumerable<IProgrammingPlatform> programmingPlatforms,
            IEnvironmentSettingsProvider environmentSettingsProvider,
            ILogger<DefaultBuildScriptGenerator> logger)
        {
            _programmingPlatforms = programmingPlatforms;
            _environmentSettingsProvider = environmentSettingsProvider;
            _logger = logger;
        }

        public bool TryGenerateBashScript(BuildScriptGeneratorContext context, out string script)
        {
            script = null;

            var toolsToVersion = new Dictionary<string, string>();
            List<BuildScriptSnippet> snippets;
            var directoriesToExcludeFromCopyToIntermediateDir = new List<string>();

            using (var timedEvent = _logger.LogTimedEvent("GetBuildSnippets"))
            {
                snippets = GetBuildSnippets(
                    context,
                    toolsToVersion,
                    directoriesToExcludeFromCopyToIntermediateDir);
                timedEvent.SetProperties(toolsToVersion);
            }

            if (snippets.Any())
            {
                // By default exclude these irrespective of platform
                directoriesToExcludeFromCopyToIntermediateDir.Add(".git");

                script = BuildScriptFromSnippets(
                    snippets,
                    toolsToVersion,
                    directoriesToExcludeFromCopyToIntermediateDir);
                return true;
            }
            else
            {
                LogAndThrowNoPlatformFound(context);
                return false;
            }
        }

        private static string GetBenvArgs(Dictionary<string, string> benvArgsMap)
        {
            var listOfBenvArgs = benvArgsMap.Select(t => $"{t.Key}={t.Value}");
            var benvArgs = string.Join(' ', listOfBenvArgs);
            return benvArgs;
        }

        private List<BuildScriptSnippet> GetBuildSnippets(
            BuildScriptGeneratorContext context,
            Dictionary<string, string> toolsToVersion,
            List<string> directoriesToExcludeFromCopyToIntermediateDir)
        {
            bool providedLanguageFound = false;
            var snippets = new List<BuildScriptSnippet>();

            foreach (var platform in _programmingPlatforms)
            {
                if (!platform.IsEnabled(context))
                {
                    _logger.LogDebug("{platformName} has been disabled", platform.Name);
                    continue;
                }

                bool usePlatform = false;
                var currPlatformMatchesProvided = !string.IsNullOrEmpty(context.Language) &&
                    string.Equals(context.Language, platform.Name, StringComparison.OrdinalIgnoreCase);

                string targetVersionSpec = null;
                if (currPlatformMatchesProvided)
                {
                    providedLanguageFound = true;
                    targetVersionSpec = context.LanguageVersion;
                    usePlatform = true;
                }
                else if (context.DisableMultiPlatformBuild && !string.IsNullOrEmpty(context.Language))
                {
                    _logger.LogDebug(
                        "Multi platform build is disabled and platform was specified. " +
                        "Skipping language {skippedLang}",
                        platform.Name);
                    continue;
                }

                if (!currPlatformMatchesProvided || string.IsNullOrEmpty(targetVersionSpec))
                {
                    _logger.LogDebug("Detecting platform using {platformName}", platform.Name);
                    var detectionResult = platform.Detect(context.SourceRepo);
                    if (detectionResult != null)
                    {
                        _logger.LogDebug(
                            "Detected {platformName} version {platformVersion} for app in repo",
                            platform.Name,
                            detectionResult.LanguageVersion);
                        usePlatform = true;
                        targetVersionSpec = detectionResult.LanguageVersion;
                        if (string.IsNullOrEmpty(targetVersionSpec))
                        {
                            throw new UnsupportedVersionException(
                                $"Couldn't detect a version for the platform '{platform.Name}' in the repo.");
                        }
                    }
                }

                if (usePlatform)
                {
                    var excludedDirs = platform.GetDirectoriesToExcludeFromCopyToIntermediateDir(context);
                    if (excludedDirs.Any())
                    {
                        directoriesToExcludeFromCopyToIntermediateDir.AddRange(excludedDirs);
                    }

                    string targetVersion = GetMatchingTargetVersion(platform, targetVersionSpec);
                    platform.SetVersion(context, targetVersion);

                    string cleanOrNot = platform.IsCleanRepo(context.SourceRepo) ? "clean" : "not clean";
                    _logger.LogDebug($"Repo is {cleanOrNot} for {platform.Name}");

                    var snippet = platform.GenerateBashBuildScriptSnippet(context);
                    if (snippet != null)
                    {
                        _logger.LogDebug("Script generator {scriptGenType} was used", platform.GetType());
                        snippets.Add(snippet);
                        platform.SetRequiredTools(context.SourceRepo, targetVersion, toolsToVersion);
                    }
                    else
                    {
                        _logger.LogDebug("Script generator {scriptGenType} cannot be used", platform.GetType());
                    }
                }
            }

            // Even if a language was detected, we throw an error if the user provided
            // an unsupported language as target.
            if (!string.IsNullOrEmpty(context.Language) && !providedLanguageFound)
            {
                ThrowInvalidLanguageProvided(context);
            }

            return snippets;
        }

        private void ThrowInvalidLanguageProvided(BuildScriptGeneratorContext context)
        {
            var languages = _programmingPlatforms.Select(sg => sg.Name);
            var exc = new UnsupportedLanguageException($"'{context.Language}' platform is not supported. " +
                $"Supported platforms are: {string.Join(", ", languages)}");
            _logger.LogError(exc, "Exception caught");
            throw exc;
        }

        private void LogScriptIfGiven(string type, string scriptPath)
        {
            if (!string.IsNullOrWhiteSpace(scriptPath))
            {
                _logger.LogInformation("Using {type} script from {scriptPath}", type, scriptPath);
            }
        }

        /// <summary>
        /// Builds the full build script from the list of snippets for each platform.
        /// </summary>
        /// <returns>Finalized build script as a string.</returns>
        private string BuildScriptFromSnippets(
            List<BuildScriptSnippet> snippets,
            Dictionary<string, string> toolsToVersion,
            List<string> directoriesToExcludeFromCopyToIntermediateDir)
        {
            string script;
            string benvArgs = GetBenvArgs(toolsToVersion);
            _environmentSettingsProvider.TryGetAndLoadSettings(out var environmentSettings);

            Dictionary<string, string> buildProperties = snippets
                .Where(s => s.BuildProperties != null)
                .SelectMany(s => s.BuildProperties)
                .ToDictionary(p => p.Key, p => p.Value);

            var buildScriptProps = new BaseBashBuildScriptProperties()
            {
                BuildScriptSnippets = snippets.Select(s => s.BashBuildScriptSnippet),
                BenvArgs = benvArgs,
                PreBuildScriptPath = environmentSettings?.PreBuildScriptPath,
                PostBuildScriptPath = environmentSettings?.PostBuildScriptPath,
                DirectoriesToExcludeFromCopyToIntermediateDir = directoriesToExcludeFromCopyToIntermediateDir,
                ManifestFileName = Constants.ManifestFileName,
                BuildProperties = buildProperties
            };

            LogScriptIfGiven("pre-build", buildScriptProps.PreBuildScriptPath);
            LogScriptIfGiven("post-build", buildScriptProps.PostBuildScriptPath);

            script = TemplateHelpers.Render(
                TemplateHelpers.TemplateResource.BaseBashScript,
                buildScriptProps,
                _logger);
            return script;
        }

        /// <summary>
        /// Handles the error when no platform was found, logging information about the repo.
        /// </summary>
        private void LogAndThrowNoPlatformFound(BuildScriptGeneratorContext context)
        {
            try
            {
                var directoryStructureData = OryxDirectoryStructureHelper.GetDirectoryStructure(
                    context.SourceRepo.RootPath);
                _logger.LogTrace(
                    "logDirectoryStructure",
                    new Dictionary<string, string> { { "directoryStructure", directoryStructureData } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception caught");
            }
            finally
            {
                throw new UnsupportedLanguageException("Could not detect the language from repo.");
            }
        }

        /// <summary>
        /// Gets a matching version for the platform given a version in SemVer format.
        /// If the given version is not supported, an exception is thrown.
        /// </summary>
        /// <returns>The maximum version that satisfies the requested version spec.</returns>
        private string GetMatchingTargetVersion(IProgrammingPlatform platform, string targetVersionSpec)
        {
            string targetVersion;
            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
               targetVersionSpec,
               platform.SupportedLanguageVersions);

            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exc = new UnsupportedVersionException(
                    $"The '{platform.Name}' version '{targetVersionSpec}' is not supported. " +
                    $"Supported versions are: {string.Join(", ", platform.SupportedLanguageVersions)}");
                _logger.LogError(exc, "Exception caught");
                throw exc;
            }
            else
            {
                targetVersion = maxSatisfyingVersion;
            }

            return targetVersion;
        }
    }
}