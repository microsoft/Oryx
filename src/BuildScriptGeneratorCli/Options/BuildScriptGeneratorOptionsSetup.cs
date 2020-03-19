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
    public class BuildScriptGeneratorOptionsSetup : IConfigureOptions<BuildScriptGeneratorOptions>
    {
        private readonly IConfiguration _config;

        public BuildScriptGeneratorOptionsSetup(IConfiguration configuration)
        {
            _config = configuration;
        }

        public void Configure(BuildScriptGeneratorOptions options)
        {
            // "config.GetValue" call will get the most closest value provided based on the order of
            // configuration sources added to the ConfigurationBuilder above.
            options.PlatformName = _config.GetValue<string>(SettingsKeys.PlatformName);
            options.PlatformVersion = _config.GetValue<string>(SettingsKeys.PlatformVersion);
            options.ShouldPackage = _config.GetValue<bool>(SettingsKeys.CreatePackage);
            var requiredOsPackages = _config.GetValue<string>(SettingsKeys.RequiredOsPackages);
            options.RequiredOsPackages = string.IsNullOrWhiteSpace(requiredOsPackages)
                ? null : requiredOsPackages.Split(',').Select(pkg => pkg.Trim()).ToArray();

            options.EnableDynamicInstall = _config.GetValue<bool>(SettingsKeys.EnableDynamicInstall);
            options.DisableDotNetCoreBuild = _config.GetValue<bool>(SettingsKeys.DisableDotNetCoreBuild);
            options.DisableNodeJSBuild = _config.GetValue<bool>(SettingsKeys.DisableNodeJSBuild);
            options.DisablePythonBuild = _config.GetValue<bool>(SettingsKeys.DisablePythonBuild);
            options.DisablePhpBuild = _config.GetValue<bool>(SettingsKeys.DisablePhpBuild);
            options.EnableMultiPlatformBuild = _config.GetValue<bool>(SettingsKeys.EnableMultiPlatformBuild);
            options.DisableTelemetry = _config.GetValue<bool>(SettingsKeys.DisableTelemetry);
            options.PreBuildScriptPath = _config.GetValue<string>(SettingsKeys.PreBuildScriptPath);
            options.PreBuildCommand = _config.GetValue<string>(SettingsKeys.PreBuildCommand);
            options.PostBuildScriptPath = _config.GetValue<string>(SettingsKeys.PostBuildScriptPath);
            options.PostBuildCommand = _config.GetValue<string>(SettingsKeys.PostBuildCommand);
        }
    }
}
