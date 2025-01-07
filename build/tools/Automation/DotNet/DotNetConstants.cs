// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.Automation.DotNet
{
    public class DotNetConstants
    {
        public const string DotNetBlockedVersionsEnvVar = "DOTNET_BLOCKED_VERSIONS_ARRAY";
        public const string DotNetMinReleaseVersionEnvVar = "DOTNET_MIN_RELEASE_VERSION";
        public const string DotNetMaxReleaseVersionEnvVar = "DOTNET_MAX_RELEASE_VERSION";
        public const string DotNetMaxVersionEnvVar = "DOTNET_MAX_VERSION";
        public const string DotNetName = "dotnet";
        public const string DotNetCoreName = "net-core";
        public const string DotNetAspCoreName = "aspnet-core";
        public const string DotNetSdkKey = "dot-net-core-sdk-versions";
        public const string DotNetRuntimeKey = "dot-net-core-run-time-versions";
        public const string DotNetLinuxTarFileRegex = ".*-linux-x64.tar.gz";
        public const string SdkName = "sdk";
<<<<<<< Updated upstream
        public const string ReleasesIndexJsonUrl = "https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/releases-index.json";
=======
        public const string ReleasesIndexJsonUrl = "https://builds.dotnet.microsoft.com/dotnet/release-metadata/releases-index.json";
>>>>>>> Stashed changes
        public const string DotNetSuffixUrl = "/dotnet?restype=container&comp=list&include=metadata";
    }
}
