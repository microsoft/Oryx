// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Oryx.Common.Utilities
{
    public static class ProcessHelper
    {
        public static (int exitCode, string output) RunProcessAndCaptureOutput(
            string fileName,
            IEnumerable<string> arguments,
            int waitForExitInSeconds = 10)
        {
            var outputBuilder = new StringBuilder();

            var process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += DataReceivedHandler;
            process.ErrorDataReceived += DataReceivedHandler;

            foreach (var argument in arguments)
            {
                process.StartInfo.ArgumentList.Add(argument);
            }

            using (process)
            {
                var hasStarted = process.Start();
                if (!hasStarted)
                {
                    throw new InvalidOperationException(
                        "Process failed to start. The command used to run the process was:" +
                        Environment.NewLine +
                        $"{fileName} {string.Join(" ", arguments)}" +
                        Environment.NewLine +
                        "Output from standard output and error:" + outputBuilder.ToString());
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                var hasExited = process.WaitForExit((int)TimeSpan.FromSeconds(waitForExitInSeconds).TotalMilliseconds);
                if (!hasExited)
                {
                    throw new InvalidOperationException(
                        $"The process with id '{process.Id}' didn't exit within the allocated time." +
                        Environment.NewLine +
                        "Output from standard output and error:" + outputBuilder.ToString());
                }

                // From https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.waitforexit?view=netcore-2.1
                // When standard output has been redirected to asynchronous event handlers, it is possible that output
                // processing will not have completed when this method returns. To ensure that asynchronous
                // eventhandling has been completed, call the WaitForExit() overload that takes no parameter after
                // receiving a true from this overload

                process.WaitForExit();

                return (exitCode: process.ExitCode, output: outputBuilder.ToString());
            }

            void DataReceivedHandler(object sender, DataReceivedEventArgs e)
            {
                // Preserve the output structure and use AppendLine as this handler
                // is called for each line that is written to the output.
                outputBuilder.AppendLine(e.Data);
            }
        }
    }
}