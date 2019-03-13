// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Oryx.Tests.Common
{
    public class DockerRunCommandProcessResult : IDockerResult
    {
        private readonly StringBuilder _stdOutBuilder;
        private readonly StringBuilder _stdErrBuilder;

        public DockerRunCommandProcessResult(
            string containerName,
            Process process,
            Exception exception,
            StringBuilder stdOutput,
            StringBuilder stdError,
            string executedCommand)
        {
            ContainerName = containerName;
            Exception = exception;
            Process = process;
            ExecutedCommand = executedCommand;
            _stdOutBuilder = stdOutput;
            _stdErrBuilder = stdError;
        }

        public string ContainerName { get; }
        public Process Process { get; }
        public Exception Exception { get; }
        public string ExecutedCommand { get; }

        public int ExitCode
        {
            get
            {
                return Process.ExitCode;
            }
        }

        public string StdOut
        {
            get
            {
                return _stdOutBuilder.ToString();
            }
        }

        public string StdErr
        {
            get
            {
                return _stdErrBuilder.ToString();
            }
        }

        public string GetDebugInfo(IDictionary<string, string> extraDefs = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Debugging Information:");
            sb.AppendLine("----------------------");

            var infoFormatter = new DefinitionListFormatter();
            infoFormatter.AddDefinition("Executed command", ExecutedCommand);
            if (Process.HasExited) infoFormatter.AddDefinition("Exit code", ExitCode.ToString());
            infoFormatter.AddDefinition("StdOut", StdOut);
            infoFormatter.AddDefinition("StdErr", StdErr);
            infoFormatter.AddDefinition("Exception.Message:", Exception?.Message);
            infoFormatter.AddDefinitions(extraDefs);
            sb.AppendLine(infoFormatter.ToString());

            return sb.ToString();
        }
    }
}
