// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    /// <summary>
    /// Gets hierarchical configuration from IConfiguration api and
    /// binds the properties on <see cref="DotNetCoreScriptGeneratorOptions"/>.
    /// </summary>
    public class DotNetCoreScriptGeneratorOptionsSetup
        : OptionsSetupBase, IConfigureOptions<DotNetCoreScriptGeneratorOptions>
    {
        public DotNetCoreScriptGeneratorOptionsSetup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public void Configure(DotNetCoreScriptGeneratorOptions options)
        {
            options.MSBuildConfiguration = this.GetStringValue(SettingsKeys.MSBuildConfiguration);
            options.DotNetCoreRuntimeVersion = this.GetStringValue(SettingsKeys.DotNetVersion);
            options.DefaultRuntimeVersion = this.GetStringValue(SettingsKeys.DotNetDefaultVersion);
        }
    }
}