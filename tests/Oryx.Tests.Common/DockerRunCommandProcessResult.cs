// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Text;

namespace Oryx.Tests.Common
{
    public class DockerRunCommandProcessResult
    {
        public DockerRunCommandProcessResult(
            string containerName,
            Process process,
            Exception exception,
            StringBuilder stdOutput,
            StringBuilder stdError,
            string executedCommand)
        {
            ContainerName = containerName;
            Process = process;
            Exception = exception;
            StdOutput = stdOutput;
            StdError = stdError;
            ExecutedCommand = executedCommand;
        }

        public string ContainerName { get; }
        public Process Process { get; }
        public Exception Exception { get; }
        public StringBuilder StdOutput { get; }
        public StringBuilder StdError { get; }
        public string ExecutedCommand { get; }

        public string GetDebugInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Debugging Information:");
            sb.AppendLine("----------------------");
            sb.AppendLine($"Executed command: {ExecutedCommand}");

            if (Process.HasExited)
            {
                sb.AppendLine($"Exit code: {Process.ExitCode}");
            }

            sb.AppendLine($"StdOutput: {StdOutput.ToString()}");
            sb.AppendLine($"StdError: {StdError.ToString()}");
            sb.AppendLine($"Exception: {Exception?.Message}");
            return sb.ToString();
        }
    }
}
