// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    public class DotNetCoreScriptGeneratorOptionsSetup : IConfigureOptions<DotNetCoreScriptGeneratorOptions>
    {
        private readonly IConfiguration _config;

        public DotNetCoreScriptGeneratorOptionsSetup(IConfiguration configuration)
        {
            _config = configuration;
        }

        public void Configure(DotNetCoreScriptGeneratorOptions options)
        {
            options.DotNetVersion = _config.GetValue<string>(SettingsKeys.DotNetVersion);
            options.Project = _config.GetValue<string>(SettingsKeys.Project);
            options.MSBuildConfiguration = _config.GetValue<string>(SettingsKeys.MSBuildConfiguration);
        }
    }
}
