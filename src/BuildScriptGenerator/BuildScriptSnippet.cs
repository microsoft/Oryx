// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Snippet that can be used to build a particular language.
    /// </summary>
    public class BuildScriptSnippet
    {
        /// <summary>
        /// Gets or sets the build script snippet, written in bash.
        /// </summary>
        public string BashBuildScriptSnippet { get; set; }

        /// <summary>
        /// Gets or sets a property bag with a manifest for the build.
        /// </summary>
        /// <remarks>
        /// Each build script snippet might make decisions that should be
        /// recorded for later use, particularly when running the application.
        /// Those decisions can be expressed in the form of key-value pairs that
        /// are stored in a file in the build output.
        /// </remarks>
        public IDictionary<string, string> BuildProperties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this snippet represents a full script.
        /// </summary>
        public bool IsFullScript { get; set; }

        public bool CopySourceDirectoryContentToDestinationDirectory { get; set; } = true;
    }
}
