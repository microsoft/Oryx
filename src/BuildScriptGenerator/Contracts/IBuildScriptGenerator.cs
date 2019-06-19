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
        /// <param name="ctx">The <see cref="BuildScriptGeneratorContext"/> with parameters for the script.</param>
        /// <param name="script">The generated script if the operation was successful.</param>
        /// <param name="checkerMessageSink">
        /// If specified, messages from checkers will be appended to this list.
        /// </param>
        void GenerateBashScript(
            BuildScriptGeneratorContext ctx,
            out string script,
            List<ICheckerMessage> checkerMessageSink = null);

        /// <summary>
        /// Determines which platforms can be used to build the given application.
        /// </summary>
        /// <param name="ctx">A <see cref="BuildScriptGeneratorContext"/>.</param>
        /// <returns>a list of platform and version pairs.</returns>
        IList<Tuple<IProgrammingPlatform, string>> GetCompatiblePlatforms(BuildScriptGeneratorContext ctx);

        /// <summary>
        /// Determines the versions of the tools required for building the given application.
        /// </summary>
        /// <param name="ctx">A <see cref="BuildScriptGeneratorContext"/>.</param>
        /// <returns>a dictionary of tool name and version pairs.</returns>
        IDictionary<string, string> GetRequiredToolVersions(BuildScriptGeneratorContext ctx);
    }
}