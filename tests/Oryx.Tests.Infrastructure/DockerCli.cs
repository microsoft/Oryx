// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;

namespace Oryx.Tests.Infrastructure
{
    public class DockerCli
    {
        private const string CreatedContainerPrefix = "oryxtests_";

        private readonly int _waitTimeInSeconds;

        public DockerCli()
        {
            _waitTimeInSeconds = 10;
        }

        public DockerCli(int waitTimeInSeconds)
        {
            _waitTimeInSeconds = waitTimeInSeconds;
        }

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
            var containerName = $"{CreatedContainerPrefix}{Guid.NewGuid().ToString("N")}";

            var fileName = "docker";
            var arguments = PrepareArguments();

            var output = string.Empty;
            int exitCode = -1;
            Exception exception = null;
            try
            {
                (exitCode, output) = ProcessHelper.RunProcessAndCaptureOutput(fileName, arguments, _waitTimeInSeconds);
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

                if (environmentVariables?.Count > 0)
                {
                    foreach (var environmentVariable in environmentVariables)
                    {
                        args.Add("-e");
                        args.Add($"{environmentVariable.Key}={environmentVariable.Value}");
                    }
                }

                DockerVolume testScriptsVolume = null;
                if (volumes?.Count > 0)
                {
                    var hostTestScriptsDir = Path.Combine(Directory.GetCurrentDirectory(), "TestScripts");
                    testScriptsVolume = DockerVolume.Create(hostTestScriptsDir);

                    volumes.Add(testScriptsVolume);

                    foreach (var volume in volumes)
                    {
                        args.Add("-v");
                        // Always mount as read only to prevent the containers (which run as 'root' by default)
                        // from writing to the host's directory (which does not run as 'root', for example on 
                        // the build agent).
                        args.Add($"{volume.HostDir}:{volume.ReadOnlyContainerDir}:ro");
                    }
                }

                args.Add(imageId);

                if (volumes?.Count > 0)
                {
                    args.Add($"{testScriptsVolume.ReadOnlyContainerDir}/copyVolumesAndExecuteCommand.sh");
                    args.Add(DockerVolume.ReadOnlyDirRootInContainer);
                    args.Add(DockerVolume.WritableDirRootInContainer);
                }

                args.Add(command);

                if (commandArguments?.Length > 0)
                {
                    args.AddRange(commandArguments);
                }

                return args;
            }
        }

        public DockerCommandResult RemoveContainer(string containerName, bool forceRemove)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException($"'{nameof(containerName)}' cannot be null or empty.");
            }

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

            return new DockerCommandResult(
                exitCode,
                exception,
                output,
                $"{fileName} {string.Join(" ", arguments)}");

            IEnumerable<string> PrepareArguments()
            {
                var args = new List<string>();
                args.Add("rm");

                if (forceRemove)
                {
                    args.Add("--force");
                }

                args.Add("--volumes");
                args.Add(containerName);
                return args;
            }
        }
    }
}
