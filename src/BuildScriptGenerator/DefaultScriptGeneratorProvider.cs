// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Detects the language used in a project.
    /// </summary>
    public class DefaultScriptGeneratorProvider : IScriptGeneratorProvider
    {
        private readonly IEnumerable<IScriptGenerator> _allScriptGenerators;
        private readonly ILogger<DefaultScriptGeneratorProvider> _logger;

        public DefaultScriptGeneratorProvider(
            IEnumerable<IScriptGenerator> scriptGenerators,
            ILogger<DefaultScriptGeneratorProvider> logger)
        {
            _allScriptGenerators = scriptGenerators;
            _logger = logger;
        }

        public IScriptGenerator GetScriptGenerator(ScriptGeneratorContext context)
        {
            var scriptGenerators = GetScriptGeneratorsByLanguageNameAndVersion(context);
            if (scriptGenerators == null)
            {
                return null;
            }

            foreach (var scriptGenerator in scriptGenerators)
            {
                if (scriptGenerator.CanGenerateScript(context))
                {
                    _logger.LogDebug(
                        $"Script generator '{scriptGenerator.GetType()}' will be used to generate the script.");

                    return scriptGenerator;
                }
                else
                {
                    _logger.LogDebug(
                        $"Script generator '{scriptGenerator.GetType()}' cannot generate the script.");
                }
            }

            return null;
        }

        private IEnumerable<IScriptGenerator> GetScriptGeneratorsByLanguageNameAndVersion(
            ScriptGeneratorContext context)
        {
            if (string.IsNullOrEmpty(context.Language))
            {
                return _allScriptGenerators;
            }

            var languageScriptGenerators = _allScriptGenerators.Where(sg =>
            {
                return string.Equals(
                    context.Language,
                    sg.SupportedLanguageName,
                    StringComparison.OrdinalIgnoreCase);
            });

            if (!languageScriptGenerators.Any())
            {
                _logger.LogError(
                    $"Cound not find a script generator which supports the language '{context.Language}'.");
                return null;
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
                _logger.LogError(
                    $"Found script generator(s) which support language '{context.Language}', but could " +
                    $"not find one which supports the version '{context.LanguageVersion}'.");
                return null;
            }

            var maxSatisfyingVersionGenerators = languageScriptGenerators.Where(
                sg => sg.SupportedLanguageVersions.Contains(
                    maxSatisfyingVersion,
                    StringComparer.OrdinalIgnoreCase));

            // Throw if multiple generators support the same version
            // For example, an exception should be thrown for input language versions like: 1, 1.2, 1.2.3
            // generator1 supports versions 1.2.2, 1.2.3
            // generator2 supports versions 1.2.3, 2.0.0
            if (maxSatisfyingVersionGenerators.Count() > 1)
            {
                var names = string.Join(", ", maxSatisfyingVersionGenerators.Select(sg => sg.GetType().FullName));
                throw new InvalidOperationException(
                    $"Cannot have multiple script generators supporting the same version '{maxSatisfyingVersion}'." +
                    $"Generators: {names}");
            }

            return maxSatisfyingVersionGenerators;
        }
    }
}