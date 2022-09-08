// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Ruby;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    /// <summary>
    /// Gets hierarchical configuration from IConfiguration api and
    /// binds the properties on <see cref="RubyScriptGeneratorOptions"/>.
    /// </summary>
    public class RubyScriptGeneratorOptionsSetup : OptionsSetupBase, IConfigureOptions<RubyScriptGeneratorOptions>
    {
        public RubyScriptGeneratorOptionsSetup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public void Configure(RubyScriptGeneratorOptions options)
        {
            options.RubyVersion = this.GetStringValue(SettingsKeys.RubyVersion);
            options.DefaultVersion = this.GetStringValue(SettingsKeys.RubyDefaultVersion);
            options.CustomBuildCommand = this.GetStringValue(SettingsKeys.CustomBuildCommand);
        }
    }
}