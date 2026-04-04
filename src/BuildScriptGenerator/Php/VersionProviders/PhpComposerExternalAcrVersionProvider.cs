// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    /// <summary>
    /// ACR-based version provider for PHP Composer SDKsvia external socket provider.
    /// Parallel to <see cref="PhpComposerExternalVersionProvider"/> (blob) and
    /// <see cref="PhpComposerAcrVersionProvider"/> (direct OCI).
    /// </summary>
    internal class PhpComposerExternalAcrVersionProvider : ExternalAcrVersionProviderBase, IPhpVersionProvider
    {
        public PhpComposerExternalAcrVersionProvider(
            ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        public virtual PlatformVersionInfo GetVersionInfo()
        {
            var version = this.GetCompanionSdkVersion(platformName: "php-composer");
            return PlatformVersionInfo.CreateOnDiskVersionInfo(
                supportedVersions: version != null ? new[] { version } : Array.Empty<string>(),
                defaultVersion: version);
        }
    }
}
