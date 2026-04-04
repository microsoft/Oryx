// This file was auto-generated from 'constants.yaml'. Changes may be overridden.

namespace Microsoft.Oryx.BuildScriptGenerator.Common
{
    public static class SdkStorageConstants
    {
        public const string EnableDynamicInstallKey = "ENABLE_DYNAMIC_INSTALL";
        public const string SdkStorageBaseUrlKeyName = "ORYX_SDK_STORAGE_BASE_URL";
        public const string SdkStorageBackupBaseUrlKeyName = "ORYX_SDK_STORAGE_BACKUP_BASE_URL";
        public const string ProdSdkStorageBaseUrl = "https://oryxsdksprod.blob.core.windows.net";
        public const string ProdBackupSdkStorageBaseUrl = "https://oryxsdksprodbackup.blob.core.windows.net";
        public const string ProdSdkCdnStorageBaseUrl = "https://oryx-cdn.microsoft.io";
        public const string DefaultVersionFileName = "defaultVersion.txt";
        public const string DefaultVersionFilePrefix = "defaultVersion";
        public const string DefaultVersionFileType = "txt";
        public const string VersionsToBuildFileName = "versionsToBuild.txt";
        public const string ContainerMetadataUrlFormat = "{0}/{1}?restype=container&comp=list&include=metadata&marker={2}";
        public const string SdkDownloadSentinelFileName = ".oryx-sdkdownload-sentinel";
        public const string SdkVersionMetadataName = "Sdk_version";
        public const string LegacySdkVersionMetadataName = "Version";
        public const string DotnetRuntimeVersionMetadataName = "Dotnet_runtime_version";
        public const string LegacyDotnetRuntimeVersionMetadataName = "Runtime_version";
        public const string OsTypeMetadataName = "Os_type";

        // OCI image based SDK distribution constants
        public const string EnableExternalAcrSdkProviderKey = "ORYX_ENABLE_EXTERNAL_ACR_SDK_PROVIDER";
        public const string EnableAcrSdkProviderKey = "ORYX_ENABLE_ACR_SDK_PROVIDER";
        public const string AcrSdkRegistryUrlKeyName = "ORYX_ACR_SDK_REGISTRY_URL";
        public const string DefaultAcrSdkRegistryUrl = "https://mcr.microsoft.com";
        public const string AcrSdkRepositoryPrefixKeyName = "ORYX_ACR_SDK_REPOSITORY_PREFIX";
        public const string DefaultAcrSdkRepositoryPrefix = "oryx";

        /// <summary>
        /// Maps a platform name to its OCI SDK image repository path.
        /// e.g. "nodejs" → "oryx/nodejs-sdk", "php" → "oryx/php-sdk".
        /// Final image ref: mcr.microsoft.com/oryx/nodejs-sdk:bookworm-20.20.2
        /// </summary>
        public static string GetSdkImageRepository(string platformName, string prefix = null)
        {
            prefix = string.IsNullOrEmpty(prefix) ? DefaultAcrSdkRepositoryPrefix : prefix;
            return $"{prefix}/{platformName}-sdk";
        }
    }
}