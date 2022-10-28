// This file was auto-generated from 'constants.yaml'. Changes may be overridden.

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.Common
{
    public static class StagingRuntimeConstants
    {
        public const string DotnetcorePrivateDownloadUrlFormat = "https://dotnetcli.blob.core.windows.net/dotnet-private/internal/#DOTNETVERSION#/dotnet-runtime-#DOTNETVERSION#-linux-x64.tar.gz$DOTNET_PRIVATE_STORAGE_ACCOUNT_ACCESS_TOKEN";
        public const string AspnetcorePrivateDownloadUrlFormat = "https://dotnetcli.blob.core.windows.net/dotnet-private/internal/#ASPNETVERSION#/aspnetcore-runtime-#ASPNETVERSION#-linux-x64.tar.gz$DOTNET_PRIVATE_STORAGE_ACCOUNT_ACCESS_TOKEN";
        public static readonly List<string> DotnetcoreStagingRuntimeVersions = new List<string> { "7.0" };
    }
}