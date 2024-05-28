// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Java;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    /// <summary>
    /// Gets hierarchical configuration from IConfiguration api and
    /// binds the properties on <see cref="ScriptGeneratorOptionsForJava"/>.
    /// </summary>
    public class ScriptGeneratorOptionsSetupForJava : OptionsSetupBase, IConfigureOptions<ScriptGeneratorOptionsForJava>
    {
        public ScriptGeneratorOptionsSetupForJava(IConfiguration configuration)
            : base(configuration)
        {
        }

        public void Configure(ScriptGeneratorOptionsForJava options)
        {
            options.JavaVersion = this.GetStringValue(SettingsKeys.JavaVersion);
            options.MavenVersion = this.GetStringValue(SettingsKeys.MavenVersion);
            options.JavaDefaultVersion = this.GetStringValue(SettingsKeys.JavaDefaultVersion);
            options.MavenDefaultVersion = this.GetStringValue(SettingsKeys.MavenDefaultVersion);
        }
    }
}