// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Detector;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    public class DetectorOptionsSetup : OptionsSetupBase, IConfigureOptions<DetectorOptions>
    {
        public DetectorOptionsSetup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public void Configure(DetectorOptions options)
        {
            options.Project = GetStringValue(SettingsKeys.Project);
            options.AppType = GetStringValue(SettingsKeys.AppType);
            options.DisableRecursiveLookUp = GetBooleanValue(SettingsKeys.DisableRecursiveLookUp);
            options.CustomRequirementsTxtPath = GetStringValue(SettingsKeys.CustomRequirementsTxtPath);
        }
    }
}
