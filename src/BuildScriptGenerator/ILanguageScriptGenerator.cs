// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Build script generator for a particular language.
    /// </summary>
    public interface ILanguageScriptGenerator
    {
        /// <summary>
        /// Gets the language which the script generator will create builds for.
        /// </summary>
        string SupportedLanguageName { get; }

        /// <summary>
        /// Gets the list of versions that the script generator supports.
        /// </summary>
        IEnumerable<string> SupportedLanguageVersions { get; }

        /// <summary>
        /// Tries generating a bash script based on the application in source directory.
        /// </summary>
        /// <param name="scriptGeneratorContext">The <see cref="ScriptGeneratorContext"/>.</param>
        /// <returns><see cref="BuildScriptSnippet "/> with the build snippet if successful, <c>null</c> otherwise.</returns>
        BuildScriptSnippet GenerateBashBuildScriptSnippet(ScriptGeneratorContext scriptGeneratorContext);
    }
}