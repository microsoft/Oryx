// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGenerator.DeploymentProgress
{
    internal sealed class BoundedDeploymentProgressWriter
    {
        internal static readonly TimeSpan StartupAllowance = TimeSpan.FromSeconds(2);
        internal static readonly TimeSpan IoTimeout = TimeSpan.FromSeconds(1);
        internal static readonly TimeSpan ProcessTimeout = StartupAllowance + IoTimeout;
        internal static readonly TimeSpan TerminationTimeout = TimeSpan.FromSeconds(1);

        private readonly string endpointPath;
        private readonly IDeploymentProgressWriterProcessFactory processFactory;

        public BoundedDeploymentProgressWriter(
            string endpointPath,
            IDeploymentProgressWriterProcessFactory processFactory = null)
        {
            this.endpointPath = endpointPath;
            this.processFactory = processFactory ?? new DeploymentProgressWriterProcessFactory();
        }

        public void AppendRecord(byte[] record)
        {
            IDeploymentProgressWriterProcess process = null;
            using (var cancellationSource = new CancellationTokenSource())
            {
                var cancellationToken = cancellationSource.Token;
                var writerTask = Task.Run(() =>
                {
                    using (var writerProcess = this.processFactory.Start(this.endpointPath))
                    {
                        Volatile.Write(ref process, writerProcess);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            writerProcess.Kill();
                            return;
                        }

                        writerProcess.StandardInput.Write(record, 0, record.Length);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            writerProcess.Kill();
                            return;
                        }

                        writerProcess.StandardInput.Dispose();
                        if (!writerProcess.WaitForExit(ProcessTimeout))
                        {
                            writerProcess.Kill();
                            throw new TimeoutException("Deployment progress writer timed out.");
                        }

                        if (writerProcess.ExitCode != 0)
                        {
                            throw new IOException("Deployment progress writer failed.");
                        }
                    }
                });

                if (Task.WhenAny(writerTask, Task.Delay(ProcessTimeout)).GetAwaiter().GetResult() != writerTask)
                {
                    cancellationSource.Cancel();
                    var startedProcess = Volatile.Read(ref process);
                    if (startedProcess != null)
                    {
                        _ = Task.Run(() => startedProcess.Kill());
                    }

                    _ = Task.WhenAny(writerTask, Task.Delay(TerminationTimeout)).GetAwaiter().GetResult();
                    throw new TimeoutException("Deployment progress writer timed out.");
                }

                writerTask.GetAwaiter().GetResult();
            }
        }
    }
}
