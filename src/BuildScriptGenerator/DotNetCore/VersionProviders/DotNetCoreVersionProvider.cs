// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class DotNetCoreVersionProvider : IDotNetCoreVersionProvider
    {
        private readonly BuildScriptGeneratorOptions cliOptions;
        private readonly DotNetCoreOnDiskVersionProvider onDiskVersionProvider;
        private readonly DotNetCoreSdkStorageVersionProvider sdkStorageVersionProvider;
        private readonly DotNetCoreExternalVersionProvider externalVersionProvider;
        private readonly DotNetCoreAcrVersionProvider acrVersionProvider;
        private readonly ILogger<DotNetCoreVersionProvider> logger;
        private readonly IStandardOutputWriter outputWriter;
        private string defaultRuntimeVersion;
        private Dictionary<string, string> supportedVersions;

        public DotNetCoreVersionProvider(
            IOptions<BuildScriptGeneratorOptions> cliOptions,
            DotNetCoreOnDiskVersionProvider onDiskVersionProvider,
            DotNetCoreSdkStorageVersionProvider sdkStorageVersionProvider,
            DotNetCoreExternalVersionProvider externalVersionProvider,
            DotNetCoreAcrVersionProvider acrVersionProvider,
            ILogger<DotNetCoreVersionProvider> logger,
            IStandardOutputWriter outputWriter)
        {
            this.cliOptions = cliOptions.Value;
            this.onDiskVersionProvider = onDiskVersionProvider;
            this.sdkStorageVersionProvider = sdkStorageVersionProvider;
            this.externalVersionProvider = externalVersionProvider;
            this.acrVersionProvider = acrVersionProvider;
            this.logger = logger;
            this.outputWriter = outputWriter;
        }

        public string GetDefaultRuntimeVersion()
        {
            if (string.IsNullOrEmpty(this.defaultRuntimeVersion))
            {
                this.defaultRuntimeVersion = this.cliOptions.EnableDynamicInstall
                    ? this.ResolveDynamicDefaultRuntimeVersion()
                    : this.onDiskVersionProvider.GetDefaultRuntimeVersion();
            }

            this.logger.LogDebug("Default runtime version is {defaultRuntimeVersion}", this.defaultRuntimeVersion);

            return this.defaultRuntimeVersion;
        }

        public Dictionary<string, string> GetSupportedVersions()
        {
            if (this.supportedVersions == null)
            {
                this.supportedVersions = this.cliOptions.EnableDynamicInstall
                    ? this.ResolveDynamicSupportedVersions()
                    : this.onDiskVersionProvider.GetSupportedVersions();

                // A temporary fix to make building netcoreapp1.0 versions using the 1.1 SDK
                // This SDK has 2 runtimes: 1.1.13 and 1.0.16
                this.supportedVersions[DotNetCoreRunTimeVersions.NetCoreApp10] =
                    DotNetCoreSdkVersions.DotNetCore11SdkVersion;
            }

            this.logger.LogDebug("Got the list of supported versions");

            return this.supportedVersions;
        }

        // Note: External ACR provider is handled directly by DotNetCorePlatform.ResolveVersions()
        // which short-circuits before calling this provider.
        // Priority here: External-blob → Direct-ACR → CDN
        private string ResolveDynamicDefaultRuntimeVersion()
        {
            if (this.cliOptions.EnableExternalSdkProvider)
            {
                var version = this.TryGetDefaultRuntimeVersionFromExternalBlob();
                if (!string.IsNullOrEmpty(version))
                {
                    this.outputWriter.WriteLine("DotNet version resolved using external SDK provider(blob).");
                    return version;
                }
            }

            if (this.cliOptions.EnableAcrSdkProvider)
            {
                var version = this.TryGetDefaultRuntimeVersionFromAcr();
                if (!string.IsNullOrEmpty(version))
                {
                    this.outputWriter.WriteLine("DotNet version resolved using direct ACR SDK provider.");
                    return version;
                }
            }

            this.outputWriter.WriteLine("DotNet version resolved using blob SDK storage provider(CDN).");
            return this.sdkStorageVersionProvider.GetDefaultRuntimeVersion();
        }

        private Dictionary<string, string> ResolveDynamicSupportedVersions()
        {
            if (this.cliOptions.EnableExternalSdkProvider)
            {
                var versions = this.TryGetSupportedVersionsFromExternalBlob();
                if (versions != null)
                {
                    return versions;
                }
            }

            if (this.cliOptions.EnableAcrSdkProvider)
            {
                var versions = this.TryGetSupportedVersionsFromAcr();
                if (versions != null)
                {
                    return versions;
                }
            }

            return this.sdkStorageVersionProvider.GetSupportedVersions();
        }

        private string TryGetDefaultRuntimeVersionFromExternalBlob()
        {
            try
            {
                return this.externalVersionProvider.GetDefaultRuntimeVersion();
            }
            catch (System.Exception ex)
            {
                this.logger.LogError(
                    $"Error while getting default runtime version from external blob provider. Ex: {ex}");
                return null;
            }
        }

        private string TryGetDefaultRuntimeVersionFromAcr()
        {
            try
            {
                return this.acrVersionProvider.GetDefaultRuntimeVersion();
            }
            catch (System.Exception ex)
            {
                this.logger.LogError(
                    $"Error while getting default runtime version from direct ACR provider. Ex: {ex}");
                return null;
            }
        }

        private Dictionary<string, string> TryGetSupportedVersionsFromExternalBlob()
        {
            try
            {
                return this.externalVersionProvider.GetSupportedVersions();
            }
            catch (System.Exception ex)
            {
                this.logger.LogError(
                    $"Error while getting supported versions from external blob provider. Ex: {ex}");
                return null;
            }
        }

        private Dictionary<string, string> TryGetSupportedVersionsFromAcr()
        {
            try
            {
                return this.acrVersionProvider.GetSupportedVersions();
            }
            catch (System.Exception ex)
            {
                this.logger.LogError(
                    $"Error while getting supported versions from direct ACR provider. Ex: {ex}");
                return null;
            }
        }
    }
}