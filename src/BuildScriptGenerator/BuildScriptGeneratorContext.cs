// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Options for the build script generation process.
    /// </summary>
    public partial class BuildScriptGeneratorContext : RepositoryContext
    {
        /// <summary>
        /// Gets or sets the information which is used to correlate log messages.
        /// </summary>
        public string OperationId { get; set; }

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
        /// Gets or sets a value indicating whether the built sources should be packaged into a platform-specific format.
        /// </summary>
        public bool IsPackage { get; set; }

        /// <summary>
        /// Gets or sets a list of OS packages required for this build.
        /// </summary>
        public string[] RequiredOsPackages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether build-time checkers should run.
        /// Defaults to true.
        /// </summary>
        public bool EnableCheckers { get; set; } = true;

        /// <summary>
        /// Gets or sets a value of a directory where the build manifest file should be put into.
        /// </summary>
        public string ManifestDir { get; set; }
    }
}