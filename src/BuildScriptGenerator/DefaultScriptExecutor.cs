// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

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
            DataReceivedEventHandler stdOutHandler,
            DataReceivedEventHandler stdErrHandler)
        {
            var exitCode = SetExecutePerimssionOnScript(scriptPath, stdOutHandler, stdErrHandler);
            if (exitCode != 0)
            {
                _logger.LogError(
                    $"Failed to set execute permission on script '{scriptPath}'. " +
                    $"Failed with exit code: {exitCode}");
                return exitCode;
            }

            exitCode = ExecuteScriptInternal(scriptPath, args, stdOutHandler, stdErrHandler);
            if (exitCode != 0)
            {
                _logger.LogError(
                    $"Execution of script at '{scriptPath}' did not succeed. " +
                    $"Failed with exit code: {exitCode}");
            }
            return exitCode;
        }

        protected virtual int SetExecutePerimssionOnScript(
            string scriptPath,
            DataReceivedEventHandler stdOutHandler,
            DataReceivedEventHandler stdErrHandler)
        {
            var exitCode = ProcessHelper.RunProcess(
                "chmod",
                arguments: new[] { "+x", scriptPath },
                standardOutputHandler: stdOutHandler,
                standardErrorHandler: stdErrHandler,
                // Do not provide wait time as the caller can do this themselves.
                waitForExitInSeconds: null);
            return exitCode;
        }

        protected virtual int ExecuteScriptInternal(
            string scriptPath,
            string[] args,
            DataReceivedEventHandler stdOutHandler,
            DataReceivedEventHandler stdErrHandler)
        {
            var exitCode = ProcessHelper.RunProcess(
                scriptPath,
                args,
                standardOutputHandler: stdOutHandler,
                standardErrorHandler: stdErrHandler,
                // Do not provide wait time as the caller can do this themselves.
                waitForExitInSeconds: null);
            return exitCode;
        }
    }
}
