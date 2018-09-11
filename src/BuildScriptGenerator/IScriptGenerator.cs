// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator
{
    public interface IScriptGenerator
    {
        /// <summary>
        /// Gets a value to indicate if a script generator can generate a SH script for the given inputs.
        /// </summary>
        /// <returns><c>true</c>, if a generator can generate a script, <c>false</c> otherwise.</returns>
        bool CanGenerateShScript();

        /// <summary>
        /// Generates an SH script that builds the source code in a path.
        /// </summary>
        /// <returns>
        /// The build script.
        /// </returns>
        string GenerateShScript();
    }
}