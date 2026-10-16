// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Oryx.BuildScriptGenerator.DeploymentProgress
{
    internal sealed class DeploymentProgressWriterProcess : IDeploymentProgressWriterProcess
    {
        private readonly Process process;

        public DeploymentProgressWriterProcess(Process process)
        {
            this.process = process ?? throw new InvalidOperationException(
                "Deployment progress writer process failed to start.");
            this.process.OutputDataReceived += (sender, args) => { };
            this.process.ErrorDataReceived += (sender, args) => { };
            this.process.BeginOutputReadLine();
            this.process.BeginErrorReadLine();
        }

        public Stream StandardInput => this.process.StandardInput.BaseStream;

        public int ExitCode => this.process.ExitCode;

        public bool WaitForExit(TimeSpan timeout)
        {
            return this.process.WaitForExit((int)timeout.TotalMilliseconds);
        }

        public void Kill()
        {
            this.process.Kill(entireProcessTree: true);
        }

        public void Dispose()
        {
            this.process.Dispose();
        }
    }
}
