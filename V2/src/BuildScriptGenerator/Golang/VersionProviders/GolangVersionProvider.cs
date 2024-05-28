// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Golang
{
    internal class GolangVersionProvider : IGolangVersionProvider
    {
        private readonly BuildScriptGeneratorOptions options;
        private readonly GolangOnDiskVersionProvider onDiskVersionProvider;
        private readonly GolangSdkStorageVersionProvider sdkStorageVersionProvider;
        private readonly ILogger<GolangVersionProvider> logger;
        private PlatformVersionInfo versionInfo;

        public GolangVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            GolangOnDiskVersionProvider onDiskVersionProvider,
            GolangSdkStorageVersionProvider sdkStorageVersionProvider,
            ILogger<GolangVersionProvider> logger)
        {
            this.options = options.Value;
            this.onDiskVersionProvider = onDiskVersionProvider;
            this.sdkStorageVersionProvider = sdkStorageVersionProvider;
            this.logger = logger;
        }

        public PlatformVersionInfo GetVersionInfo()
        {
            if (this.versionInfo == null)
            {
                if (this.options.EnableDynamicInstall)
                {
                    return this.sdkStorageVersionProvider.GetVersionInfo();
                }

                this.versionInfo = this.onDiskVersionProvider.GetVersionInfo();
            }

            return this.versionInfo;
        }
    }
}
