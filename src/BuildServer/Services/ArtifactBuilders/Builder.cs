// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildServer.Models;
using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Oryx.BuildServer.Services.ArtifactBuilders
{
    class FileOutputHandler
    {
        private StreamWriter _fileStream;
        private readonly ILogger<Builder> _logger;

        public FileOutputHandler(StreamWriter filestream, ILogger<Builder> logger)
        {
            _fileStream = filestream;
            _logger = logger;
        }

        public void handle(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                _fileStream.Write(outLine.Data + "\n");
                _logger.LogInformation(outLine.Data);
            }
        }
    }
    public class Builder : IArtifactBuilder
    {
        private readonly ILogger<Builder> _logger;

        public Builder(ILogger<Builder> logger)
        {
            _logger = logger;
        }

        public bool Build(Build build)
        {
            var logFilePath = $"{build.LogPath}/{build.Id}.log";
            var sourcePath = build.SourcePath;
            var outputPath = $"{build.OutputPath}/{build.Id}";
            Directory.CreateDirectory(outputPath);
            var cmd = $"oryx build {sourcePath} --log-file {logFilePath} " +
                $"--output {outputPath} --platform {build.Platform} " +
                $"--platform-version {build.Version}";
            var escapedArgs = cmd.Replace("\"", "\\\"");
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{cmd}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                }
            };
            process.Start();
            _logger.LogInformation($"Process has started for command: {cmd}");
            var outputHandler = new FileOutputHandler(new StreamWriter(logFilePath), _logger);
            process.OutputDataReceived += outputHandler.handle;
            process.ErrorDataReceived += outputHandler.handle;
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
