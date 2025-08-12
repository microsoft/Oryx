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
        private readonly ILogger<DotNetCoreVersionProvider> logger;
        private string defaultRuntimeVersion;
        private Dictionary<string, string> supportedVersions;

        public DotNetCoreVersionProvider(
            IOptions<BuildScriptGeneratorOptions> cliOptions,
            DotNetCoreOnDiskVersionProvider onDiskVersionProvider,
            DotNetCoreSdkStorageVersionProvider sdkStorageVersionProvider,
            DotNetCoreExternalVersionProvider externalVersionProvider,
            ILogger<DotNetCoreVersionProvider> logger)
        {
            this.cliOptions = cliOptions.Value;
            this.onDiskVersionProvider = onDiskVersionProvider;
            this.sdkStorageVersionProvider = sdkStorageVersionProvider;
            this.externalVersionProvider = externalVersionProvider;
            this.logger = logger;
        }

        public string GetDefaultRuntimeVersion()
        {
            if (string.IsNullOrEmpty(this.defaultRuntimeVersion))
            {
                if (this.cliOptions.EnableDynamicInstall)
                {
                    if (this.cliOptions.EnableExternalSdkProvider)
                    {
                        try
                        {
                            this.defaultRuntimeVersion = this.externalVersionProvider.GetDefaultRuntimeVersion();
                        }
                        catch (System.Exception ex)
                        {
                            this.logger.LogError($"Failed to get default runtime version from external SDK provider. Falling back to http based sdkStorageVersionProvider. Ex: {ex}");
                            this.defaultRuntimeVersion = this.sdkStorageVersionProvider.GetDefaultRuntimeVersion();
                        }
                    }
                    else
                    {
                        this.defaultRuntimeVersion = this.sdkStorageVersionProvider.GetDefaultRuntimeVersion();
                    }
                }
                else
                {
                    this.defaultRuntimeVersion = this.onDiskVersionProvider.GetDefaultRuntimeVersion();
                }
            }

            this.logger.LogDebug("Default runtime version is {defaultRuntimeVersion}", this.defaultRuntimeVersion);

            return this.defaultRuntimeVersion;
        }

        public Dictionary<string, string> GetSupportedVersions()
        {
            if (this.supportedVersions == null)
            {
                if (this.cliOptions.EnableDynamicInstall)
                {
                    if (this.cliOptions.EnableExternalSdkProvider)
                    {
                        try
                        {
                            this.supportedVersions = this.externalVersionProvider.GetSupportedVersions();
                        }
                        catch (System.Exception ex)
                        {
                            this.logger.LogError($"Failed to get supported versions from external SDK provider. Falling back to http based sdkStorageVersionProvider. Ex: {ex}");
                            this.supportedVersions = this.sdkStorageVersionProvider.GetSupportedVersions();
                        }
                    }
                    else
                    {
                        this.supportedVersions = this.sdkStorageVersionProvider.GetSupportedVersions();
                    }
                }
                else
                {
                    this.supportedVersions = this.onDiskVersionProvider.GetSupportedVersions();
                }

                // A temporary fix to make building netcoreapp1.0 versions using the 1.1 SDK
                // This SDK has 2 runtimes: 1.1.13 and 1.0.16
                this.supportedVersions[DotNetCoreRunTimeVersions.NetCoreApp10] =
                    DotNetCoreSdkVersions.DotNetCore11SdkVersion;
            }

            this.logger.LogDebug("Got the list of supported versions");

            return this.supportedVersions;
        }
    }
}