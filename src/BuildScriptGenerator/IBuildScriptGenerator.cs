// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public interface IBuildScriptGenerator
    {
        /// <summary>
        /// Tries to generate a bash script to build an application.
        /// </summary>
        /// <param name="scriptGeneratorContext">
        /// The <see cref="BuildScriptGeneratorContext"/> with parameters for the script.
        /// </param>
        /// <param name="script">The generated script if the operation was successful.</param>
        /// <returns><c>true</c> if the operation was successful, <c>false</c> otherwise.</returns>
        bool TryGenerateBashScript(BuildScriptGeneratorContext scriptGeneratorContext, out string script);

        /// <summary>
        /// Determines which platforms can be used to build the given application.
        /// </summary>
        /// <param name="scriptGeneratorContext">
        /// The <see cref="BuildScriptGeneratorContext"/> with parameters for check.
        /// </param>
        /// <returns>a list of platform and version pairs.</returns>
        IList<Tuple<IProgrammingPlatform, string>> GetCompatiblePlatforms(BuildScriptGeneratorContext ctx);
    }
}