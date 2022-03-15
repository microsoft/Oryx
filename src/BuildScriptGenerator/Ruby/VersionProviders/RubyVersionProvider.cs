// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Ruby
{
    internal class RubyVersionProvider : IRubyVersionProvider
    {
        private readonly BuildScriptGeneratorOptions options;
        private readonly RubyOnDiskVersionProvider onDiskVersionProvider;
        private readonly RubySdkStorageVersionProvider sdkStorageVersionProvider;
        private readonly ILogger<RubyVersionProvider> logger;
        private PlatformVersionInfo versionInfo;

        public RubyVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            RubyOnDiskVersionProvider onDiskVersionProvider,
            RubySdkStorageVersionProvider sdkStorageVersionProvider,
            ILogger<RubyVersionProvider> logger)
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