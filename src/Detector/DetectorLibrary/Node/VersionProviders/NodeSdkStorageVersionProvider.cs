// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.Detector.Node
{
    internal class NodeSdkStorageVersionProvider : SdkStorageVersionProviderBase
    {
        private PlatformVersionInfo _platformVersionInfo;

        public NodeSdkStorageVersionProvider(
            IOptions<DetectorOptions> detectorOptions,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
            : base(detectorOptions, httpClientFactory, loggerFactory)
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