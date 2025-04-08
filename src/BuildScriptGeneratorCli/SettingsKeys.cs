// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    public static class SettingsKeys
    {
        public const string BindPort = "BIND_PORT";
        public const string BindPort2 = "BIND_PORT2";
        public const string BuildImage = "BUILD_IMAGE";
        public const string PlatformName = "PLATFORM_NAME";
        public const string PlatformVersion = "PLATFORM_VERSION";
        public const string RuntimePlatformName = "RUNTIME_PLATFORM_NAME";
        public const string RuntimePlatformVersion = "RUNTIME_PLATFORM_VERSION";
        public const string CreatePackage = "CREATE_PACKAGE";
        public const string CompressDestinationDir = "COMPRESS_DESTINATION_DIR";
        public const string EnableDynamicInstall = "ENABLE_DYNAMIC_INSTALL";
        public const string EnableExternalSdkProvider = "ORYX_ENABLE_EXTERNAL_SDK_PROVIDER";
        public const string DisableCheckers = "DISABLE_CHECKERS";
        public const string DisableDotNetCoreBuild = "DISABLE_DOTNETCORE_BUILD";
        public const string DisableGolangBuild = "DISABLE_GOLANG_BUILD";
        public const string DisableNodeJSBuild = "DISABLE_NODEJS_BUILD";
        public const string DisablePythonBuild = "DISABLE_PYTHON_BUILD";
        public const string DisablePhpBuild = "DISABLE_PHP_BUILD";
        public const string DisableHugoBuild = "DISABLE_HUGO_BUILD";
        public const string DisableRubyBuild = "DISABLE_RUBY_BUILD";
        public const string DisableJavaBuild = "DISABLE_JAVA_BUILD";
        public const string EnableMultiPlatformBuild = "ENABLE_MULTIPLATFORM_BUILD";
        public const string DisableTelemetry = "ORYX_DISABLE_TELEMETRY";
        public const string PreBuildScriptPath = "PRE_BUILD_SCRIPT_PATH";
        public const string PreBuildCommand = "PRE_BUILD_COMMAND";
        public const string PostBuildScriptPath = "POST_BUILD_SCRIPT_PATH";
        public const string PostBuildCommand = "POST_BUILD_COMMAND";
        public const string DotNetVersion = "DOTNET_VERSION";
        public const string DotNetDefaultVersion = "DOTNET_DEFAULT_VERSION";
        public const string NodeVersion = "NODE_VERSION";
        public const string NodeDefaultVersion = "NODE_DEFAULT_VERSION";
        public const string CustomRunBuildCommand = "RUN_BUILD_COMMAND";
        public const string CustomBuildCommand = "CUSTOM_BUILD_COMMAND";
        public const string PythonVersion = "PYTHON_VERSION";
        public const string PythonDefaultVersion = "PYTHON_DEFAULT_VERSION";
        public const string GolangVersion = "GOLANG_VERSION";
        public const string GolangDefaultVersion = "GOLANG_DEFAULT_VERSION";
        public const string PhpVersion = "PHP_VERSION";
        public const string PhpDefaultVersion = "PHP_DEFAULT_VERSION";
        public const string PhpComposerVersion = "PHP_COMPOSER_VERSION";
        public const string PhpComposerDefaultVersion = "PHP_COMPOSER_DEFAULT_VERSION";
        public const string HugoVersion = "HUGO_VERSION";
        public const string HugoDefaultVersion = "HUGO_DEFAULT_VERSION";
        public const string RubyVersion = "RUBY_VERSION";
        public const string RubyDefaultVersion = "RUBY_DEFAULT_VERSION";
        public const string JavaVersion = "JAVA_VERSION";
        public const string JavaDefaultVersion = "JAVA_DEFAULT_VERSION";
        public const string MavenVersion = "MAVEN_VERSION";
        public const string MavenDefaultVersion = "MAVEN_DEFAULT_VERSION";
        public const string Project = "PROJECT";
        public const string MSBuildConfiguration = "MSBUILD_CONFIGURATION";
        public const string DisableCollectStatic = "DISABLE_COLLECTSTATIC";
        public const string RequiredOsPackages = "REQUIRED_OS_PACKAGES";
        public const string PruneDevDependencies = "PRUNE_DEV_DEPENDENCIES";
        public const string NpmRegistryUrl = "NPM_REGISTRY_URL";
        public const string EnableNodeMonorepoBuild = "ENABLE_NODE_MONOREPO_BUILD";
        public const string YarnTimeOutConfig = "YARN_TIMEOUT_CONFIG";
        public const string PythonVirtualEnvironmentName = "VIRTUALENV_NAME";
        public const string OryxSdkStorageBaseUrl = "ORYX_SDK_STORAGE_BASE_URL";
        public const string OryxSdkStorageBackupBaseUrl = "ORYX_SDK_STORAGE_BACKUP_BASE_URL";
        public const string AppType = "ORYX_APP_TYPE";
        public const string BuildCommandsFileName = "BUILDCOMMANDS_FILE";
        public const string DynamicInstallRootDir = "DYNAMIC_INSTALL_ROOT_DIR";
        public const string DisableRecursiveLookUp = "DISABLE_RECURSIVE_LOOKUP";
        public const string CustomRequirementsTxtPath = "CUSTOM_REQUIREMENTSTXT_PATH";
        public const string DebianFlavor = "DEBIAN_FLAVOR";
        public const string CallerId = "CALLER_ID";
        public const string OryxDisablePipUpgrade = "ORYX_DISABLE_PIP_UPGRADE";
    }
}
