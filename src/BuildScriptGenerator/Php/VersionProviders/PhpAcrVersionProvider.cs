// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    /// <summary>
    /// ACR-based version provider for PHP SDKs.
    /// Parallel to <see cref="PhpSdkStorageVersionProvider"/> but uses OCI Distribution API.
    /// </summary>
    internal class PhpAcrVersionProvider : AcrVersionProviderBase, IPhpVersionProvider
    {
        private PlatformVersionInfo platformVersionInfo;

        public PhpAcrVersionProvider(
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
                this.platformVersionInfo = this.GetAvailableVersionsFromAcr(platformName: ToolNameConstants.PhpName);
            }

            return this.platformVersionInfo;
        }
    }
}
