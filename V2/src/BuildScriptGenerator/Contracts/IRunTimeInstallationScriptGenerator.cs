// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public interface IRunTimeInstallationScriptGenerator
    {
        /// <summary>
        /// Generates a bash script to install the platform's runtime components.
        /// </summary>
        /// <param name="targetPlatform">Name of the platform to install runtime for.</param>
        /// <param name="options">Options for the script generation.</param>
        /// <returns>Bash script to install the platform's runtime components.</returns>
        string GenerateBashScript(string targetPlatform, RunTimeInstallationScriptGeneratorOptions options);
    }
}
