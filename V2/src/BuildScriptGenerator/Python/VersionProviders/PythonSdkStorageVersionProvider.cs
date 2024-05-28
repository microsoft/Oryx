// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonSdkStorageVersionProvider : SdkStorageVersionProviderBase, IPythonVersionProvider
    {
        public PythonSdkStorageVersionProvider(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
            : base(commonOptions, httpClientFactory, loggerFactory)
        {
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            return this.GetAvailableVersionsFromStorage(platformName: ToolNameConstants.PythonName);
        }
    }
}