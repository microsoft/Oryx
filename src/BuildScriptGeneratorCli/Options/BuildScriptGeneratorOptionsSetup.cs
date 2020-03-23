// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    public class BuildScriptGeneratorOptionsSetup : OptionsSetupBase, IConfigureOptions<BuildScriptGeneratorOptions>
    {
        public BuildScriptGeneratorOptionsSetup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public void Configure(BuildScriptGeneratorOptions options)
        {
            // "config.GetValue" call will get the most closest value provided based on the order of
            // configuration sources added to the ConfigurationBuilder above.
            options.PlatformName = GetStringValue(SettingsKeys.PlatformName);
            options.PlatformVersion = GetStringValue(SettingsKeys.PlatformVersion);
            options.ShouldPackage = GetBooleanValue(SettingsKeys.CreatePackage);
            var requiredOsPackages = GetStringValue(SettingsKeys.RequiredOsPackages);
            options.RequiredOsPackages = string.IsNullOrWhiteSpace(requiredOsPackages)
                ? null : requiredOsPackages.Split(',').Select(pkg => pkg.Trim()).ToArray();

            options.EnableCheckers = !GetBooleanValue(SettingsKeys.DisableCheckers);
            options.EnableDynamicInstall = GetBooleanValue(SettingsKeys.EnableDynamicInstall);
            options.EnableDotNetCoreBuild = !GetBooleanValue(SettingsKeys.DisableDotNetCoreBuild);
            options.EnableNodeJSBuild = !GetBooleanValue(SettingsKeys.DisableNodeJSBuild);
            options.EnablePythonBuild = !GetBooleanValue(SettingsKeys.DisablePythonBuild);
            options.EnablePhpBuild = !GetBooleanValue(SettingsKeys.DisablePhpBuild);
            options.EnableMultiPlatformBuild = GetBooleanValue(SettingsKeys.EnableMultiPlatformBuild);
            options.EnableTelemetry = !GetBooleanValue(SettingsKeys.DisableTelemetry);
            options.PreBuildScriptPath = GetStringValue(SettingsKeys.PreBuildScriptPath);
            options.PreBuildCommand = GetStringValue(SettingsKeys.PreBuildCommand);
            options.PostBuildScriptPath = GetStringValue(SettingsKeys.PostBuildScriptPath);
            options.PostBuildCommand = GetStringValue(SettingsKeys.PostBuildCommand);
            options.OryxSdkStorageBaseUrl = GetStringValue(SettingsKeys.OryxSdkStorageBaseUrl);
        }
    }
}
