// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Common.Utilities;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Finds and resolves scripts generators based on user input and invokes one of them to generate a script.
    /// </summary>
    internal class DefaultScriptGenerator : IScriptGenerator
    {
        private readonly IEnumerable<IProgrammingPlatform> _programmingPlatforms;
        private readonly ILogger<DefaultScriptGenerator> _logger;
        private readonly IEnvironmentSettingsProvider _environmentSettingsProvider;

        public DefaultScriptGenerator(
            IEnumerable<IProgrammingPlatform> programmingPlatforms,
            IEnvironmentSettingsProvider environmentSettingsProvider,
            ILogger<DefaultScriptGenerator> logger)
        {
            _programmingPlatforms = programmingPlatforms;
            _environmentSettingsProvider = environmentSettingsProvider;
            _logger = logger;
        }

        public bool TryGenerateBashScript(ScriptGeneratorContext context, out string script)
        {
            script = null;
            var toolsToVersion = new Dictionary<string, string>();
            List<BuildScriptSnippet> snippets = GetBuildSnippets(context, toolsToVersion);

            if (snippets.Any())
            {
                script = BuildScriptFromSnippets(snippets, toolsToVersion);
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

        private List<BuildScriptSnippet> GetBuildSnippets(ScriptGeneratorContext context, Dictionary<string, string> toolsToVersion)
        {
            bool providedLanguageFound = false;
            var snippets = new List<BuildScriptSnippet>();

            foreach (var platform in _programmingPlatforms)
            {
                if (!platform.IsEnabled(context))
                {
                    _logger.LogDebug("{lang} has been disabled.", platform.Name);
                    continue;
                }

                bool usePlatform = false;
                var currPlatformMatch = !string.IsNullOrEmpty(context.Language) &&
                    string.Equals(context.Language, platform.Name, StringComparison.OrdinalIgnoreCase);

                string targetVersionSpec = null;
                if (currPlatformMatch)
                {
                    providedLanguageFound = true;
                    targetVersionSpec = context.LanguageVersion;
                    usePlatform = true;
                }
                else if (context.DisableMultiPlatformBuild)
                {
                    continue;
                }

                if (!currPlatformMatch || string.IsNullOrEmpty(targetVersionSpec))
                {
                    _logger.LogDebug("Detecting platform using {langPlat}", platform.Name);
                    var detectionResult = platform.Detect(context.SourceRepo);
                    if (detectionResult != null)
                    {
                        _logger.LogDebug("Detected {lang} version {version} for app in repo", platform.Name, detectionResult.LanguageVersion);
                        usePlatform = true;
                        targetVersionSpec = detectionResult.LanguageVersion;
                        if (string.IsNullOrEmpty(targetVersionSpec))
                        {
                            throw new UnsupportedVersionException($"Couldn't detect a version for the platform '{platform.Name}' in the repo.");
                        }
                    }
                }

                if (usePlatform)
                {
                    string targetVersion = GetMatchingTargetVersion(platform, targetVersionSpec);
                    platform.SetVersion(context, targetVersion);
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

            // Even if a language was detected, we throw an error if the user provided an unsupported language
            // as target.
            if (!string.IsNullOrEmpty(context.Language) && !providedLanguageFound)
            {
                ThrowInvalidLanguageProvided(context);
            }

            return snippets;
        }

        private void ThrowInvalidLanguageProvided(ScriptGeneratorContext context)
        {
            var languages = _programmingPlatforms.Select(sg => sg.Name);
            var exc = new UnsupportedLanguageException($"'{context.Language}' platform is not supported. " +
                $"Supported platforms are: {string.Join(", ", languages)}");
            _logger.LogError(exc, "Exception caught");
            throw exc;
        }

        /// <summary>
        /// Builds the full build script from the list of snippets for each platform.
        /// </summary>
        private string BuildScriptFromSnippets(List<BuildScriptSnippet> snippets, Dictionary<string, string> toolsToVersion)
        {
            string script;
            string benvArgs = GetBenvArgs(toolsToVersion);
            _environmentSettingsProvider.TryGetAndLoadSettings(out var environmentSettings);
            var buildScript = new BaseBashBuildScript()
            {
                BuildScriptSnippets = snippets.Select(s => s.BashBuildScriptSnippet),
                BenvArgs = benvArgs,
                PreBuildScriptPath = environmentSettings?.PreBuildScriptPath,
                PostBuildScriptPath = environmentSettings?.PostBuildScriptPath
            };
            script = buildScript.TransformText();
            return script;
        }

        /// <summary>
        /// Handles the error when no platform was found, logging information about the repo.
        /// </summary>
        private void LogAndThrowNoPlatformFound(ScriptGeneratorContext context)
        {
            try
            {
                var diretoryStructureData = OryxDirectoryStructureHelper.GetDirectoryStructure(context.SourceRepo.RootPath);
                _logger.LogDebug("Source repo structure {repoDir}", diretoryStructureData);
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