// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal interface IPythonVersionResolver
    {
        /// <summary>
        /// Finds a python version that satisfies a version range.
        /// </summary>
        /// <param name="versionRange">The desired version range.</param>
        /// <returns>
        /// The maximum version that satisfies the provided range if one exists; null otherwise.
        /// </returns>
        string GetSupportedPythonVersion(string versionRange);
    }
}
