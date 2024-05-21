// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;

namespace Microsoft.Oryx.BuildScriptGenerator.Common
{
    public static class ProcessHelper
    {
        public static (int ExitCode, string Output, string Error) RunProcess(
            string fileName,
            IEnumerable<string> arguments,
            string workingDirectory,
            TimeSpan? waitTimeForExit)
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
                waitTimeForExit);

            return (exitCode, Output: outputBuilder.ToString(), Error: errorBuilder.ToString());
        }

        public static int RunProcess(
            string fileName,
            IEnumerable<string> arguments,
            string workingDirectory,
            DataReceivedEventHandler standardOutputHandler,
            DataReceivedEventHandler standardErrorHandler,
            TimeSpan? waitTimeForExit)
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

                if (waitTimeForExit.HasValue)
                {
                    var hasExited = process.WaitForExit((int)waitTimeForExit.Value.TotalMilliseconds);
                    if (!hasExited)
                    {
                        throw new InvalidOperationException(
                            $"Process {process.Id} didn't exit within the allotted time.");
                    }

                    if (redirectOutput || redirectError)
                    {
                        // From:https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.waitforexit?view=netcore-2.1
                        // When standard output has been redirected to asynchronous event handlers,
                        // it is possible that output processing will not have completed when this method returns.
                        // To ensure that asynchronous eventhandling has been completed, call the WaitForExit()
                        // overload that takes no parameter after receiving a true from this overload
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

#pragma warning disable CA1416 // Validate platform compatibility
            var hasStarted = process.Start();
#pragma warning restore CA1416 // Validate platform compatibility
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

            return process;
        }

        public static int TrySetExecutableMode([CanBeNull] string path, [CanBeNull] string workingDir = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return ProcessConstants.ExitFailure;
            }

            try
            {
                return RunProcess(
                    "chmod",
                    new string[] { "+rx", path },
                    workingDir ?? System.IO.Path.GetDirectoryName(path),
                    null).ExitCode;
            }
            catch
            {
                return ProcessConstants.ExitFailure;
            }
        }
    }
}
