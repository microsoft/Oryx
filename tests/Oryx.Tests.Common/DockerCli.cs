// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.Tests.Common
{
    public class DockerCli
    {
        private const string CreatedContainerPrefix = "oryxtests_";

        private readonly TimeSpan _waitTimeForExit;
        private readonly IEnumerable<EnvironmentVariable> _globalEnvVars;

        public DockerCli(IEnumerable<EnvironmentVariable> globalEnvVars = null)
            : this(TimeSpan.FromMinutes(10), globalEnvVars)
        {
        }

        public DockerCli(TimeSpan waitTimeForExit, IEnumerable<EnvironmentVariable> globalEnvVars = null)
        {
            _waitTimeForExit = waitTimeForExit;
            _globalEnvVars = globalEnvVars;
        }

        public DockerRunCommandResult Run(DockerRunArguments dockerRunArguments)
        {
            if (dockerRunArguments == null)
            {
                throw new ArgumentNullException(nameof(dockerRunArguments));
            }

            if (string.IsNullOrEmpty(dockerRunArguments.ImageId))
            {
                throw new ArgumentException(
                    $"'{nameof(dockerRunArguments)}.{nameof(dockerRunArguments.ImageId)}' cannot be null or empty.");
            }

            var output = string.Empty;
            var error = string.Empty;
            int exitCode = -1;
            Exception exception = null;

            // Generate a unique container name for each 'run' call.
            // Provide a prefix so that one can delete the containers using regex, if needed
            var containerName = $"{CreatedContainerPrefix}{Guid.NewGuid().ToString("N")}";

            var fileName = "docker";
            var arguments = PrepareDockerRunArguments(containerName, dockerRunArguments);

            try
            {
                (exitCode, output, error) = ProcessHelper.RunProcess(
                        fileName,
                        arguments,
                        workingDirectory: null,
                        _waitTimeForExit);
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
                dockerRunArguments.Volumes,
                $"{fileName} {string.Join(" ", arguments)}");
        }

        public DockerRunCommandProcessResult RunAndDoNotWaitForProcessExit(DockerRunArguments dockerRunArguments)
        {
            if (dockerRunArguments == null)
            {
                throw new ArgumentNullException(nameof(dockerRunArguments));
            }

            if (string.IsNullOrEmpty(dockerRunArguments.ImageId))
            {
                throw new ArgumentException(
                    $"'{nameof(dockerRunArguments)}.{nameof(dockerRunArguments.ImageId)}' cannot be null or empty.");
            }

            Process process = null;
            Exception exception = null;
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            // Generate a unique container name for each 'run' call.
            // Provide a prefix so that one can delete the containers using regex, if needed
            var containerName = $"{CreatedContainerPrefix}{Guid.NewGuid().ToString("N")}";

            // Make sure not to run the container in background
            dockerRunArguments.RunContainerInBackground = false;

            var fileName = "docker";
            var arguments = PrepareDockerRunArguments(containerName, dockerRunArguments);

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

            return ExecuteCommand(new[] { "stop", containerName });
        }

        public string GetContainerStatus(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException($"'{nameof(containerName)}' cannot be null or empty.");
            }

            var result = ExecuteCommand(new[] { "ps", "--filter", $"name={containerName}", "--format", "{{.Status}}" });
            return result.StdOut.Trim();
        }

        public (string stdOut, string stdErr) GetContainerLogs(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException($"'{nameof(containerName)}' cannot be null or empty.");
            }

            var result = ExecuteCommand(new[] { "logs", containerName });
            return (stdOut: result.StdOut, stdErr: result.StdErr);
        }

        public DockerCommandResult Exec(string containerName, string command, string[] commandArgs)
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
                args.Add("exec");
                args.Add(containerName);
                args.Add(command);

                if (commandArgs?.Length > 0)
                {
                    args.AddRange(commandArgs);
                }
                return args;
            }
        }

        public DockerCommandResult GetPortMapping(string containerName, int portInContainer)
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
                args.Add("port");
                args.Add(containerName);
                args.Add(portInContainer.ToString());
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
                    _waitTimeForExit);
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

        private static void AddEnvVarArg([NotNull] List<string> args, EnvironmentVariable newVar)
        {
            args.Add("-e");
            args.Add($"{newVar.Key}={newVar.Value}");
        }

        private static void AddEnvVarArgs(
            [NotNull] List<string> args,
            [CanBeNull] IEnumerable<EnvironmentVariable> newVars)
        {
            if (newVars == null)
            {
                return;
            }

            foreach (var envVar in newVars)
            {
                AddEnvVarArg(args, envVar);
            }
        }

        private IEnumerable<string> PrepareDockerRunArguments(
            string containerName,
            DockerRunArguments dockerRunArguments)
        {
            var args = new List<string>();
            args.Add("run");
            args.Add("--name");
            args.Add(containerName);

            // By default we want to remove containers that are created when running tests.
            var removeContainers = Environment.GetEnvironmentVariable(
                Settings.RemoveTestContainersEnvironmentVariableName);
            if (string.IsNullOrEmpty(removeContainers)
                || !string.Equals(removeContainers, "false", StringComparison.OrdinalIgnoreCase))
            {
                args.Add("--rm");
            }

            if (dockerRunArguments.RunContainerInBackground)
            {
                args.Add("-d");
            }

            AddEnvVarArgs(args, _globalEnvVars);
            AddEnvVarArgs(args, dockerRunArguments.EnvironmentVariables);

            var aiKeyOverride = Environment.GetEnvironmentVariable(
                "TEST_OVERRIDE_" + LoggingConstants.ApplicationInsightsInstrumentationKeyEnvironmentVariableName);
            if (!string.IsNullOrWhiteSpace(aiKeyOverride))
            {
                AddEnvVarArg(
                    args,
                    new EnvironmentVariable(
                        LoggingConstants.ApplicationInsightsInstrumentationKeyEnvironmentVariableName,
                        aiKeyOverride));
            }

            var appServiceAppName = Environment.GetEnvironmentVariable(
                LoggingConstants.AppServiceAppNameEnvironmentVariableName);
            if (!string.IsNullOrWhiteSpace(appServiceAppName))
            {
                AddEnvVarArg(
                    args,
                    new EnvironmentVariable(
                        LoggingConstants.AppServiceAppNameEnvironmentVariableName,
                        appServiceAppName));
            }

            if (dockerRunArguments.Volumes?.Count > 0)
            {
                foreach (var volume in dockerRunArguments.Volumes)
                {
                    args.Add("-v");
                    args.Add($"{volume.MountedHostDir}:{volume.ContainerDir}");
                }
            }

            if (!string.IsNullOrEmpty(dockerRunArguments.Link))
            {
                args.Add("--link");
                args.Add(dockerRunArguments.Link);
            }

            if (dockerRunArguments.PortInContainer.HasValue)
            {
                args.Add("-p");
                args.Add(dockerRunArguments.PortInContainer.ToString());
            }

            args.Add(dockerRunArguments.ImageId);

            if (!string.IsNullOrEmpty(dockerRunArguments.CommandToExecuteOnRun))
            {
                args.Add(dockerRunArguments.CommandToExecuteOnRun);
            }

            if (dockerRunArguments.CommandArguments?.Length > 0)
            {
                args.AddRange(dockerRunArguments.CommandArguments);
            }

            return args;
        }
    }
}