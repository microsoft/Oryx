// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

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
    }
}
