// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpComposerVersionProvider : PlatformVersionProviderBase, IPhpComposerVersionProvider
    {
        private readonly PhpComposerOnDiskVersionProvider onDiskVersionProvider;
        private readonly PhpComposerSdkStorageVersionProvider sdkStorageVersionProvider;
        private readonly PhpComposerExternalVersionProvider externalVersionProvider;
        private readonly PhpComposerExternalAcrVersionProvider externalAcrVersionProvider;
        private readonly PhpComposerAcrVersionProvider acrVersionProvider;

        public PhpComposerVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            PhpComposerOnDiskVersionProvider onDiskVersionProvider,
            PhpComposerSdkStorageVersionProvider sdkStorageVersionProvider,
            PhpComposerExternalVersionProvider externalVersionProvider,
            PhpComposerExternalAcrVersionProvider externalAcrVersionProvider,
            PhpComposerAcrVersionProvider acrVersionProvider,
            ILogger<PhpComposerVersionProvider> logger,
            IStandardOutputWriter outputWriter)
            : base(options.Value, logger, outputWriter)
        {
            this.onDiskVersionProvider = onDiskVersionProvider;
            this.sdkStorageVersionProvider = sdkStorageVersionProvider;
            this.externalVersionProvider = externalVersionProvider;
            this.externalAcrVersionProvider = externalAcrVersionProvider;
            this.acrVersionProvider = acrVersionProvider;
        }

        protected override string PlatformName => "php-composer";

        protected override PlatformVersionInfo GetOnDiskVersionInfo() => this.onDiskVersionProvider.GetVersionInfo();

        protected override PlatformVersionInfo GetSdkStorageVersionInfo() => this.sdkStorageVersionProvider.GetVersionInfo();

        protected override PlatformVersionInfo GetExternalVersionInfo() => this.externalVersionProvider.GetVersionInfo();

        protected override PlatformVersionInfo GetExternalAcrVersionInfo() => this.externalAcrVersionProvider.GetVersionInfo();

        protected override PlatformVersionInfo GetAcrVersionInfo() => this.acrVersionProvider.GetVersionInfo();
    }
}