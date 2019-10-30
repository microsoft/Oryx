// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Abstraction over the repository context.
    /// </summary>
    public abstract partial class RepositoryContext
    {
        public ISourceRepo SourceRepo { get; set; }

        /// <summary>
        /// Gets or sets specific properties for the generated script.
        /// </summary>
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether only the provided platform should be built, disabling
        /// the detection and build of all other platforms. If set to <c>true</c>, all other languages
        /// are disabled even if they are enabled by their specific flags.
        /// </summary>
        public bool DisableMultiPlatformBuild { get; set; } = true;
    }
}