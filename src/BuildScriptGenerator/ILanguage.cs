// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator
{
    using System.Collections.Generic;
    using BuildScriptGenerator.SourceRepo;

    /// <summary>
    /// Represents a supported language.
    /// </summary>
    public interface ILanguage
    {
        /// <summary>
        /// Collection of names that are commonly used for the language.
        /// </summary>
        IEnumerable<string> Name { get; }

        /// <summary>
        /// Gets a build script builder for the language, in case the language is used
        /// in a given folder.
        /// </summary>
        /// <param name="sourceRepo">The repo with the source code.</param>
        /// <param name="buildScriptBuilder">The build script builder that can be used in the given folder.</param>
        /// <returns>true if the provided source root has source code for the given language, false otherwise.</returns>
        bool TryGetBuildScriptBuilder(ISourceRepo sourceRepo, out IBuildScriptBuilder buildScriptBuilder);
    }
}