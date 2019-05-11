// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.Tests.Common
{
    public class DockerCommandResult : DockerResultBase
    {
        public DockerCommandResult(
            int exitCode,
            Exception exception,
            string output,
            string error,
            string executedCommand)
            : base(exception, executedCommand)
        {
            ExitCode = exitCode;
            StdOut = output;
            StdErr = error;
        }

        public override bool HasExited { get => true; }

        public override int ExitCode { get; }

        public override string StdOut { get; }

        public override string StdErr { get; }

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

        public override string ToString()
        {
            return $"ExitCode: {ExitCode}" + Environment.NewLine +
                $"Exception: {Exception}" + Environment.NewLine +
                $"StdOut: {StdOut}" + Environment.NewLine +
                $"StdErr: {StdErr}";
        }
    }
}
