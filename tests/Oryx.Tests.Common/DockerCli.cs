// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Oryx.Common.Utilities;

namespace Oryx.Tests.Common
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
            string portMapping,
            bool runContainerInBackground,
            string command,
            string[] commandArguments)
        {
            if (string.IsNullOrEmpty(imageId))
            {
                throw new ArgumentException($"'{nameof(imageId)}' cannot be null or empty.");
            }

            var output = string.Empty;
            var error = string.Empty;
            int exitCode = -1;
            Exception exception = null;

            // Generate a unique container name for each 'run' call.
            // Provide a prefix so that one can delete the containers using regex, if needed
            var containerName = $"{CreatedContainerPrefix}{Guid.NewGuid().ToString("N")}";

            var fileName = "docker";
            var arguments = PrepareDockerRunArguments(
                containerName,
                runContainerInBackground,
                environmentVariables,
                volumes,
                portMapping,
                imageId,
                command,
                commandArguments);

            try
            {
                (exitCode, output, error) = ProcessHelper.RunProcess(
                        fileName,
                        arguments,
                        workingDirectory: null,
                        _waitTimeInSeconds);
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
                error,
                volumes,
                $"{fileName} {string.Join(" ", arguments)}");
        }

        public DockerRunCommandProcessResult RunAndDoNotWaitForProcessExit(
            string imageId,
            List<EnvironmentVariable> environmentVariables,
            List<DockerVolume> volumes,
            string portMapping,
            string command,
            string[] commandArguments)
        {
            if (string.IsNullOrEmpty(imageId))
            {
                throw new ArgumentException($"'{nameof(imageId)}' cannot be null or empty.");
            }

            Process process = null;
            Exception exception = null;
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            // Generate a unique container name for each 'run' call.
            // Provide a prefix so that one can delete the containers using regex, if needed
            var containerName = $"{CreatedContainerPrefix}{Guid.NewGuid().ToString("N")}";

            var fileName = "docker";
            var arguments = PrepareDockerRunArguments(
                containerName,
                runContainerInBackground: false,
                environmentVariables,
                volumes,
                portMapping,
                imageId,
                command,
                commandArguments);

            try
            {
                process = ProcessHelper.StartProcess(
                    fileName,
                    arguments,
                    workingDirectory: null,
                    // Preserve the output structure and use AppendLine as these handlers
                    // are called for each line that is written to the output.
                    standardOutputHandler: (sender, args) =>
                    {
                        outputBuilder.AppendLine(args.Data);
                    },
                    standardErrorHandler: (sender, args) =>
                    {
                        errorBuilder.AppendLine(args.Data);
                    });
            }
            catch (InvalidOperationException invalidOperationException)
            {
                exception = invalidOperationException;
            }

            return new DockerRunCommandProcessResult(
                containerName,
                process,
                exception,
                outputBuilder,
                errorBuilder,
                $"{fileName} {string.Join(" ", arguments)}");
        }

        public DockerCommandResult RemoveContainer(string containerName, bool forceRemove)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException($"'{nameof(containerName)}' cannot be null or empty.");
            }

            var arguments = PrepareArguments();
            return ExecuteCommand(arguments);

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

        public DockerCommandResult StopContainer(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException($"'{nameof(containerName)}' cannot be null or empty.");
            }

            var arguments = PrepareArguments();
            return ExecuteCommand(arguments);

            IEnumerable<string> PrepareArguments()
            {
                var args = new List<string>();
                args.Add("stop");
                args.Add(containerName);
                return args;
            }
        }

        public DockerCommandResult Logs(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException($"'{nameof(containerName)}' cannot be null or empty.");
            }

            var arguments = PrepareArguments();
            return ExecuteCommand(arguments);

            IEnumerable<string> PrepareArguments()
            {
                var args = new List<string>();
                args.Add("logs");
                args.Add(containerName);
                return args;
            }
        }

        private DockerCommandResult ExecuteCommand(IEnumerable<string> arguments)
        {
            var fileName = "docker";

            var output = string.Empty;
            var error = string.Empty;
            int exitCode = -1;
            Exception exception = null;
            try
            {
                (exitCode, output, error) = ProcessHelper.RunProcess(
                    fileName,
                    arguments,
                    workingDirectory: null,
                    _waitTimeInSeconds);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                exception = invalidOperationException;
            }

            return new DockerCommandResult(
                exitCode,
                exception,
                output,
                error,
                $"{fileName} {string.Join(" ", arguments)}");
        }

        private IEnumerable<string> PrepareDockerRunArguments(
            string containerName,
            bool runContainerInBackground,
            List<EnvironmentVariable> environmentVariables,
            List<DockerVolume> volumes,
            string portMapping,
            string imageId,
            string command,
            string[] commandArguments)
        {
            var args = new List<string>();
            args.Add("run");
            args.Add("--name");
            args.Add(containerName);

            if (runContainerInBackground)
            {
                args.Add("-d");
            }

            if (environmentVariables?.Count > 0)
            {
                foreach (var environmentVariable in environmentVariables)
                {
                    args.Add("-e");
                    args.Add($"{environmentVariable.Key}={environmentVariable.Value}");
                }
            }

            if (volumes?.Count > 0)
            {
                foreach (var volume in volumes)
                {
                    args.Add("-v");
                    args.Add($"{volume.MountedHostDir}:{volume.ContainerDir}");
                }
            }

            if (!string.IsNullOrEmpty(portMapping))
            {
                args.Add("-p");
                args.Add(portMapping);
            }

            args.Add(imageId);

            args.Add(command);

            if (commandArguments?.Length > 0)
            {
                args.AddRange(commandArguments);
            }

            return args;
        }
    }
}
