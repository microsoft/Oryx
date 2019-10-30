// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// An abstraction over the detection of compatible platforms in a given repository.
    /// </summary>
    public interface ICompatiblePlatformDetector
    {
        /// <summary>
        /// Returns the compatible platforms for the given repository context.
        /// </summary>
        /// <param name="ctx">The <see cref="RepositoryContext"/>.</param>
        /// <returns>A collection of tuples mapping the valid platforms to the compatible version.</returns>
        IList<Tuple<IProgrammingPlatform, string>> GetCompatiblePlatforms(RepositoryContext ctx);

        /// <summary>
        /// Returns a bool specifying whether or not the given platform is compatible with the provided repository.
        /// </summary>
        /// <param name="ctx">The <see cref="RepositoryContext"/>.</param>
        /// <param name="platformName">The name of the platform.</param>
        /// <param name="platformResult">If the given platform is compatible, this will be a tuple that maps
        /// the platform object to a compatible version.</param>
        /// <returns>True if the platform is compatible, false otherwise.</returns>
        bool IsCompatiblePlatform(RepositoryContext ctx,
                                  string platformName,
                                  out Tuple<IProgrammingPlatform, string> platformResult);

        /// <summary>
        /// Returns a bool specifying whether or not the given platform is compatible with the provided repository.
        /// </summary>
        /// <param name="ctx">The <see cref="RepositoryContext"/>.</param>
        /// <param name="platformName">The name of the platform.</param>
        /// <param name="platformVersion">The version of the platform.</param>
        /// <param name="platformResult">If the given platform is compatible, this will be a tuple that maps
        /// the platform object to a compatible version.</param>
        /// <returns>True if the platform is compatible, false otherwise.</returns>
        bool IsCompatiblePlatform(RepositoryContext ctx,
                                  string platformName,
                                  string platformVersion,
                                  out Tuple<IProgrammingPlatform, string> platformResult);
    }
}
