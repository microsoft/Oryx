// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpComposerVersionProvider : IPhpComposerVersionProvider
    {
        private readonly BuildScriptGeneratorOptions options;
        private readonly PhpComposerOnDiskVersionProvider onDiskVersionProvider;
        private readonly PhpComposerSdkStorageVersionProvider sdkStorageVersionProvider;
        private readonly PhpComposerExternalVersionProvider externalVersionProvider;
        private readonly ILogger<PhpComposerVersionProvider> logger;
        private PlatformVersionInfo versionInfo;

        public PhpComposerVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            PhpComposerOnDiskVersionProvider onDiskVersionProvider,
            PhpComposerSdkStorageVersionProvider sdkStorageVersionProvider,
            PhpComposerExternalVersionProvider externalVersionProvider,
            ILogger<PhpComposerVersionProvider> logger)
        {
            this.options = options.Value;
            this.onDiskVersionProvider = onDiskVersionProvider;
            this.sdkStorageVersionProvider = sdkStorageVersionProvider;
            this.externalVersionProvider = externalVersionProvider;
            this.logger = logger;
        }

        public PlatformVersionInfo GetVersionInfo()
        {
            if (this.versionInfo == null)
            {
                if (this.options.EnableDynamicInstall)
                {
                    if (this.options.EnableExternalSdkProvider)
                    {
                        try
                        {
                            return this.externalVersionProvider.GetVersionInfo();
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogError($"Failed to get version info from external SDK provider. Falling back to http based sdkStorageVersionProvider. Ex: {ex}");
                        }
                    }

                    return this.sdkStorageVersionProvider.GetVersionInfo();
                }

                this.versionInfo = this.onDiskVersionProvider.GetVersionInfo();
            }

            return this.versionInfo;
        }
    }
}