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
    public class DockerCommandResult : IDockerResult
    {
        public DockerCommandResult(int exitCode, Exception exception, string output, string error, string executedCommand)
        {
            Exception = exception;
            ExecutedCommand = executedCommand;
            ExitCode = exitCode;
            StdOut = output;
            StdErr = error;
        }

        public int ExitCode { get; }

        public Exception Exception { get; }

        public string ExecutedCommand { get; }

        public string StdOut { get; }

        public string StdErr { get; }

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

        public virtual string GetDebugInfo(IDictionary<string, string> extraDefs = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Debugging Information:");
            sb.AppendLine("----------------------");

            var infoFormatter = new DefinitionListFormatter();
            infoFormatter.AddDefinition("Executed command", ExecutedCommand);
            infoFormatter.AddDefinition("Exit code", ExitCode.ToString());
            infoFormatter.AddDefinition("StdOut", StdOut);
            infoFormatter.AddDefinition("StdErr", StdErr);
            infoFormatter.AddDefinition("Exception.Message:", Exception?.Message);
            infoFormatter.AddDefinitions(extraDefs);
            sb.AppendLine(infoFormatter.ToString());

            return sb.ToString();
        }

        public override string ToString()
        {
            return $"ExitCode: {ExitCode}" + Environment.NewLine +
                $"Exception: {Exception}" + Environment.NewLine +
                $"StdOut: {StdOut}" + Environment.NewLine +
                $"StdErr: {StdErr}";
        }
    }
}
