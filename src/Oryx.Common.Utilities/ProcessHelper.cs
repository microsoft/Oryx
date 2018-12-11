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
            string workingDirectory,
            int? waitForExitInSeconds)
        {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            var exitCode = RunProcess(
                fileName,
                arguments,
                workingDirectory,
                standardOutputHandler: (sender, args) =>
                {
                    // Preserve the output structure and use AppendLine as these handlers
                    // are called for each line that is written to the output.
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
            string workingDirectory,
            DataReceivedEventHandler standardOutputHandler,
            DataReceivedEventHandler standardErrorHandler,
            int? waitForExitInSeconds)
        {
            var redirectOutput = standardOutputHandler != null;
            var redirectError = standardErrorHandler != null;

            Process process = null;
            try
            {
                process = StartProcess(
                    fileName,
                    arguments,
                    workingDirectory,
                    standardOutputHandler,
                    standardErrorHandler);

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
            finally
            {
                process?.Dispose();
            }
        }

        public static Process StartProcess(
            string fileName,
            IEnumerable<string> arguments,
            string workingDirectory,
            DataReceivedEventHandler standardOutputHandler,
            DataReceivedEventHandler standardErrorHandler)
        {
            var redirectOutput = standardOutputHandler != null;
            var redirectError = standardErrorHandler != null;

            var process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.CreateNoWindow = true;

            if (!string.IsNullOrEmpty(workingDirectory))
            {
                process.StartInfo.WorkingDirectory = workingDirectory;
            }

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
            try
            {
                var hasStarted = process.Start();
                if (!hasStarted)
                {
                    throw new InvalidOperationException(
                        "Process failed to start. The command used to run the process was:" +
                        Environment.NewLine +
                        $"{fileName} {string.Join(" ", arguments)}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error running an internal command '{fileName} {string.Join(" ", arguments)}': {e.Message}");
                throw;
            }

            if (redirectOutput)
            {
                process.BeginOutputReadLine();
            }

            if (redirectError)
            {
                process.BeginErrorReadLine();
            }

            return process;
        }
    }
}