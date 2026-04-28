// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpVersionProvider : PlatformVersionProviderBase, IPhpVersionProvider
    {
        private readonly PhpOnDiskVersionProvider onDiskVersionProvider;
        private readonly PhpSdkStorageVersionProvider sdkStorageVersionProvider;
        private readonly PhpExternalVersionProvider externalVersionProvider;
        private readonly PhpExternalAcrVersionProvider externalAcrVersionProvider;
        private readonly PhpAcrVersionProvider acrVersionProvider;

        public PhpVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            PhpOnDiskVersionProvider onDiskVersionProvider,
            PhpSdkStorageVersionProvider sdkStorageVersionProvider,
            PhpExternalVersionProvider externalVersionProvider,
            PhpExternalAcrVersionProvider externalAcrVersionProvider,
            PhpAcrVersionProvider acrVersionProvider,
            ILogger<PhpVersionProvider> logger,
            IStandardOutputWriter outputWriter)
            : base(options.Value, logger, outputWriter)
        {
            this.onDiskVersionProvider = onDiskVersionProvider;
            this.sdkStorageVersionProvider = sdkStorageVersionProvider;
            this.externalVersionProvider = externalVersionProvider;
            this.externalAcrVersionProvider = externalAcrVersionProvider;
            this.acrVersionProvider = acrVersionProvider;
        }

        protected override string PlatformName => "php";

        protected override PlatformVersionInfo GetOnDiskVersionInfo() => this.onDiskVersionProvider.GetVersionInfo();

        protected override PlatformVersionInfo GetSdkStorageVersionInfo() => this.sdkStorageVersionProvider.GetVersionInfo();

        protected override PlatformVersionInfo GetExternalVersionInfo() => this.externalVersionProvider.GetVersionInfo();

        protected override PlatformVersionInfo GetExternalAcrVersionInfo() => this.externalAcrVersionProvider.GetVersionInfo();

        protected override PlatformVersionInfo GetAcrVersionInfo() => this.acrVersionProvider.GetVersionInfo();
    }
}