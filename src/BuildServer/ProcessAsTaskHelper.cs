// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using RunProcessAsTask;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildServer
{
    public static class ProcessAsTaskHelper
    {
        public static async Task<ProcessResults> RunProcessAsync(
            string fileName,
            IEnumerable<string> arguments,
            string workingDirectory,
            TimeSpan? waitTimeForExit)
        {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            var exitCode = await RunProcessAsync(
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
                (TimeSpan)waitTimeForExit);

            return exitCode;
        }

        public static async Task<ProcessResults> RunProcessAsync(
            string fileName,
            IEnumerable<string> arguments,
            string workingDirectory,
            DataReceivedEventHandler standardOutputHandler,
            DataReceivedEventHandler standardErrorHandler,
            TimeSpan waitTimeForExit)
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
                using (var cancellationTokenSource = new CancellationTokenSource(waitTimeForExit))
                {
                    var processResults = await ProcessEx.RunAsync(process.StartInfo, cancellationTokenSource.Token);
                    return processResults;
                }

            }
            catch (OperationCanceledException)
            {
                throw new OperationCanceledException(
                    $"Process failed to finish within {waitTimeForExit}, The command used to run the process was:" +
                    Environment.NewLine +
                    $"{fileName} {string.Join(" ", arguments)}");
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException(
                    "Process failed to start. The command used to run the process was:" +
                    Environment.NewLine +
                    $"{fileName} {string.Join(" ", arguments)}");
            }

            return null;
        }
    }
}