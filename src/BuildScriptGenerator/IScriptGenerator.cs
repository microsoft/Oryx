// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator
{
    public interface IScriptGenerator
    {
        /// <summary>
        /// Tries generating a bash script based on the application in source directory.
        /// </summary>
        /// <param name="scriptGeneratorContext">The <see cref="ScriptGeneratorContext"/>.</param>
        /// <param name="script">The generated script if the operation was successful.</param>
        /// <returns><c>true</c> if the operation was successful, <c>false</c> otherwise.</returns>
        bool TryGenerateBashScript(ScriptGeneratorContext scriptGeneratorContext, out string script);
    }
}