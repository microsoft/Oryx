// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator
{
    using System.Collections.Generic;
    using Microsoft.Oryx.BuildScriptGenerator.SourceRepo;

    public interface IScriptGeneratorProvider
    {
        /// <summary>
        /// Look for the script generator most suitable for a source repo.
        /// </summary>
        /// <param name="sourceRepo">The source code repository.</param>
        /// <param name="providedLanguage">Optional parameter to identify the language used in the source repo; if none is provided,
        /// an attempt will be made to detect it.</param>
        /// <returns>If found, returns an instance of the script generator for the source repo; otherwise, returns null.</returns>
        IScriptGenerator GetScriptGenerator(ISourceRepo sourceRepo, string providedLanguage = null);

        /// <summary>
        /// Gets all available script generators.
        /// </summary>
        /// <returns>A collection of the available script generators.</returns>
        IEnumerable<IScriptGenerator> GetScriptGenerators();
    }
}