// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Text;

namespace Oryx.Tests.Infrastructure
{
    public class DockerCommandResult
    {
        public DockerCommandResult(
            int exitCode,
            Exception exception,
            string output,
            string executedCommand)
        {
            ExitCode = exitCode;
            Exception = exception;
            Output = output;

            ExecutedCommand = executedCommand;
        }

        public int ExitCode { get; }

        public Exception Exception { get; }

        public string Output { get; }

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

        public string ReplaceNewLine(string replacingString = "")
        {
            return Output.Replace(Environment.NewLine, replacingString).Replace("\0", replacingString).Replace("\r", replacingString);
        }

        public virtual string GetDebugInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Debugging Information:");
            sb.AppendLine("----------------------");
            sb.AppendLine($"Executed command: {ExecutedCommand}");
            sb.AppendLine($"Exit code: {ExitCode}");
            sb.AppendLine($"Output: {Output}");
            sb.AppendLine($"Exception: {Exception?.Message}");
            return sb.ToString();
        }

        public override string ToString()
        {
            return $"ExitCode: {ExitCode}" +
                Environment.NewLine +
                $"Exception: {Exception}" +
                Environment.NewLine +
                $"Output: {Output}";
        }
    }
}
