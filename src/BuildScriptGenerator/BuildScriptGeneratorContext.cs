// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Options for the build script generation process.
    /// </summary>
    public partial class BuildScriptGeneratorContext
    {
        /// <summary>
        /// Gets or sets the information which is used to correlate log messages.
        /// </summary>
        public string OperationId { get; set; }

        public ISourceRepo SourceRepo { get; set; }

        /// <summary>
        /// Gets or sets the name of the main programming language used in the repo.
        /// If none is given, a language detection algorithm will attemp to detect it.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the version of the programming language used in the repo.
        /// If provided, the <see cref="BuildScriptGeneratorContext.Language"/> property should also be provided.
        /// </summary>
        public string LanguageVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether only the provided platform should be built, disabling
        /// the detection and build of all other platforms. If set to <c>true</c>, all other languages
        /// are disabled even if they are enabled by their specific flags.
        /// </summary>
        public bool DisableMultiPlatformBuild { get; set; } = true;

        /// <summary>
        /// Gets or sets specific properties for the build script.
        /// </summary>
        public IDictionary<string, string> Properties { get; set; }

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