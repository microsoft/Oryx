// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeSdkStorageVersionProvider : SdkStorageVersionProviderBase, INodeVersionProvider
    {
        private PlatformVersionInfo platformVersionInfo;

        public NodeSdkStorageVersionProvider(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
            : base(commonOptions, httpClientFactory, loggerFactory)
        {
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            if (this.platformVersionInfo == null)
            {
                this.platformVersionInfo = this.GetAvailableVersionsFromStorage(platformName: "nodejs");
            }

            return this.platformVersionInfo;
        }
    }
}