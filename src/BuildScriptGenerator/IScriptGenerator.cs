// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Build script generator for a particular language.
    /// </summary>
    public interface IScriptGenerator
    {
        /// <summary>
        /// The language which the script generator will create builds for.
        /// </summary>
        string SupportedLanguageName { get; }

        /// <summary>
        /// The list of versions that the script generator supports.
        /// </summary>
        IEnumerable<string> SupportedLanguageVersions { get; }

        /// <summary>
        /// Checks if the script generator supports the language being used in a given source repo.
        /// </summary>
        /// <param name="sourceRepo">The source repo to be checked.</param>
        /// <param name="scriptGeneratorContext">The <see cref="ScriptGeneratorContext"/>.</returns>
        bool CanGenerateScript(ScriptGeneratorContext scriptGeneratorContext);

        /// <summary>
        /// Generates an SH script that builds the source code in a path.
        /// </summary>
        /// <param name="ScriptGeneratorContext">The <see cref="ScriptGeneratorContext"/>.</param>
        /// <returns>
        /// The build script.
        /// </returns>
        string GenerateBashScript(ScriptGeneratorContext scriptGeneratorContext);
    }
}