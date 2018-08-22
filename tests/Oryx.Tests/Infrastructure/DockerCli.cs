// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Oryx.Tests.Infrastructure
{
    public class DockerCli
    {
        public DockerRunCommandResult Run(
            string imageId,
            List<EnvironmentVariable> environmentVariables,
            List<DockerVolume> volumes,
            string command,
            string[] commandArguments)
        {
            if (string.IsNullOrEmpty(imageId))
            {
                throw new ArgumentException($"'{nameof(imageId)}' cannot be null or empty.");
            }

            // Generate a unique container name for each 'run' call.
            // Provide a prefix so that one can delete the containers using regex, if needed
            var containerName = $"oryxtests_{Guid.NewGuid().ToString("N")}";

            var fileName = "docker";
            var arguments = PrepareArguments();

            var output = string.Empty;
            int exitCode = -1;
            Exception exception = null;
            try
            {
                (exitCode, output) = ProcessHelper.RunProcessAndCaptureOutput(fileName, arguments);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                exception = invalidOperationException;
            }

            return new DockerRunCommandResult(
                containerName,
                exitCode,
                exception,
                output,
                volumes,
                $"{fileName} {string.Join(" ", arguments)}");

            IEnumerable<string> PrepareArguments()
            {
                var args = new List<string>();
                args.Add("run");
                args.Add("--name");
                args.Add(containerName);

                if (environmentVariables != null)
                {
                    foreach (var environmentVariable in environmentVariables)
                    {
                        args.Add("-e");
                        args.Add($"{environmentVariable.Key}={environmentVariable.Value}");
                    }
                }

                if (volumes != null)
                {
                    foreach (var volume in volumes)
                    {
                        args.Add("-v");
                        args.Add($"{volume.HostDir}:/{volume.ContainerDir.TrimStart('/')}");
                    }
                }

                args.Add(imageId);
                args.Add(command);

                if (commandArguments != null)
                {
                    args.AddRange(commandArguments);
                }
                return args;
            }
        }
    }
}
