// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Php;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    public class PhpScriptGeneratorOptionsSetup : IConfigureOptions<PhpScriptGeneratorOptions>
    {
        private readonly IConfiguration _config;

        public PhpScriptGeneratorOptionsSetup(IConfiguration configuration)
        {
            _config = configuration;
        }

        public void Configure(PhpScriptGeneratorOptions options)
        {
            options.PhpVersion = _config.GetValue<string>(SettingsKeys.PhpVersion);
        }
    }
}
