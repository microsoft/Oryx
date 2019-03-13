// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Oryx.Tests.Common
{
    public class DockerRunCommandProcessResult : DockerResultBase
    {
        private readonly StringBuilder _stdOutBuilder;
        private readonly StringBuilder _stdErrBuilder;

        public DockerRunCommandProcessResult(
            string containerName,
            Process process,
            Exception exception,
            StringBuilder stdOutput,
            StringBuilder stdError,
            string executedCommand) : base(exception, executedCommand)
        {
            ContainerName = containerName;
            Process = process;
            _stdOutBuilder = stdOutput;
            _stdErrBuilder = stdError;
        }

        public string ContainerName { get; }

        public Process Process { get; }

        public override bool HasExited { get => Process.HasExited; }

        public override int ExitCode { get => Process.ExitCode; }

        public override string StdOut { get => _stdOutBuilder.ToString(); }

        public override string StdErr { get => _stdErrBuilder.ToString(); }
    }
}
