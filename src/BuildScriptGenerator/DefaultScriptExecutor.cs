// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal class DefaultScriptExecutor : IScriptExecutor
    {
        private readonly ILogger<DefaultScriptExecutor> logger;
        private readonly TelemetryClient telemetryClient;

        public DefaultScriptExecutor(ILogger<DefaultScriptExecutor> logger, TelemetryClient telemetryClient)
        {
            this.logger = logger;
            this.telemetryClient = telemetryClient;
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

            return this.ExecuteScriptInternal(scriptPath, args, workingDirectory, stdOutHandler, stdErrHandler);
        }

        protected virtual int ExecuteScriptInternal(
            string scriptPath,
            string[] args,
            string workingDirectory,
            DataReceivedEventHandler stdOutHandler,
            DataReceivedEventHandler stdErrHandler)
        {
            int exitCode;
            using (var timedEvent = this.telemetryClient.LogTimedEvent("ExecuteScript"))
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
