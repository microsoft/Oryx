// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// An abstraction over the detection of compatible platforms in a given repository.
    /// </summary>
    public interface ICompatiblePlatformDetector
    {
        /// <summary>
        /// Returns the compatible platforms for the given repository context. If a platform name (and version) was
        /// provided, this method only checks to see if the given platform is compatible.
        /// </summary>
        /// <param name="ctx">The <see cref="RepositoryContext"/>.</param>
        /// <param name="platformName">The name of the platform to check compatibility for. If not provided,
        /// all platforms will be checked for compatibility.</param>
        /// <param name="platformVersion">The version of the platform to check compatibility for. If not provided,
        /// a compatible version will be checked for.</param>
        /// <returns>A dictionary mapping the valid platforms to the compatible version.</returns>
        IDictionary<IProgrammingPlatform, string> GetCompatiblePlatforms(RepositoryContext ctx);
    }
}
