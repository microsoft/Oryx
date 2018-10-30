// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

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

        public DefaultScriptGenerator(
            IEnumerable<ILanguageDetector> languageDetectors,
            IEnumerable<ILanguageScriptGenerator> scriptGenerators,
            ILogger<DefaultScriptGenerator> logger)
        {
            _languageDetectors = languageDetectors;
            _allScriptGenerators = scriptGenerators;
            _logger = logger;
        }

        public bool TryGenerateBashScript(ScriptGeneratorContext context, out string script)
        {
            script = null;

            var scriptGenerators = GetScriptGeneratorsByLanguageNameAndVersion(context);
            if (scriptGenerators == null)
            {
                return false;
            }

            foreach (var scriptGenerator in scriptGenerators)
            {
                if (scriptGenerator.TryGenerateBashScript(context, out script))
                {
                    _logger.LogDebug(
                        $"Script generator '{scriptGenerator.GetType()}' was used to generate the script.");

                    return true;
                }
                else
                {
                    _logger.LogDebug(
                        $"Script generator '{scriptGenerator.GetType()}' cannot generate the script.");
                }
            }

            return false;
        }

        private IEnumerable<ILanguageScriptGenerator> GetScriptGeneratorsByLanguageNameAndVersion(
            ScriptGeneratorContext context)
        {
            EnsureLanguageAndVersion(context);

            _logger.LogDebug(
                $"Finding script generator for language '{context.Language}' " +
                $"and version '{context.LanguageVersion}'.");

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
                var message = $"'{context.Language}' language is not supported. " +
                    $"Supported languages are: {string.Join(", ", languages)}";

                _logger.LogError(message);
                throw new UnsupportedLanguageException(message);
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
                var message = $"The '{context.Language}' version '{context.LanguageVersion}' is not supported. " +
                    $"Supported versions are: {string.Join(", ", allLanguageScriptGeneratorsVersions)}";

                _logger.LogError(message);
                throw new UnsupportedVersionException(message);
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
                _logger.LogDebug(
                    "Detecting the source directory for language and/or version ...");

                (languageName, languageVersion) = DetectLanguageAndVersion(context.SourceRepo);

                if (string.IsNullOrEmpty(languageName) || string.IsNullOrEmpty(languageVersion))
                {
                    throw new InvalidOperationException(
                        "Could not detect the language and/or version from source directory.");
                }

                _logger.LogDebug(
                    $"Detected language '{languageName}' and version '{languageVersion}' " +
                    "for application in source directory.");

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
                    _logger.LogDebug($"Language detector '{languageDetector.GetType()}' could not " +
                        "detect language in source directory.");
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