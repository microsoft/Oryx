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
        private const string DisableDotNetCoreEnvVarName = "DISABLE_DOTNETCORE_BUILD";
        private const string DisablePythonEnvVarName = "DISABLE_PYTHON_BUILD";
        private const string DisableNodeJsEnvVarName = "DISABLE_NODEJS_BUILD";
        private const string DisableMultiPlatformBuildEnvVarName = "DISABLE_MULTIPLATFORM_BUILD";

        private IEnvironment _environment;

        public CliEnvironmentSettings(IEnvironment environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public bool DisableDotNetCore => IsDisableVariableSet(DisableDotNetCoreEnvVarName);

        public bool DisableNodeJs => IsDisableVariableSet(DisableNodeJsEnvVarName);

        public bool DisablePython => IsDisableVariableSet(DisablePythonEnvVarName);

        public bool DisableMultiPlatformBuild => IsDisableVariableSet(DisableMultiPlatformBuildEnvVarName);

        private bool IsDisableVariableSet(string disableEnvVarName)
        {
            var isDisabledVar = _environment.GetBoolEnvironmentVariable(disableEnvVarName);
            if (isDisabledVar == true)
            {
                // The user has set the variable _and_ its value is true, so the feature is disabled.
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}