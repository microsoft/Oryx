// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Java
{
    internal class JavaSdkStorageVersionProvider : SdkStorageVersionProviderBase, IJavaVersionProvider
    {
        private PlatformVersionInfo platformVersionInfo;

        public JavaSdkStorageVersionProvider(
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
                this.platformVersionInfo = this.GetAvailableVersionsFromStorage(platformName: JavaConstants.PlatformName);
            }

            return this.platformVersionInfo;
        }
    }
}