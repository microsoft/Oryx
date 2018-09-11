// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;

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

        public IScriptGenerator GetScriptGenerator()
        {
            foreach (var scriptGenerator in _scriptGenerators)
            {
                if (scriptGenerator.CanGenerateShScript())
                {
                    return scriptGenerator;
                }
            }
            return null;
        }
    }
}