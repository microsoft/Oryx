// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal interface INodeVersionResolver
    {
        /// <summary>
        /// Finds a node version that satisfies a version range.
        /// </summary>
        /// <param name="versionRange">The desired version range.</param>
        /// <returns>
        /// The maximum version that satisfies the provided range if one exists; null otherwise.
        /// </returns>
        string GetSupportedNodeVersion(string versionRange);

        /// <summary>
        /// Finds an npm version that satisfies a version range.
        /// </summary>
        /// <param name="versionRange">The desired version range.</param>
        /// <returns>
        /// The maximum version that satisfies the provided range if one exists; null otherwise.
        /// </returns>
        string GetSupportedNpmVersion(string versionRange);
    }
}
