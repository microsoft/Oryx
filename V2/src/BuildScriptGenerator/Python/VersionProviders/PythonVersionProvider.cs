// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonVersionProvider : IPythonVersionProvider
    {
        private readonly BuildScriptGeneratorOptions options;
        private readonly PythonOnDiskVersionProvider onDiskVersionProvider;
        private readonly PythonSdkStorageVersionProvider sdkStorageVersionProvider;
        private readonly ILogger<PythonVersionProvider> logger;
        private PlatformVersionInfo versionInfo;

        public PythonVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            PythonOnDiskVersionProvider onDiskVersionProvider,
            PythonSdkStorageVersionProvider sdkStorageVersionProvider,
            ILogger<PythonVersionProvider> logger)
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