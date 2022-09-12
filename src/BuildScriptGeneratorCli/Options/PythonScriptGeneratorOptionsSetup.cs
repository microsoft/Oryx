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
            options.PythonVersion = this.GetStringValue(SettingsKeys.PythonVersion);
            options.DefaultVersion = this.GetStringValue(SettingsKeys.PythonDefaultVersion);
            options.EnableCollectStatic = !this.GetBooleanValue(SettingsKeys.DisableCollectStatic);
            options.VirtualEnvironmentName = this.GetStringValue(SettingsKeys.PythonVirtualEnvironmentName);
            options.CustomRequirementsTxtPath = this.GetStringValue(SettingsKeys.CustomRequirementsTxtPath);
        }
    }
}
