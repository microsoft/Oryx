// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeSdkStorageVersionProvider : SdkStorageVersionProviderBase, INodeVersionProvider
    {
        private PlatformVersionInfo _platformVersionInfo;

        public NodeSdkStorageVersionProvider(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
            : base(environment, httpClientFactory, loggerFactory)
        {
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            if (_platformVersionInfo == null)
            {
                _platformVersionInfo = GetAvailableVersionsFromStorage(
                    platformName: "nodejs",
                    versionMetadataElementName: "Version");
            }

            return _platformVersionInfo;
        }
    }
}