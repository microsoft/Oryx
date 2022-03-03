// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildServer.Models;

namespace Microsoft.Oryx.BuildServer.Services.ArtifactBuilders
{
    public class ArtifactBuilder : IArtifactBuilder
    {
        private readonly ILogger<ArtifactBuilder> _logger;

        public ArtifactBuilder(ILogger<ArtifactBuilder> logger)
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
                    RedirectStandardError = true,
                },
            };
            try
            {
                process.Start();
                _logger.LogInformation($"Process has started for command: {cmd}");
                var outputHandler = new FileOutputHandler(new StreamWriter(logFilePath), _logger);
                process.OutputDataReceived += outputHandler.Handle;
                process.ErrorDataReceived += outputHandler.Handle;
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                return process.ExitCode == 0;
            }
            finally
            {
                process.Dispose();
            }
        }
    }
}
