// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class DotNetCoreScriptGeneratorOptionsSetup : IConfigureOptions<DotNetCoreScriptGeneratorOptions>
    {
        private readonly IEnvironment _environment;

        public DotNetCoreScriptGeneratorOptionsSetup(IEnvironment environment)
        {
            _environment = environment;
        }

        public void Configure(DotNetCoreScriptGeneratorOptions options)
        {
            options.DotNetVersion = _environment.GetEnvironmentVariable(
                EnvironmentSettingsKeys.DotNetVersion);
            options.Project = _environment.GetEnvironmentVariable(EnvironmentSettingsKeys.Project);
            options.MSBuildConfiguration = _environment.GetEnvironmentVariable(
                EnvironmentSettingsKeys.MSBuildConfiguration);
        }
    }
}