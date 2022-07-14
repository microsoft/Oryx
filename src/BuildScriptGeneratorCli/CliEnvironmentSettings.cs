// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    public class CliEnvironmentSettings
    {
        // Environment Variables from GitHub Actions.
        public const string GitHubActionsEnvVarName = "GITHUB_ACTIONS";
        public const string GitHubActionsBuildImagePullStartTimeEnvVarName = "GITHUB_ACTIONS_BUILD_IMAGE_PULL_START_TIME";
        public const string GitHubActionsBuildImagePullEndTimeEnvVarName = "GITHUB_ACTIONS_BUILD_IMAGE_PULL_END_TIME";

        public const string DebianFlavor = "DEBIAN_FLAVOR";

        private IEnvironment environment;

        public CliEnvironmentSettings(IEnvironment environment)
        {
            this.environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        // From the GitHub Actions environment variable documentation:
        // GITHUB_ACTIONS: Always set to true when GitHub Actions is running the workflow.
        public bool GitHubActions => this.IsEnvVariableTrue(GitHubActionsEnvVarName);

        /// <summary>
        /// Gets the time when a GitHub action starts to download build images for the container.
        /// </summary>
        public string GitHubActionsBuildImagePullStartTime => this.environment.GetEnvironmentVariable(
            GitHubActionsBuildImagePullStartTimeEnvVarName);

        /// <summary>
        /// Gets the time when a GitHub action finishes downloading build images and successfully built the container.
        /// </summary>
        public string GitHubActionsBuildImagePullEndTime => this.environment.GetEnvironmentVariable(
            GitHubActionsBuildImagePullEndTimeEnvVarName);

        private bool IsEnvVariableTrue(string disableEnvVarName)
        {
            var isDisabledVar = this.environment.GetBoolEnvironmentVariable(disableEnvVarName);
            if (isDisabledVar == true)
            {
                // The user has set the variable _and_ its value is true.
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}