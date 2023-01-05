// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildServer.Models;

namespace Microsoft.Oryx.BuildServer.Services.ArtifactBuilders
{
    public class ArtifactBuilder : IArtifactBuilder
    {
        private readonly ILogger<ArtifactBuilder> logger;

        public ArtifactBuilder(ILogger<ArtifactBuilder> logger)
        {
            this.logger = logger;
        }

        public bool Build(Build build)
        {
            // TODO: improve validation by using semantic versioning,
            // absolute path regex, platform names in a set.
            var logFilePath = this.ValidateParameter($"{build.LogPath}/{build.Id}.log");
            var sourcePath = this.ValidateParameter(build.SourcePath);
            var outputPath = this.ValidateParameter($"{build.OutputPath}/{build.Id}");
            var platform = this.ValidateParameter(build.Platform);
            var version = this.ValidateParameter(build.Version);
            Directory.CreateDirectory(outputPath);
            var cmd = $"oryx build {sourcePath} --log-file {logFilePath} " +
                $"--output {outputPath} --platform {platform} " +
                $"--platform-version {version}";
            cmd = cmd.Replace("'", "\\'");
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
                this.logger.LogInformation($"Process has started for command: {cmd}");
                var outputHandler = new FileOutputHandler(new StreamWriter(logFilePath), this.logger);
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

        private string ValidateParameter(string parameter)
        {
            Regex regex = new Regex(@"^[a-zA-Z0-9./\-_]*$");
            Match match = regex.Match(parameter);
            if (match.Success)
            {
                return parameter;
            }

            this.logger.LogError($"Invalid parameter provided: {parameter}\n" +
                $"Only alpha-numeric, '/', '.', '-', '_' characters are allowed");
            return null;
        }
    }
}
