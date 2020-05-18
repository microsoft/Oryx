// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;

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
            // "config.GetValue" call will get the most closest value provided based on the order of
            // configuration sources added to the ConfigurationBuilder above.
            options.PlatformName = GetStringValue(SettingsKeys.PlatformName);
            options.PlatformVersion = GetStringValue(SettingsKeys.PlatformVersion);
            options.EnableCheckers = !GetBooleanValue(SettingsKeys.DisableCheckers);
            options.EnableDynamicInstall = GetBooleanValue(SettingsKeys.EnableDynamicInstall);
            options.EnableTelemetry = !GetBooleanValue(SettingsKeys.DisableTelemetry);
            options.OryxSdkStorageBaseUrl = GetStringValue(SettingsKeys.OryxSdkStorageBaseUrl);
        }
    }
}
