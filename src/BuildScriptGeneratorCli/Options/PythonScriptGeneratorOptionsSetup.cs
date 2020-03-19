// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Python;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    public class PythonScriptGeneratorOptionsSetup : IConfigureOptions<PythonScriptGeneratorOptions>
    {
        private readonly IConfiguration _config;

        public PythonScriptGeneratorOptionsSetup(IConfiguration configuration)
        {
            _config = configuration;
        }

        public void Configure(PythonScriptGeneratorOptions options)
        {
            options.PythonVersion = _config.GetValue<string>(SettingsKeys.PythonVersion);
            options.DisableCollectStatic = _config.GetValue<bool>(SettingsKeys.DisableCollectStatic);
        }
    }
}
