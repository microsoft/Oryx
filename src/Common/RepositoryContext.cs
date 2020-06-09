// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.Common
{
    /// <summary>
    /// Abstraction over the repository context.
    /// </summary>
    public abstract class RepositoryContext
    {
        public ISourceRepo SourceRepo { get; set; }

        /// <summary>
        /// Gets or sets specific properties for the generated script.
        /// </summary>
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// Gets or sets the version of PHP used in the repo.
        /// </summary>
        public string ResolvedPhpVersion { get; set; }

        /// <summary>
        /// Gets or sets the version of Python used in the repo.
        /// </summary>
        public string ResolvedPythonVersion { get; set; }

        /// <summary>
        /// Gets or sets the version of Node used in the repo.
        /// </summary>
        public string ResolvedNodeVersion { get; set; }

        /// <summary>
        /// Gets or sets the version of DotNetCore used in the repo.
        /// </summary>
        public string ResolvedDotNetCoreVersion { get; set; }

        /// <summary>
        /// Gets or sets the version of Hugo used in the repo.
        /// </summary>
        public string ResolvedHugoVersion { get; set; }
    }
}