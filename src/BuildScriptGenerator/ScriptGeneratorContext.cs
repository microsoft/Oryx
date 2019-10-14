// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Abstraction over the context used for the script generation process.
    /// </summary>
    public abstract class ScriptGeneratorContext
    {
        public ISourceRepo SourceRepo { get; set; }

        /// <summary>
        /// Gets or sets specific properties for the generated script.
        /// </summary>
        public IDictionary<string, string> Properties { get; set; }
    }
}