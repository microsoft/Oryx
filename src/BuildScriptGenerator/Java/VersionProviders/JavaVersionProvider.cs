// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Java
{
    internal class JavaVersionProvider : IJavaVersionProvider
    {
        private readonly BuildScriptGeneratorOptions options;
        private readonly JavaOnDiskVersionProvider onDiskVersionProvider;
        private readonly JavaSdkStorageVersionProvider sdkStorageVersionProvider;
        private readonly ILogger<JavaVersionProvider> logger;
        private PlatformVersionInfo versionInfo;

        public JavaVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            JavaOnDiskVersionProvider onDiskVersionProvider,
            JavaSdkStorageVersionProvider sdkStorageVersionProvider,
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