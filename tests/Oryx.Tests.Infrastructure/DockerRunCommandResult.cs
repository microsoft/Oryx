// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oryx.Tests.Infrastructure
{
    public class DockerRunCommandResult : DockerCommandResult
    {
        public DockerRunCommandResult(
            string containerName,
            int exitCode,
            Exception exception,
            string output,
            List<DockerVolume> volumes,
            string executedRunCommand)
            : base(exitCode, exception, output, executedRunCommand)
        {
            ContainerName = containerName;
            Volumes = volumes;
        }

        public string ContainerName { get; }

        private List<DockerVolume> Volumes { get; }

        public override string GetDebugInfo()
        {
            var volumeList = string.Empty;
            if (Volumes?.Count > 0)
            {
                volumeList = string.Join(
                    " ",
                    Volumes.Select(kvp => $"-v {kvp.HostDir}:/{kvp.ContainerDir.TrimStart('/')}:ro"));
            }

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Debugging Information:");
            sb.AppendLine("----------------------");
            sb.AppendLine($"Executed command: {ExecutedCommand}");
            sb.AppendLine($"Exit code: {ExitCode}");
            sb.AppendLine($"Output: {Output}");
            sb.AppendLine($"Exception: {Exception?.Message}");
            sb.AppendLine();
            sb.AppendLine("Use the following commands to investigate the failed container:");
            sb.AppendLine($"docker logs {ContainerName}");
            sb.AppendLine();
            sb.AppendLine($"docker commit {ContainerName} investigate_{ContainerName}");
            sb.AppendLine();
            sb.AppendLine($"docker run -it {volumeList} investigate_{ContainerName} /bin/bash");
            return sb.ToString();
        }
    }
}