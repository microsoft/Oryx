// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class BuildScriptGeneratorOptions
    {
        public string SourceDir { get; set; }

        public string IntermediateDir { get; set; }

        public string DestinationDir { get; set; }

        public string BindPort { get; set; }

        public string BindPort2 { get; set; }

        public string BuildImage { get; set; }

        public string PlatformName { get; set; }

        public string PlatformVersion { get; set; }

        public string RuntimePlatformName { get; set; }

        public string RuntimePlatformVersion { get; set; }

        public bool ScriptOnly { get; set; }

        public bool ShouldPackage { get; set; }

        public string[] RequiredOsPackages { get; set; }

        public IDictionary<string, string> Properties { get; set; }

        public string ManifestDir { get; set; }

        public bool EnableDynamicInstall { get; set; }

        public bool EnableExternalSdkProvider { get; set; }

        public string DynamicInstallRootDir { get; set; }

        public bool EnableCheckers { get; set; }

        public bool EnableDotNetCoreBuild { get; set; }

        public bool EnableNodeJSBuild { get; set; }

        public bool EnableGolangBuild { get; set; }

        public bool EnablePythonBuild { get; set; }

        public bool EnablePhpBuild { get; set; }

        public bool EnableHugoBuild { get; set; }

        public bool EnableRubyBuild { get; set; }

        public bool EnableJavaBuild { get; set; }

        public bool EnableMultiPlatformBuild { get; set; }

        public string OryxSdkStorageBaseUrl { get; set; }

        public string OryxSdkStorageBackupBaseUrl { get; set; }

        public bool EnableTelemetry { get; set; }

        public string PreBuildScriptPath { get; set; }

        public string PreBuildCommand { get; set; }

        public string PostBuildScriptPath { get; set; }

        public string PostBuildCommand { get; set; }

        public string AppType { get; set; }

        public string BuildCommandsFileName { get; set; }

        public bool CompressDestinationDir { get; set; }

        public string CustomRequirementsTxtPath { get; set; }

        public string DebianFlavor { get; set; }

        public string CallerId { get; set; }

        public string ImageType { get; set; }

        public bool OryxDisablePipUpgrade { get; set; }
    }
}