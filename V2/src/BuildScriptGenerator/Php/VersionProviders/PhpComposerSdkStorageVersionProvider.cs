// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpComposerSdkStorageVersionProvider : SdkStorageVersionProviderBase, IPhpVersionProvider
    {
        public PhpComposerSdkStorageVersionProvider(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
            : base(commonOptions, httpClientFactory, loggerFactory)
        {
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            return this.GetAvailableVersionsFromStorage(platformName: "php-composer");
        }
    }
}