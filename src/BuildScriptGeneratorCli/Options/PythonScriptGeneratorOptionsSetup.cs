// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Python;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    public class PythonScriptGeneratorOptionsSetup : OptionsSetupBase, IConfigureOptions<PythonScriptGeneratorOptions>
    {
        public PythonScriptGeneratorOptionsSetup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public void Configure(PythonScriptGeneratorOptions options)
        {
            options.PythonVersion = GetStringValue(SettingsKeys.PythonVersion);
            options.EnableCollectStatic = !GetBooleanValue(SettingsKeys.DisableCollectStatic);
            options.VirtualEnvironmentName = GetStringValue(SettingsKeys.PythonVirtualEnvironmentName);
        }
    }
}
