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
        private const string DisableCheckersEnvVarName = "DISABLE_CHECKERS";
        private const string DisableDotNetCoreEnvVarName = "DISABLE_DOTNETCORE_BUILD";
        private const string DisablePythonEnvVarName = "DISABLE_PYTHON_BUILD";
        private const string DisablePhpEnvVarName = "DISABLE_PHP_BUILD";
        private const string DisableNodeJsEnvVarName = "DISABLE_NODEJS_BUILD";
        private const string EnableMultiPlatformBuildEnvVarName = "ENABLE_MULTIPLATFORM_BUILD";
        private const string GitHubActionsEnvVarName = "GITHUB_ACTIONS";
        private const string GitHubActionsBuildContainerStartTimeEnvVarName = "GITHUB_BUILD_CONTAINER_START";
        private const string GitHubActionsBuildContainerEndTimeEnvVarName = "GITHUB_BUILD_CONTAINER_COMPLETE";

        private IEnvironment _environment;

        public CliEnvironmentSettings(IEnvironment environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public bool DisableCheckers => IsEnvVariableTrue(DisableCheckersEnvVarName);

        public bool DisableDotNetCore => IsEnvVariableTrue(DisableDotNetCoreEnvVarName);

        public bool DisableNodeJs => IsEnvVariableTrue(DisableNodeJsEnvVarName);

        public bool DisablePython => IsEnvVariableTrue(DisablePythonEnvVarName);

        public bool DisablePhp => IsEnvVariableTrue(DisablePhpEnvVarName);

        // From the GitHub Actions environment variable documentation:
        // GITHUB_ACTIONS: Always set to true when GitHub Actions is running the workflow.
        public bool GitHubActions => IsEnvVariableTrue(GitHubActionsEnvVarName);

        /// <summary>
        /// Gets the time when a GitHub action starts to download build images for the container.
        /// </summary>
        public string GitHubActionsBuildContainerStartTime => _environment.GetEnvironmentVariable(GitHubActionsBuildContainerStartTimeEnvVarName);

        /// <summary>
        /// Gets the time when a GitHub action finishes downloading build images and successfully built the container.
        /// </summary>
        public string GitHubActionsBuildContainerEndTime => _environment.GetEnvironmentVariable(GitHubActionsBuildContainerEndTimeEnvVarName);

        /// <summary>
        /// Gets a value indicating whether multi-platform builds must be disabled.
        /// They are disabled by default, so the user must opt-in setting environment
        /// variable <c>ENABLE_MULTIPLATFORM_BUILD</c> to <c>true</c>.
        /// </summary>
        public bool DisableMultiPlatformBuild => !IsEnvVariableTrue(EnableMultiPlatformBuildEnvVarName);

        private bool IsEnvVariableTrue(string disableEnvVarName)
        {
            var isDisabledVar = _environment.GetBoolEnvironmentVariable(disableEnvVarName);
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