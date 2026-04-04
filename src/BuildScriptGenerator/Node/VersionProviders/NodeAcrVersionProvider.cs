// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    /// <summary>
    /// ACR-based version provider for Node.js SDKs.
    /// Parallel to <see cref="NodeSdkStorageVersionProvider"/> but uses OCI Distribution API.
    /// </summary>
    internal class NodeAcrVersionProvider : AcrVersionProviderBase, INodeVersionProvider
    {
        private PlatformVersionInfo platformVersionInfo;

        public NodeAcrVersionProvider(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
            : base(commonOptions, httpClientFactory, loggerFactory)
        {
        }

        public virtual PlatformVersionInfo GetVersionInfo()
        {
            return this.platformVersionInfo
                ??= this.GetAvailableVersionsFromAcr(
                    platformName: "nodejs",
                    defaultVersionPerFlavor: NodeConstants.DefaultVersionPerFlavor);
        }
    }
}
