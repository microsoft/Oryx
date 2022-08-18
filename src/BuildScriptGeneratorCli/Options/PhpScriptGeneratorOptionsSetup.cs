// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Php;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    /// <summary>
    /// Gets hierarchical configuration from IConfiguration api and
    /// binds the properties on <see cref="PhpScriptGeneratorOptions"/>.
    /// </summary>
    public class PhpScriptGeneratorOptionsSetup : OptionsSetupBase, IConfigureOptions<PhpScriptGeneratorOptions>
    {
        public PhpScriptGeneratorOptionsSetup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public void Configure(PhpScriptGeneratorOptions options)
        {
            options.PhpVersion = this.GetStringValue(SettingsKeys.PhpVersion);
            options.PhpComposerVersion = this.GetStringValue(SettingsKeys.PhpComposerVersion);
            options.PhpDefaultVersion = this.GetStringValue(SettingsKeys.PhpDefaultVersion);
            options.PhpComposerDefaultVersion = this.GetStringValue(SettingsKeys.PhpComposerDefaultVersion);
        }
    }
}