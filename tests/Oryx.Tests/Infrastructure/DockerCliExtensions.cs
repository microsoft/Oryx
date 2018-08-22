// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Oryx.Tests.Infrastructure
{
    public static class DockerCliExtensions
    {
        public static DockerRunCommandResult Run(
            this DockerCli dockerCli,
            string imageId,
            string commandToExecuteOnRun)
        {
            return dockerCli.Run(
                imageId,
                environmentVariable: null,
                volume: null,
                commandToExecuteOnRun,
                commandArguments: null);
        }

        public static DockerRunCommandResult Run(
            this DockerCli dockerCli,
            string imageId,
            string commandToExecuteOnRun,
            string[] commandArguments)
        {
            return dockerCli.Run(
                imageId,
                environmentVariable: null,
                volume: null,
                commandToExecuteOnRun,
                commandArguments);
        }

        public static DockerRunCommandResult Run(
            this DockerCli dockerCli,
            string imageId,
            DockerVolume volume,
            string commandToExecuteOnRun)
        {
            return dockerCli.Run(
                imageId,
                environmentVariable: null,
                volume,
                commandToExecuteOnRun,
                commandArguments: null);
        }

        public static DockerRunCommandResult Run(
            this DockerCli dockerCli,
            string imageId,
            DockerVolume volume,
            string commandToExecuteOnRun,
            string[] commandArguments)
        {
            return dockerCli.Run(
                imageId,
                environmentVariable: null,
                volume,
                commandToExecuteOnRun,
                commandArguments);
        }

        public static DockerRunCommandResult Run(
            this DockerCli dockerCli,
            string imageId,
            EnvironmentVariable environmentVariable,
            string commandToExecuteOnRun)
        {
            return dockerCli.Run(
                imageId,
                environmentVariable,
                volume: null,
                commandToExecuteOnRun,
                commandArguments: null);
        }

        public static DockerRunCommandResult Run(
            this DockerCli dockerCli,
            string imageId,
            EnvironmentVariable environmentVariable,
            string commandToExecuteOnRun,
            string[] commandArguments)
        {
            return dockerCli.Run(
                imageId,
                environmentVariable,
                volume: null,
                commandToExecuteOnRun,
                commandArguments);
        }

        public static DockerRunCommandResult Run(
            this DockerCli dockerCli,
            string imageId,
            EnvironmentVariable environmentVariable,
            DockerVolume volume,
            string commandToExecuteOnRun)
        {
            return dockerCli.Run(
                imageId,
                environmentVariable,
                volume,
                commandToExecuteOnRun,
                commandArguments: null);
        }

        public static DockerRunCommandResult Run(
            this DockerCli dockerCli,
            string imageId,
            EnvironmentVariable environmentVariable,
            DockerVolume volume,
            string commandToExecuteOnRun,
            string[] commandArguments)
        {
            var environmentVariables = new List<EnvironmentVariable>();
            if (environmentVariable != null)
            {
                environmentVariables.Add(environmentVariable);
            }

            var volumes = new List<DockerVolume>();
            if (volume != null)
            {
                volumes.Add(volume);
            }

            return dockerCli.Run(
                imageId,
                environmentVariables,
                volumes,
                commandToExecuteOnRun,
                commandArguments);
        }
    }
}
