// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class CliEnvironmentSettings
    {
        private const string DisableDotNetCoreEnvVarName = "DISABLE_DOTNETCORE_BUILD";
        private const string DisablePythonEnvVarName = "DISABLE_PYTHON_BUILD";
        private const string DisableNodeJsEnvVarName = "DISABLE_NODEJS_BUILD";

        private IEnvironment _environment;

        public CliEnvironmentSettings(IEnvironment environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public bool DisableDotNetCore => IsLanguageDisabled(DisableDotNetCoreEnvVarName);

        public bool DisableNodeJs => IsLanguageDisabled(DisableNodeJsEnvVarName);

        public bool DisablePython => IsLanguageDisabled(DisablePythonEnvVarName);

        private bool IsLanguageDisabled(string disableLanguageEnvVarName)
        {
            var isLangDisabledVar = _environment.GetBoolEnvironmentVariable(disableLanguageEnvVarName);
            if (isLangDisabledVar == true)
            {
                // The user has set the variable _and_ its value is true, so the language is disabled.
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}