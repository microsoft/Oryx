// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator
{
    public interface IBuildScriptBuilder
    {
        /// <summary>
        /// Generates an SH script that builds the source code in a path.
        /// </summary>
        /// <returns>
        /// The build script.
        /// </returns>
        string GenerateShScript();
    }
}