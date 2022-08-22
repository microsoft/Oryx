// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Golang;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    /// <summary>
    /// Gets hierarchical configuration from IConfiguration api and
    /// binds the properties on <see cref="GolangScriptGeneratorOptions"/>.
    /// </summary>
    public class GolangScriptGeneratorOptionsSetup : OptionsSetupBase, IConfigureOptions<GolangScriptGeneratorOptions>
    {
        public GolangScriptGeneratorOptionsSetup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public void Configure(GolangScriptGeneratorOptions options)
        {
            options.GolangVersion = this.GetStringValue(SettingsKeys.GolangVersion);
            options.DefaultVersion = this.GetStringValue(SettingsKeys.GolangDefaultVersion);
        }
    }
}