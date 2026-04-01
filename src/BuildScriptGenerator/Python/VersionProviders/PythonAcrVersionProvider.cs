// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    /// <summary>
    /// ACR-based version provider for Python SDKs.
    /// Parallel to <see cref="PythonSdkStorageVersionProvider"/> but uses OCI Distribution API.
    /// </summary>
    internal class PythonAcrVersionProvider : AcrVersionProviderBase, IPythonVersionProvider
    {
        private PlatformVersionInfo platformVersionInfo;

        public PythonAcrVersionProvider(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
            : base(commonOptions, httpClientFactory, loggerFactory)
        {
        }

        public virtual PlatformVersionInfo GetVersionInfo()
        {
            if (this.platformVersionInfo == null)
            {
                this.platformVersionInfo = this.GetAvailableVersionsFromAcr(platformName: ToolNameConstants.PythonName);
            }

            return this.platformVersionInfo;
        }
    }
}
