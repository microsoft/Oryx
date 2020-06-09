// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Detector;
using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// An abstraction over the detection of compatible platforms in a given repository.
    /// </summary>
    public interface ICompatiblePlatformDetector
    {
        IDictionary<IProgrammingPlatform, string> GetCompatiblePlatforms(RepositoryContext ctx);

        /// <summary>
        /// Returns the compatible platforms for the given repository context. If a platform name (and version) was
        /// provided, this method only checks to see if the given platform is compatible.
        /// </summary>
        /// <param name="ctx">The <see cref="RepositoryContext"/>.</param>
        /// <returns>A dictionary mapping the valid platforms to the compatible version.</returns>
        IDictionary<IProgrammingPlatform, string> GetCompatiblePlatforms(
            RepositoryContext ctx,
            IEnumerable<PlatformDetectorResult> detectionResults);
    }
}
