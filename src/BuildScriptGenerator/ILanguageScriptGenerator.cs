// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Build script generator for a particular language.
    /// </summary>
    public interface ILanguageScriptGenerator : IScriptGenerator
    {
        /// <summary>
        /// The language which the script generator will create builds for.
        /// </summary>
        string SupportedLanguageName { get; }

        /// <summary>
        /// The list of versions that the script generator supports.
        /// </summary>
        IEnumerable<string> SupportedLanguageVersions { get; }
    }
}