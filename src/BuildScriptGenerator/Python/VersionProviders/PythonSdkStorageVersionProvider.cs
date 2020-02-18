// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonSdkStorageVersionProvider : SdkStorageVersionProviderBase, IPythonVersionProvider
    {
        public PythonSdkStorageVersionProvider(IEnvironment environment, IHttpClientFactory httpClientFactory)
            : base(environment, httpClientFactory)
        {
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            return GetAvailableVersionsFromStorage(
                platformName: "python",
                versionMetadataElementName: "version");
        }
    }
}