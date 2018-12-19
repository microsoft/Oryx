// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.Common.Utilities;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal class DefaultScriptExecutor : IScriptExecutor
    {
        private readonly ILogger<DefaultScriptExecutor> _logger;

        public DefaultScriptExecutor(ILogger<DefaultScriptExecutor> logger)
        {
            _logger = logger;
        }

        public int ExecuteScript(
            string scriptPath,
            string[] args,
            string workingDirectory,
            DataReceivedEventHandler stdOutHandler,
            DataReceivedEventHandler stdErrHandler)
        {
            int exitCode = SetExecutePerimssionOnScript(scriptPath, workingDirectory, stdOutHandler, stdErrHandler);
            if (exitCode != 0)
            {
                _logger.LogError("Failed to set execute permission on script {ScriptPath} ({ExitCode})", scriptPath, exitCode);
                return exitCode;
            }

            exitCode = ExecuteScriptInternal(scriptPath, args, workingDirectory, stdOutHandler, stdErrHandler);
            if (exitCode != 0)
            {
                _logger.LogError("Execution of script {ScriptPath} failed ({ExitCode})", scriptPath, exitCode);
            }

            return exitCode;
        }

        protected virtual int SetExecutePerimssionOnScript(
            string scriptPath,
            string workingDirectory,
            DataReceivedEventHandler stdOutHandler,
            DataReceivedEventHandler stdErrHandler)
        {
            var exitCode = ProcessHelper.RunProcess(
                "chmod",
                arguments: new[] { "+x", scriptPath },
                workingDirectory: workingDirectory,
                standardOutputHandler: stdOutHandler,
                standardErrorHandler: stdErrHandler,
                waitForExitInSeconds: null); // Do not provide wait time as the caller can do this themselves.
            return exitCode;
        }

        protected virtual int ExecuteScriptInternal(
            string scriptPath,
            string[] args,
            string workingDirectory,
            DataReceivedEventHandler stdOutHandler,
            DataReceivedEventHandler stdErrHandler)
        {
            int exitCode;
            using (var eventStopwatch = _logger.LogTimedEvent("RunProcess", new Dictionary<string, string> { { "scriptPath", scriptPath } }))
            {
                exitCode = ProcessHelper.RunProcess(
                    scriptPath,
                    args,
                    workingDirectory,
                    standardOutputHandler: stdOutHandler,
                    standardErrorHandler: stdErrHandler,
                    waitForExitInSeconds: null); // Do not provide wait time as the caller can do this themselves.
                eventStopwatch.AddProperty("exitCode", exitCode.ToString());
            }

            return exitCode;
        }
    }
}