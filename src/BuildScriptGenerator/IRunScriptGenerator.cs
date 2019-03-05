// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public interface IRunScriptGenerator
    {
        /// <summary>
        /// Generates a bash script to run the application.
        /// </summary>
        /// <param name="targetPlatform">Name of the platform to generate the script for.</param>
        /// <param name="options">Options for the script generation.</param>
        /// <returns>Bash script to run the application.</returns>
        string GenerateBashScript(string targetPlatform, RunScriptGeneratorOptions options);
    }
}