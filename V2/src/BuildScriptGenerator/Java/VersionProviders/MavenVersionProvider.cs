// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Java
{
    internal class MavenVersionProvider : IMavenVersionProvider
    {
        private readonly BuildScriptGeneratorOptions options;
        private readonly MavenOnDiskVersionProvider onDiskVersionProvider;
        private readonly MavenSdkStorageVersionProvider sdkStorageVersionProvider;
        private readonly ILogger<JavaVersionProvider> logger;
        private PlatformVersionInfo versionInfo;

        public MavenVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            MavenOnDiskVersionProvider onDiskVersionProvider,
            MavenSdkStorageVersionProvider sdkStorageVersionProvider,
            ILogger<JavaVersionProvider> logger)
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