// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// An abstraction over the detection of all platforms in a given repository.
    /// </summary>
    public interface IDetector
    {
        /// <summary>
        /// Returns all platforms detected for the given repository context.
        /// </summary>
        /// <param name="ctx">The <see cref="RepositoryContext"/>.</param>
        /// <returns>A dictionary mapping the detected platforms to the version.</returns>
        IDictionary<IProgrammingPlatform, string> GetAllDetectedPlatforms(RepositoryContext ctx);
    }
}