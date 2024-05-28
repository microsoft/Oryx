// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Diagnostics;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Abstraction for executing scripts.
    /// </summary>
    public interface IScriptExecutor
    {
        /// <summary>
        /// Executes the given script <paramref name="scriptPath"/> with supplied <paramref name="args"/>.
        /// </summary>
        /// <param name="scriptPath">Full path to the script that needs to be executed.</param>
        /// <param name="args">The arguments that need to be passed to the script.</param>
        /// <param name="workingDirectory">The directory under which the supplied script would be run.</param>
        /// <param name="stdOutHandler">The registered handler for capturing standard output messages.</param>
        /// <param name="stdErrHandler">The registered handler for capturing standard error messages.</param>
        /// <returns>0 if success.</returns>
        int ExecuteScript(
            string scriptPath,
            string[] args,
            string workingDirectory,
            DataReceivedEventHandler stdOutHandler,
            DataReceivedEventHandler stdErrHandler);
    }
}
