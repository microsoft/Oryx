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
        public static (int exitCode, string output, string error) RunProcess(
            string fileName,
            IEnumerable<string> arguments,
            int? waitForExitInSeconds)
        {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            var exitCode = RunProcess(
                fileName,
                arguments,
                // Preserve the output structure and use AppendLine as these handlers
                // are called for each line that is written to the output.
                standardOutputHandler: (sender, args) =>
                {
                    outputBuilder.AppendLine(args.Data);
                },
                standardErrorHandler: (sender, args) =>
                {
                    errorBuilder.AppendLine(args.Data);
                },
                waitForExitInSeconds);

            return (exitCode, output: outputBuilder.ToString(), error: errorBuilder.ToString());
        }

        public static int RunProcess(
            string fileName,
            IEnumerable<string> arguments,
            DataReceivedEventHandler standardOutputHandler,
            DataReceivedEventHandler standardErrorHandler,
            int? waitForExitInSeconds)
        {
            var redirectOutput = standardOutputHandler != null;
            var redirectError = standardErrorHandler != null;

            var process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.CreateNoWindow = true;
            if (redirectOutput)
            {
                process.StartInfo.RedirectStandardOutput = true;
                process.OutputDataReceived += standardOutputHandler;
            }

            if (redirectError)
            {
                process.StartInfo.RedirectStandardError = true;
                process.ErrorDataReceived += standardErrorHandler;
            }

            if (arguments != null)
            {
                foreach (var argument in arguments)
                {
                    process.StartInfo.ArgumentList.Add(argument);
                }
            }

            using (process)
            {
                var hasStarted = process.Start();
                if (!hasStarted)
                {
                    throw new InvalidOperationException(
                        "Process failed to start. The command used to run the process was:" +
                        Environment.NewLine +
                        $"{fileName} {string.Join(" ", arguments)}");
                }

                if (redirectOutput)
                {
                    process.BeginOutputReadLine();
                }

                if (redirectError)
                {
                    process.BeginErrorReadLine();
                }

                if (waitForExitInSeconds.HasValue)
                {
                    var hasExited = process.WaitForExit(
                        (int)TimeSpan.FromSeconds(waitForExitInSeconds.Value).TotalMilliseconds);
                    if (!hasExited)
                    {
                        throw new InvalidOperationException(
                            $"The process with id '{process.Id}' didn't exit within the allocated time.");
                    }

                    if (redirectOutput || redirectError)
                    {
                        // From https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.waitforexit?view=netcore-2.1
                        // When standard output has been redirected to asynchronous event handlers, it is possible that output
                        // processing will not have completed when this method returns. To ensure that asynchronous
                        // eventhandling has been completed, call the WaitForExit() overload that takes no parameter after
                        // receiving a true from this overload
                        process.WaitForExit();
                    }
                }
                else
                {
                    process.WaitForExit();
                }

                return process.ExitCode;
            }
        }
    }
}