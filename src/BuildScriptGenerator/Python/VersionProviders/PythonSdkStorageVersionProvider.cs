// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonSdkStorageVersionProvider : SdkStorageVersionProviderBase, IPythonVersionProvider
    {
        public PythonSdkStorageVersionProvider(
            IEnvironment environment, 
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
            : base(environment, httpClientFactory, loggerFactory)
        {
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            return GetAvailableVersionsFromStorage(
                platformName: ToolNameConstants.PythonName,
                versionMetadataElementName: "Version");
        }
    }
}