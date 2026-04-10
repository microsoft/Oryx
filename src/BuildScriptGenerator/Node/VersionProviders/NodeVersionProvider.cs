// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeVersionProvider : PlatformVersionProviderBase, INodeVersionProvider
    {
        private readonly NodeOnDiskVersionProvider onDiskVersionProvider;
        private readonly NodeSdkStorageVersionProvider sdkStorageVersionProvider;
        private readonly NodeExternalVersionProvider externalVersionProvider;
        private readonly NodeExternalAcrVersionProvider externalAcrVersionProvider;
        private readonly NodeAcrVersionProvider acrVersionProvider;

        public NodeVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            NodeOnDiskVersionProvider onDiskVersionProvider,
            NodeSdkStorageVersionProvider sdkStorageVersionProvider,
            NodeExternalVersionProvider externalVersionProvider,
            NodeExternalAcrVersionProvider externalAcrVersionProvider,
            NodeAcrVersionProvider acrVersionProvider,
            ILogger<NodeVersionProvider> logger,
            IStandardOutputWriter outputWriter)
            : base(options.Value, logger, outputWriter)
        {
            this.onDiskVersionProvider = onDiskVersionProvider;
            this.sdkStorageVersionProvider = sdkStorageVersionProvider;
            this.externalVersionProvider = externalVersionProvider;
            this.externalAcrVersionProvider = externalAcrVersionProvider;
            this.acrVersionProvider = acrVersionProvider;
        }

        protected override string PlatformName => "nodejs";

        protected override PlatformVersionInfo GetOnDiskVersionInfo() => this.onDiskVersionProvider.GetVersionInfo();

        protected override PlatformVersionInfo GetSdkStorageVersionInfo() => this.sdkStorageVersionProvider.GetVersionInfo();

        protected override PlatformVersionInfo GetExternalVersionInfo() => this.externalVersionProvider.GetVersionInfo();

        protected override PlatformVersionInfo GetExternalAcrVersionInfo() => this.externalAcrVersionProvider.GetVersionInfo();

        protected override PlatformVersionInfo GetAcrVersionInfo() => this.acrVersionProvider.GetVersionInfo();
    }
}