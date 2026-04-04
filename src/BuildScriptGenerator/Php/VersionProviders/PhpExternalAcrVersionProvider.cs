// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    /// <summary>
    /// ACR-based version provider for PHP SDKsvia external socket provider.
    /// Parallel to <see cref="PhpExternalVersionProvider"/> (blob) and
    /// <see cref="PhpAcrVersionProvider"/> (direct OCI).
    /// </summary>
    internal class PhpExternalAcrVersionProvider : ExternalAcrVersionProviderBase, IPhpVersionProvider
    {
        public PhpExternalAcrVersionProvider(
            ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        public virtual PlatformVersionInfo GetVersionInfo()
        {
            var version = this.GetCompanionSdkVersion(platformName: ToolNameConstants.PhpName);
            if (string.IsNullOrEmpty(version))
            {
                return null;
            }

            return PlatformVersionInfo.CreateOnDiskVersionInfo(
                supportedVersions: new[] { version },
                defaultVersion: version);
        }
    }
}
