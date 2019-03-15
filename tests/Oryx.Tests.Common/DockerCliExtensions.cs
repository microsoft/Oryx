// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.Tests.Common
{
    public static class DockerCliExtensions
    {
        public static DockerRunCommandResult Run(
            this DockerCli dockerCli,
            string imageId,
            string commandToExecuteOnRun,
            string[] commandArguments)
        {
            return Run(
                dockerCli,
                imageId,
                environmentVariable: null,
                volume: null,
                portMapping: null,
                link: null,
                runContainerInBackground: false,
                commandToExecuteOnRun,
                commandArguments);
        }

        public static DockerRunCommandResult Run(
            this DockerCli dockerCli,
            string imageId,
            DockerVolume volume,
            string commandToExecuteOnRun,
            string[] commandArguments)
        {
            return Run(
                dockerCli,
                imageId,
                environmentVariable: null,
                volume,
                portMapping: null,
                link: null,
                runContainerInBackground: false,
                commandToExecuteOnRun,
                commandArguments);
        }

        public static DockerRunCommandResult Run(
            this DockerCli dockerCli,
            string imageId,
            DockerVolume volume,
            string portMapping,
            string commandToExecuteOnRun,
            string[] commandArguments)
        {
            return Run(
                dockerCli,
                imageId,
                environmentVariable: null,
                volume,
                portMapping,
                link: null,
                runContainerInBackground: false,
                commandToExecuteOnRun,
                commandArguments);
        }

        public static DockerRunCommandResult Run(
            this DockerCli dockerCli,
            string imageId,
            EnvironmentVariable environmentVariable,
            string commandToExecuteOnRun,
            string[] commandArguments)
        {
            return Run(
                dockerCli,
                imageId,
                environmentVariable,
                volume: null,
                portMapping: null,
                link: null,
                runContainerInBackground: false,
                commandToExecuteOnRun,
                commandArguments);
        }

        public static DockerRunCommandResult Run(
            this DockerCli dockerCli,
            string imageId,
            EnvironmentVariable environmentVariable,
            DockerVolume volume,
            string commandToExecuteOnRun,
            string[] commandArguments)
        {
            return Run(
                dockerCli,
                imageId,
                environmentVariable,
                volume,
                portMapping: null,
                link: null,
                runContainerInBackground: false,
                commandToExecuteOnRun,
                commandArguments);
        }

        public static DockerRunCommandResult Run(
            this DockerCli dockerCli,
            string imageId,
            EnvironmentVariable environmentVariable,
            DockerVolume volume,
            string portMapping,
            string link,
            bool runContainerInBackground,
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
                portMapping,
                link,
                runContainerInBackground,
                commandToExecuteOnRun,
                commandArguments);
        }

        public static DockerRunCommandResult Run(
            this DockerCli dockerCli,
            string imageId,
            List<EnvironmentVariable> environmentVariables,
            List<DockerVolume> volumes,
            string portMapping,
            string link,
            bool runContainerInBackground,
            string commandToExecuteOnRun,
            string[] commandArguments)
        {
            return dockerCli.Run(
                imageId,
                environmentVariables,
                volumes,
                portMapping,
                link,
                runContainerInBackground,
                commandToExecuteOnRun,
                commandArguments);
        }
    }
}
