// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    public static class SettingsKeys
    {
        public const string PlatformName = "PLATFORM_NAME";
        public const string PlatformVersion = "PLATFORM_VERSION";
        public const string CreatePackage = "CREATE_PACKAGE";
        public const string EnableDynamicInstall = "ENABLE_DYNAMIC_INSTALL";
        public const string DisableCheckers = "DISABLE_CHECKERS";
        public const string DisableDotNetCoreBuild = "DISABLE_DOTNETCORE_BUILD";
        public const string DisableNodeJSBuild = "DISABLE_NODEJS_BUILD";
        public const string DisablePythonBuild = "DISABLE_PYTHON_BUILD";
        public const string DisablePhpBuild = "DISABLE_PHP_BUILD";
        public const string EnableMultiPlatformBuild = "ENABLE_MULTIPLATFORM_BUILD";
        public const string DisableTelemetry = "ORYX_DISABLE_TELEMETRY";
        public const string PreBuildScriptPath = "PRE_BUILD_SCRIPT_PATH";
        public const string PreBuildCommand = "PRE_BUILD_COMMAND";
        public const string PostBuildScriptPath = "POST_BUILD_SCRIPT_PATH";
        public const string PostBuildCommand = "POST_BUILD_COMMAND";
        public const string DotNetVersion = "DOTNET_VERSION";
        public const string NodeVersion = "NODE_VERSION";
        public const string CustomRunBuildCommand = "RUN_BUILD_COMMAND";
        public const string PythonVersion = "PYTHON_VERSION";
        public const string PhpVersion = "PHP_VERSION";
        public const string HugoVersion = "HUGO_VERSION";
        public const string Project = "PROJECT";
        public const string MSBuildConfiguration = "MSBUILD_CONFIGURATION";
        public const string DisableCollectStatic = "DISABLE_COLLECTSTATIC";
        public const string RequiredOsPackages = "REQUIRED_OS_PACKAGES";
        public const string PruneDevDependencies = "PRUNE_DEV_DEPENDENCIES";
        public const string NpmRegistryUrl = "NPM_REGISTRY_URL";
        public const string PythonVirtualEnvironmentName = "VIRTUALENV_NAME";
        public const string OryxSdkStorageBaseUrl = "ORYX_SDK_STORAGE_BASE_URL";
    }
}
