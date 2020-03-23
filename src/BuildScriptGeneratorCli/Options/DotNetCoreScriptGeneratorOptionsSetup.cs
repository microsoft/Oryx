// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    /// <summary>
    /// Gets hierarchical configuration from IConfiguration api and
    /// binds the properties on <see cref="DotNetCoreScriptGeneratorOptions"/>.
    /// </summary>
    public class DotNetCoreScriptGeneratorOptionsSetup : IConfigureOptions<DotNetCoreScriptGeneratorOptions>
    {
        private readonly IEnvironment _environment;

        public DotNetCoreScriptGeneratorOptionsSetup(IEnvironment environment)
        {
            _environment = environment;
        }

        public void Configure(DotNetCoreScriptGeneratorOptions options)
        {
            options.DotNetVersion = _environment.GetEnvironmentVariable(SettingsKeys.DotNetVersion);
            options.Project = _environment.GetEnvironmentVariable(SettingsKeys.Project);
            options.MSBuildConfiguration = _environment.GetEnvironmentVariable(SettingsKeys.MSBuildConfiguration);
        }
    }
}