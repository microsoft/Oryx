// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public static class Constants
    {
        public const string PreBuildCommandPrologue = "Executing pre-build command...";
        public const string PreBuildCommandEpilogue = "Finished executing pre-build command.";
        public const string PostBuildCommandPrologue = "Executing post-build command...";
        public const string PostBuildCommandEpilogue = "Finished executing post-build command.";

        public const string OryxEnvironmentSettingNamePrefix = "ORYX_";
        public const string AppInsightsKey = "APPINSIGHTS_INSTRUMENTATIONKEY";

        public const string OryxGitHubUrl = "https://github.com/microsoft/Oryx";

        public const string True = "true";
        public const string False = "false";

        public const string TemporaryInstallationDirectoryRoot = "/tmp/oryx/platforms";
        public const string AppType = "apptype";
        public const string BuildCommandsFileName = "buildcommands-file";
        public const string FunctionApplications = "functions";
        public const string StaticSiteApplications = "static-sites";

        /// <summary>
        /// The name of the key used by benv script to identify the dynamic install root directory so that it can set
        /// the path to the installed sdks.
        /// </summary>
        public const string BenvDynamicInstallRootDirKey = "dynamic_install_root_dir";

        public const string BuildConfigurationFileHelp = "https://aka.ms/troubleshoot-buildconfig";

        public const string NetworkConfigurationHelpText = "Please ensure that your " +
            "network configuration allows traffic to required Oryx dependencies, as documented in " +
            "'https://github.com/microsoft/Oryx/blob/main/doc/hosts/appservice.md#network-dependencies'";
    }
}
