// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Automation
{
    internal static class Constants
    {
        public const string ConstantsYaml = "constants.yaml";
        public const string SdkName = "sdk";
        public const string VersionsToBuildTxt = "versionsToBuild.txt";

        public const string DotNetName = "dotnet";
        public const string DotNetCoreName = "net-core";
        public const string DotNetAspCoreName = "aspnet-core";
        public const string DotNetSdkKey = "dot-net-core-sdk-versions";
        public const string DotNetRuntimeKey = "dot-net-core-run-time-versions";
        public const string DotNetReleasesMetaDataUrl = "https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/releases-index.json";
        public const string DotNetLinuxTarFileRegex = ".*-linux-x64.tar.gz";

        // TODO: add constants for other platforms
    }
}