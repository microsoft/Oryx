// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Options for the dockerfile generation process.
    /// </summary>
    public class DockerfileContext : RepositoryContext
    {
        /// <summary>
        /// Gets or sets the name of the main programming platform used in the repo.
        /// If none is given, a platform detection algorithm will attempt to detect it.
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Gets or sets the version of the programming platform used in the repo.
        /// If provided, the <see cref="Platform"/> property should also be provided.
        /// </summary>
        public string PlatformVersion { get; set; }
    }
}
