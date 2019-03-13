// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using System;
using System.Text;
using System.Collections.Generic;

namespace Microsoft.Oryx.Tests.Common
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

        public virtual string GetDebugInfo(IDictionary<string, string> extraDefs = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Debugging Information:");
            sb.AppendLine("----------------------");

            var infoFormatter = new DefinitionListFormatter();
            infoFormatter.AddDefinition("Executed command", ExecutedCommand);
            infoFormatter.AddDefinition("Exit code", ExitCode.ToString());
            infoFormatter.AddDefinition("StdOut", Output);
            infoFormatter.AddDefinition("StdErr", Error);
            infoFormatter.AddDefinition("Exception.Message:", Exception?.Message);
            infoFormatter.AddDefinitions(extraDefs);
            sb.AppendLine(infoFormatter.ToString());

            return sb.ToString();
        }

        public override string ToString()
        {
            return $"ExitCode: {ExitCode}" + Environment.NewLine +
                $"Exception: {Exception}" + Environment.NewLine +
                $"StdOut: {Output}" + Environment.NewLine +
                $"StdErr: {Error}";
        }
    }
}
