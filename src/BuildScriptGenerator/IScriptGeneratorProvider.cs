// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator
{
    public interface IScriptGeneratorProvider
    {
        /// <summary>
        /// Look for the script generator most suitable for a source repo.
        /// </summary>
        /// <param name="sourceRepo">The source code repository.</param>
        /// <param name="scriptGeneratorContext">The <see cref="ScriptGeneratorContext"/>.</param>
        /// <returns>If found, returns an instance of the script generator for the source repo; otherwise, returns null.</returns>
        IScriptGenerator GetScriptGenerator(ScriptGeneratorContext scriptGeneratorContext);
    }
}