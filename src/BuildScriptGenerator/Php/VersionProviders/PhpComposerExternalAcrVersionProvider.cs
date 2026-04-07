// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    /// <summary>
    /// ACR-based version provider for PHP Composer SDKs via external socket provider.
    /// Parallel to <see cref="PhpComposerExternalVersionProvider"/> (blob) and
    /// <see cref="PhpComposerAcrVersionProvider"/> (direct OCI).
    /// </summary>
    internal class PhpComposerExternalAcrVersionProvider : ExternalAcrVersionProviderBase, IPhpComposerVersionProvider
    {
        public PhpComposerExternalAcrVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            ILoggerFactory loggerFactory,
            IStandardOutputWriter outputWriter)
            : base(options, loggerFactory, outputWriter)
        {
        }

        public virtual PlatformVersionInfo GetVersionInfo()
        {
            var availableVersions = this.GetAvailableSdkVersions(platformName: "php-composer", debianFlavor: this.DebianFlavor);
            if (availableVersions == null || availableVersions.Count == 0)
            {
                return null;
            }

            return PlatformVersionInfo.CreateAvailableOnAcr(
                supportedVersions: availableVersions.ToArray(),
                defaultVersion: PhpVersions.ComposerDefaultVersion);
        }
    }
}
