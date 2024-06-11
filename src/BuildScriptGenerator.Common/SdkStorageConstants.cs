// This file was auto-generated from 'constants.yaml'. Changes may be overridden.

namespace Microsoft.Oryx.BuildScriptGenerator.Common
{
    public static class SdkStorageConstants
    {
        public const string EnableDynamicInstallKey = "ENABLE_DYNAMIC_INSTALL";
        public const string SdkStorageBaseUrlKeyName = "ORYX_SDK_STORAGE_BASE_URL";
        public const string ProdSdkStorageBaseUrl = "https://oryxsdksprod.blob.core.windows.net";
        public const string ProdBackupSdkStorageBaseUrl = "https://oryxsdksprodbackup.blob.core.windows.net";
        public const string ProdSdkCdnStorageBaseUrl = "https://oryx-cdn.microsoft.io";
        public const string DefaultVersionFileName = "defaultVersion.txt";
        public const string DefaultVersionFilePrefix = "defaultVersion";
        public const string DefaultVersionFileType = "txt";
        public const string VersionsToBuildFileName = "versionsToBuild.txt";
        public const string ContainerMetadataUrlFormat = "{0}/{1}?restype=container&comp=list&include=metadata&marker={2}&{3}";
        public const string SdkDownloadSentinelFileName = ".oryx-sdkdownload-sentinel";
        public const string SdkVersionMetadataName = "Sdk_version";
        public const string LegacySdkVersionMetadataName = "Version";
        public const string DotnetRuntimeVersionMetadataName = "Dotnet_runtime_version";
        public const string LegacyDotnetRuntimeVersionMetadataName = "Runtime_version";
        public const string OsTypeMetadataName = "Os_type";
    }
}