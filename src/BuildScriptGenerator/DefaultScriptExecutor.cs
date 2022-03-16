// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Common;

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
            int exitCode = ProcessHelper.TrySetExecutableMode(scriptPath, workingDirectory);
            if (exitCode != ProcessConstants.ExitSuccess)
            {
                return exitCode;
            }

            return ExecuteScriptInternal(scriptPath, args, workingDirectory, stdOutHandler, stdErrHandler);
        }

        protected virtual int ExecuteScriptInternal(
            string scriptPath,
            string[] args,
            string workingDirectory,
            DataReceivedEventHandler stdOutHandler,
            DataReceivedEventHandler stdErrHandler)
        {
            int exitCode;
            using (var timedEvent = _logger.LogTimedEvent("ExecuteScript"))
            {
                exitCode = ProcessHelper.RunProcess(
                    scriptPath,
                    args,
                    workingDirectory,
                    standardOutputHandler: stdOutHandler,
                    standardErrorHandler: stdErrHandler,
                    waitTimeForExit: null); // Do not provide wait time as the caller can do this themselves.
                timedEvent.AddProperty("exitCode", exitCode.ToString());
            }

            return exitCode;
        }
    }
}
