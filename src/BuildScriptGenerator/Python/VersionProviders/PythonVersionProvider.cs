// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonVersionProvider : PlatformVersionProviderBase, IPythonVersionProvider
    {
        private readonly PythonOnDiskVersionProvider onDiskVersionProvider;
        private readonly PythonSdkStorageVersionProvider sdkStorageVersionProvider;
        private readonly PythonExternalVersionProvider externalVersionProvider;
        private readonly PythonExternalAcrVersionProvider externalAcrVersionProvider;
        private readonly PythonAcrVersionProvider acrVersionProvider;

        public PythonVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            PythonOnDiskVersionProvider onDiskVersionProvider,
            PythonSdkStorageVersionProvider sdkStorageVersionProvider,
            PythonExternalVersionProvider externalVersionProvider,
            PythonExternalAcrVersionProvider externalAcrVersionProvider,
            PythonAcrVersionProvider acrVersionProvider,
            ILogger<PythonVersionProvider> logger,
            IStandardOutputWriter outputWriter)
            : base(options.Value, logger, outputWriter)
        {
            this.onDiskVersionProvider = onDiskVersionProvider;
            this.sdkStorageVersionProvider = sdkStorageVersionProvider;
            this.externalVersionProvider = externalVersionProvider;
            this.externalAcrVersionProvider = externalAcrVersionProvider;
            this.acrVersionProvider = acrVersionProvider;
        }

        protected override string PlatformName => "python";

        protected override PlatformVersionInfo GetOnDiskVersionInfo() => this.onDiskVersionProvider.GetVersionInfo();

        protected override PlatformVersionInfo GetSdkStorageVersionInfo() => this.sdkStorageVersionProvider.GetVersionInfo();

        protected override PlatformVersionInfo GetExternalVersionInfo() => this.externalVersionProvider.GetVersionInfo();

        protected override PlatformVersionInfo GetExternalAcrVersionInfo() => this.externalAcrVersionProvider.GetVersionInfo();

        protected override PlatformVersionInfo GetAcrVersionInfo() => this.acrVersionProvider.GetVersionInfo();
    }
}