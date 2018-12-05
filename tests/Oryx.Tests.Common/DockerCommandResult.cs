// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Text;

namespace Oryx.Tests.Common
{
    public class DockerCommandResult
    {
        public DockerCommandResult(
            int exitCode,
            Exception exception,
            string output,
            string error,
            string executedCommand)
        {
            ExitCode = exitCode;
            Exception = exception;
            Output = output;
            Error = error;

            ExecutedCommand = executedCommand;
        }

        public int ExitCode { get; }

        public Exception Exception { get; }

        public string Output { get; }

        public string Error { get; }

        public bool IsSuccess
        {
            get
            {
                if (ExitCode != 0 || Exception != null)
                {
                    return false;
                }
                return true;
            }
        }

        protected string ExecutedCommand { get; }

        public virtual string GetDebugInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Debugging Information:");
            sb.AppendLine("----------------------");
            sb.AppendLine($"Executed command: {ExecutedCommand}");
            sb.AppendLine($"Exit code: {ExitCode}");
            sb.AppendLine($"StdOutput: {Output}");
            sb.AppendLine($"StdError: {Error}");
            sb.AppendLine($"Exception: {Exception?.Message}");
            return sb.ToString();
        }

        public override string ToString()
        {
            return $"ExitCode: {ExitCode}" +
                Environment.NewLine +
                $"Exception: {Exception}" +
                Environment.NewLine +
                $"StdOutput: {Output}" +
                Environment.NewLine +
                $"StdError: {Error}";
        }
    }
}
