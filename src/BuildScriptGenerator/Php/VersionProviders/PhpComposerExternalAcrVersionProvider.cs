// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    /// <summary>
    /// ACR-based version provider for PHP Composer SDKs via external socket provider.
    /// Parallel to <see cref="PhpComposerExternalVersionProvider"/> (blob) and
    /// <see cref="PhpComposerAcrVersionProvider"/> (direct OCI).
    /// </summary>
    internal class PhpComposerExternalAcrVersionProvider : ExternalAcrVersionProviderBase, IPhpVersionProvider
    {
        public PhpComposerExternalAcrVersionProvider(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IExternalAcrSdkProvider externalAcrSdkProvider,
            ILoggerFactory loggerFactory)
            : base(commonOptions, externalAcrSdkProvider, loggerFactory)
        {
        }

        public virtual PlatformVersionInfo GetVersionInfo()
        {
            return this.GetAvailableVersionsFromExternalAcr(platformName: "php-composer");
        }
    }
}
