// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Options for the run script generation process.
    /// </summary>
    public class RunScriptGeneratorContext : ScriptGeneratorContext
    {
        /// <summary>
        /// Gets or sets the name of the main programming language used in the repo.
        /// If none is given, a language detection algorithm will attemp to detect it.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the version of the programming language used in the repo.
        /// If provided, the <see cref="Language"/> property should also be provided.
        /// </summary>
        public string LanguageVersion { get; set; }

        /// <summary>
        /// Gets or sets the arguments to be passed into the run script generator.
        /// </summary>
        public string[] PassThruArguments { get; set; }
    }
}