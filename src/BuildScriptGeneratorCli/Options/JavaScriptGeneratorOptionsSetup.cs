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
    /// binds the properties on <see cref="JavaScriptGeneratorOptions"/>.
    /// </summary>
    public class JavaScriptGeneratorOptionsSetup : OptionsSetupBase, IConfigureOptions<JavaScriptGeneratorOptions>
    {
        public JavaScriptGeneratorOptionsSetup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public void Configure(JavaScriptGeneratorOptions options)
        {
            options.JavaVersion = GetStringValue(SettingsKeys.JavaVersion);
            options.MavenVersion = GetStringValue(SettingsKeys.MavenVersion);
        }
    }
}