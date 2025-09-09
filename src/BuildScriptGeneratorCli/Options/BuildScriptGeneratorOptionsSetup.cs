// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
            options.BindPort = this.GetStringValue(SettingsKeys.BindPort);
            options.BindPort2 = this.GetStringValue(SettingsKeys.BindPort2);
            options.BuildImage = this.GetStringValue(SettingsKeys.BuildImage);
            options.PlatformName = this.GetStringValue(SettingsKeys.PlatformName);
            options.PlatformVersion = this.GetStringValue(SettingsKeys.PlatformVersion);
            options.RuntimePlatformName = this.GetStringValue(SettingsKeys.RuntimePlatformName);
            options.RuntimePlatformVersion = this.GetStringValue(SettingsKeys.RuntimePlatformVersion);
            options.ShouldPackage = this.GetBooleanValue(SettingsKeys.CreatePackage);
            var requiredOsPackages = this.GetStringValue(SettingsKeys.RequiredOsPackages);
            options.RequiredOsPackages = string.IsNullOrWhiteSpace(requiredOsPackages)
                ? null : requiredOsPackages.Split(',').Select(pkg => pkg.Trim()).ToArray();

            options.EnableCheckers = !this.GetBooleanValue(SettingsKeys.DisableCheckers);
            options.EnableDotNetCoreBuild = !this.GetBooleanValue(SettingsKeys.DisableDotNetCoreBuild);
            options.EnableGolangBuild = !this.GetBooleanValue(SettingsKeys.DisableGolangBuild);
            options.EnableNodeJSBuild = !this.GetBooleanValue(SettingsKeys.DisableNodeJSBuild);
            options.EnablePythonBuild = !this.GetBooleanValue(SettingsKeys.DisablePythonBuild);
            options.EnablePhpBuild = !this.GetBooleanValue(SettingsKeys.DisablePhpBuild);
            options.EnableHugoBuild = !this.GetBooleanValue(SettingsKeys.DisableHugoBuild);
            options.EnableRubyBuild = !this.GetBooleanValue(SettingsKeys.DisableRubyBuild);
            options.EnableJavaBuild = !this.GetBooleanValue(SettingsKeys.DisableJavaBuild);
            options.EnableMultiPlatformBuild = this.GetBooleanValue(SettingsKeys.EnableMultiPlatformBuild);
            options.EnableTelemetry = !this.GetBooleanValue(SettingsKeys.DisableTelemetry);
            options.PreBuildScriptPath = this.GetStringValue(SettingsKeys.PreBuildScriptPath);
            options.PreBuildCommand = this.GetStringValue(SettingsKeys.PreBuildCommand);
            options.PostBuildScriptPath = this.GetStringValue(SettingsKeys.PostBuildScriptPath);
            options.PostBuildCommand = this.GetStringValue(SettingsKeys.PostBuildCommand);
            options.OryxSdkStorageBaseUrl = this.GetStringValue(SettingsKeys.OryxSdkStorageBaseUrl);
            options.OryxSdkStorageBackupBaseUrl = this.GetStringValue(SettingsKeys.OryxSdkStorageBackupBaseUrl);
            options.AppType = this.GetStringValue(SettingsKeys.AppType);
            options.BuildCommandsFileName = this.GetStringValue(SettingsKeys.BuildCommandsFileName);
            options.CompressDestinationDir = this.GetBooleanValue(SettingsKeys.CompressDestinationDir);
            options.CustomRequirementsTxtPath = this.GetStringValue(SettingsKeys.CustomRequirementsTxtPath);
            options.CallerId = this.GetStringValue(SettingsKeys.CallerId);
            options.OryxDisablePipUpgrade = this.GetBooleanValue(SettingsKeys.OryxDisablePipUpgrade);

            // Dynamic install
            options.EnableDynamicInstall = this.GetBooleanValue(SettingsKeys.EnableDynamicInstall);
            options.EnableExternalSdkProvider = this.GetBooleanValue(SettingsKeys.EnableExternalSdkProvider);

            var dynamicInstallRootDir = this.GetStringValue(SettingsKeys.DynamicInstallRootDir);

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

            options.OsType = this.GetStringValue(SettingsKeys.OsType);
            options.OsFlavor = this.GetStringValue(SettingsKeys.OsFlavor);
            options.DebianFlavor = this.GetStringValue(SettingsKeys.DebianFlavor);
        }
    }
}
