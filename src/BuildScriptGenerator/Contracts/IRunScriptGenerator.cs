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
        /// <param name="ctx">The <see cref="RunScriptGeneratorContext"/> with parameters for the script.</param>
        /// <returns>Bash script to run the application.</returns>
        string GenerateBashScript(RunScriptGeneratorContext ctx);
    }
}
