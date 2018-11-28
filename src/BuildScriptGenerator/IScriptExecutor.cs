// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Diagnostics;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public interface IScriptExecutor
    {
        int ExecuteScript(
            string scriptPath,
            string[] args,
            DataReceivedEventHandler stdOutHandler,
            DataReceivedEventHandler stdErrHandler);
    }
}
