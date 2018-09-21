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
        private readonly IEnumerable<IScriptGenerator> _scriptGenerators;
        private readonly ILogger<DefaultScriptGeneratorProvider> _logger;

        public DefaultScriptGeneratorProvider(
            IEnumerable<IScriptGenerator> scriptGenerators,
            ILogger<DefaultScriptGeneratorProvider> logger)
        {
            _scriptGenerators = scriptGenerators;
            _logger = logger;
        }

        public IScriptGenerator GetScriptGenerator(ScriptGeneratorContext context)
        {
            foreach (var scriptGenerator in _scriptGenerators)
            {
                if (!IsSupportedLanguage(context, scriptGenerator))
                {
                    _logger.LogDebug(
                        $"Script generator '{scriptGenerator.GetType()}' does not " +
                        $"support language '{context.LanguageName}'");
                    continue;
                }

                if (!IsSupportedLanguageVersion(context, scriptGenerator))
                {
                    _logger.LogDebug(
                        $"Script generator '{scriptGenerator.GetType()}' supports language {context.LanguageName}, " +
                        $"but does not support version '{context.LanguageName}'. " +
                        $"Supported versions are: {string.Join(", ", scriptGenerator.SupportedLanguageVersions)}");
                    continue;
                }

                if (scriptGenerator.CanGenerateScript(context))
                {
                    return scriptGenerator;
                }
            }
            return null;
        }

        private bool IsSupportedLanguage(ScriptGeneratorContext context, IScriptGenerator scriptGenerator)
        {
            if (!string.IsNullOrEmpty(context.LanguageName))
            {
                return string.Equals(
                    context.LanguageName,
                    scriptGenerator.SupportedLanguageName,
                    StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        //TODO: Check if we need to do the check only for major and minor version etc.
        private bool IsSupportedLanguageVersion(ScriptGeneratorContext context, IScriptGenerator scriptGenerator)
        {
            if (!string.IsNullOrEmpty(context.LanguageVersion))
            {
                return scriptGenerator.SupportedLanguageVersions.Contains(
                        context.LanguageVersion,
                        StringComparer.OrdinalIgnoreCase);
            }
            return true;
        }
    }
}