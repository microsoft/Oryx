// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Hugo;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    /// <summary>
    /// Gets hierarchical configuration from IConfiguration api and
    /// binds the properties on <see cref="HugoScriptGeneratorOptions"/>.
    /// </summary>
    public class HugoScriptGeneratorOptionsSetup : OptionsSetupBase, IConfigureOptions<HugoScriptGeneratorOptions>
    {
        public HugoScriptGeneratorOptionsSetup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public void Configure(HugoScriptGeneratorOptions options)
        {
            options.HugoVersion = this.GetStringValue(SettingsKeys.HugoVersion);
            options.DefaultVersion = this.GetStringValue(SettingsKeys.HugoDefaultVersion);
        }
    }
}