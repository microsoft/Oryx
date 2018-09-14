// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator.SourceRepo;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Detects the language used in a project.
    /// </summary>
    public class DefaultScriptGeneratorProvider : IScriptGeneratorProvider
    {
        private readonly IEnumerable<IScriptGenerator> _scriptGenerators;

        public DefaultScriptGeneratorProvider(IEnumerable<IScriptGenerator> scriptGenerators)
        {
            _scriptGenerators = scriptGenerators;
        }

        public IScriptGenerator GetScriptGenerator(ISourceRepo sourceRepo, string providedLanguage)
        {
            foreach (var scriptGenerator in _scriptGenerators)
            {
                if (scriptGenerator.CanGenerateScript(sourceRepo, providedLanguage))
                {
                    return scriptGenerator;
                }
            }
            return null;
        }

        public IEnumerable<IScriptGenerator> GetScriptGenerators()
        {
            return _scriptGenerators;
        }
    }
}