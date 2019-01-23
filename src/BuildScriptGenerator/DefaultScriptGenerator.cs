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
        private readonly IEnumerable<ILanguageDetector> _languageDetectors;
        private readonly IEnumerable<ILanguageScriptGenerator> _allScriptGenerators;
        private readonly ILogger<DefaultScriptGenerator> _logger;
        private readonly IEnvironmentSettingsProvider _environmentSettingsProvider;

        public DefaultScriptGenerator(
            IEnumerable<ILanguageDetector> languageDetectors,
            IEnumerable<ILanguageScriptGenerator> scriptGenerators,
            IEnvironmentSettingsProvider environmentSettingsProvider,
            ILogger<DefaultScriptGenerator> logger)
        {
            _languageDetectors = languageDetectors;
            _allScriptGenerators = scriptGenerators;
            _environmentSettingsProvider = environmentSettingsProvider;
            _logger = logger;
        }

        public bool TryGenerateBashScript(ScriptGeneratorContext context, out string script)
        {
            script = null;

            var scriptGenerators = GetScriptGeneratorsByLanguageNameAndVersion(context);
            if (scriptGenerators == null)
            {
                _logger.LogWarning("Could not find any script generators");
                return false;
            }

            var snippets = new List<BuildScriptSnippet>();
            foreach (var scriptGenerator in scriptGenerators)
            {
                var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);
                if (snippet != null)
                {
                    _logger.LogDebug("Script generator {scriptGenType} was used", scriptGenerator.GetType());
                    snippets.Add(snippet);
                }
                else
                {
                    _logger.LogDebug("Script generator {scriptGenType} cannot be used", scriptGenerator.GetType());
                }
            }

            if (snippets.Any())
            {
                string benvArgs = GetBenvArgs(snippets);
                _environmentSettingsProvider.TryGetAndLoadSettings(out var environmentSettings);
                var buildScript = new BaseBashBuildScript()
                {
                    BuildScriptSnippets = snippets.Select(s => s.BashBuildScriptSnippet),
                    BenvArgs = benvArgs,
                    PreBuildScriptPath = environmentSettings?.PreBuildScriptPath,
                    PostBuildScriptPath = environmentSettings?.PostBuildScriptPath
                };
                script = buildScript.TransformText();
                return true;
            }

            return false;
        }

        private static string GetBenvArgs(List<BuildScriptSnippet> snippets)
        {
            // Build a dictionary to make sure we only pass one version for each tool to benv
            var benvArgsMap = new Dictionary<string, string>();
            foreach (var snippet in snippets)
            {
                if (snippet.RequiredToolsVersion != null)
                {
                    foreach (var tool in snippet.RequiredToolsVersion.Keys)
                    {
                        benvArgsMap[tool] = snippet.RequiredToolsVersion[tool];
                    }
                }
            }

            var listOfBenvArgs = benvArgsMap.Select(t => $"{t.Key}={t.Value}");
            var benvArgs = string.Join(' ', listOfBenvArgs);
            return benvArgs;
        }

        private IEnumerable<ILanguageScriptGenerator> GetScriptGeneratorsByLanguageNameAndVersion(
            ScriptGeneratorContext context)
        {
            EnsureLanguageAndVersion(context);

            _logger.LogDebug("Finding script generator for {lang} {langVer}", context.Language, context.LanguageVersion);

            var languageScriptGenerators = _allScriptGenerators.Where(sg =>
            {
                return string.Equals(
                    context.Language,
                    sg.SupportedLanguageName,
                    StringComparison.OrdinalIgnoreCase);
            });

            if (!languageScriptGenerators.Any())
            {
                var languages = _allScriptGenerators.Select(sg => sg.SupportedLanguageName);
                var exc = new UnsupportedLanguageException($"'{context.Language}' language is not supported. " +
                    $"Supported languages are: {string.Join(", ", languages)}");

                _logger.LogError(exc, "Exception caught");
                throw exc;
            }

            if (string.IsNullOrEmpty(context.LanguageVersion))
            {
                return languageScriptGenerators;
            }

            // Ignoring the order in which script generators are registered, find the script generator
            // which best matches the provided version semantically.
            var allLanguageScriptGeneratorsVersions = languageScriptGenerators.SelectMany(
                sg => sg.SupportedLanguageVersions);

            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                context.LanguageVersion,
                allLanguageScriptGeneratorsVersions);
            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exc = new UnsupportedVersionException(
                    $"The '{context.Language}' version '{context.LanguageVersion}' is not supported. " +
                    $"Supported versions are: {string.Join(", ", allLanguageScriptGeneratorsVersions)}");
                _logger.LogError(exc, "Exception caught");
                throw exc;
            }

            var maxSatisfyingVersionGenerators = languageScriptGenerators.Where(
                sg => sg.SupportedLanguageVersions.Contains(
                    maxSatisfyingVersion,
                    StringComparer.OrdinalIgnoreCase));

            return maxSatisfyingVersionGenerators;
        }

        private void EnsureLanguageAndVersion(ScriptGeneratorContext context)
        {
            var languageName = context.Language;
            var languageVersion = context.LanguageVersion;

            // If 'language' or 'language version' wasn't explicitly provided, detect the source directory
            if (string.IsNullOrEmpty(languageName) || string.IsNullOrEmpty(languageVersion))
            {
                _logger.LogDebug("Detecting the source directory for language and/or version");

                (languageName, languageVersion) = DetectLanguageAndVersion(context.SourceRepo);

                if (string.IsNullOrEmpty(languageName) || string.IsNullOrEmpty(languageVersion))
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
                        throw new InvalidOperationException("Could not detect the language and/or version from repo");
                    }
                }

                _logger.LogDebug("Detected {lang} {langVer} for app in repo", languageName, languageVersion);

                // Reset the context with detected values so that downstream components
                // use these detected values.
                context.Language = languageName;
                context.LanguageVersion = languageVersion;
            }
        }

        private (string language, string languageVersion) DetectLanguageAndVersion(ISourceRepo sourceRepo)
        {
            LanguageDetectorResult result = null;
            foreach (var languageDetector in _languageDetectors)
            {
                result = languageDetector.Detect(sourceRepo);
                if (result == null)
                {
                    _logger.LogWarning("Language detector {langDetectorType} could not detect language in repo", languageDetector.GetType());
                }
                else
                {
                    break;
                }
            }

            return (language: result?.Language, languageVersion: result?.LanguageVersion);
        }
    }
}