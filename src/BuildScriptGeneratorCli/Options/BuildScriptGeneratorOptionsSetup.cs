// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using BuildScriptGeneratorLib = Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    public class BuildScriptGeneratorOptionsSetup
        : OptionsSetupBase, IConfigureOptions<BuildScriptGeneratorLib.BuildScriptGeneratorOptions>
    {
        public BuildScriptGeneratorOptionsSetup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public void Configure(BuildScriptGeneratorLib.BuildScriptGeneratorOptions options)
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
            options.EnableDotNetCoreBuild = !GetBooleanValue(SettingsKeys.DisableDotNetCoreBuild);
            options.EnableNodeJSBuild = !GetBooleanValue(SettingsKeys.DisableNodeJSBuild);
            options.EnablePythonBuild = !GetBooleanValue(SettingsKeys.DisablePythonBuild);
            options.EnablePhpBuild = !GetBooleanValue(SettingsKeys.DisablePhpBuild);
            options.EnableHugoBuild = !GetBooleanValue(SettingsKeys.DisableHugoBuild);
            options.EnableRubyBuild = !GetBooleanValue(SettingsKeys.DisableRubyBuild);
            options.EnableJavaBuild = !GetBooleanValue(SettingsKeys.DisableJavaBuild);
            options.EnableMultiPlatformBuild = GetBooleanValue(SettingsKeys.EnableMultiPlatformBuild);
            options.EnableTelemetry = !GetBooleanValue(SettingsKeys.DisableTelemetry);
            options.PreBuildScriptPath = GetStringValue(SettingsKeys.PreBuildScriptPath);
            options.PreBuildCommand = GetStringValue(SettingsKeys.PreBuildCommand);
            options.PostBuildScriptPath = GetStringValue(SettingsKeys.PostBuildScriptPath);
            options.PostBuildCommand = GetStringValue(SettingsKeys.PostBuildCommand);
            options.OryxSdkStorageBaseUrl = GetStringValue(SettingsKeys.OryxSdkStorageBaseUrl);
            options.AppType = GetStringValue(SettingsKeys.AppType);
            options.BuildCommandsFileName = GetStringValue(SettingsKeys.BuildCommandsFileName);
            options.CompressDestinationDir = GetBooleanValue(SettingsKeys.CompressDestinationDir);

            // Dynamic install
            options.EnableDynamicInstall = GetBooleanValue(SettingsKeys.EnableDynamicInstall);

            var dynamicInstallRootDir = GetStringValue(SettingsKeys.DynamicInstallRootDir);

            if (string.IsNullOrEmpty(dynamicInstallRootDir))
            {
                // If no explicit value was provided for the directory, we fall back to the safest option
                // (in terms of permissions)
                options.DynamicInstallRootDir = BuildScriptGeneratorLib.Constants.TemporaryInstallationDirectoryRoot;
            }
            else
            {
                dynamicInstallRootDir = dynamicInstallRootDir.Trim().TrimEnd('/');
                dynamicInstallRootDir = Path.GetFullPath(dynamicInstallRootDir);
                options.DynamicInstallRootDir = dynamicInstallRootDir;
            }
        }
    }
}
