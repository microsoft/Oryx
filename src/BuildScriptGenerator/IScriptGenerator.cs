// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator
{
    using System.Collections.Generic;
    using BuildScriptGenerator.SourceRepo;

    /// <summary>
    /// Build script generator for a particular language.
    /// </summary>
    public interface IScriptGenerator
    {
        /// <summary>
        /// The language which the script generator will create builds for.
        /// </summary>
        string LanguageName { get; }

        /// <summary>
        /// The list of versions that the script generator supports.
        /// </summary>
        IEnumerable<string> LanguageVersions { get; }

        /// <summary>
        /// Checks if the script generator supports the language being used in a given source repo.
        /// </summary>
        /// <param name="sourceRepo">The source repo to be checked.</param>
        /// <param name="providedLanguage">optional parameter that specifies which language is used in the source repo.</param>
        /// <returns><c>true</c> if the language is supported, <c>false</c> otherwise.</returns>
        bool CanGenerateScript(ISourceRepo sourceRepo, string providedLanguage = null);

        /// <summary>
        /// Generates an SH script that builds the source code in a path.
        /// </summary>
        /// <param name="sourceRepo">The source repo to create a build script for.</param>
        /// <returns>
        /// The build script.
        /// </returns>
        string GenerateBashScript(ISourceRepo sourceRepo);
    }
}