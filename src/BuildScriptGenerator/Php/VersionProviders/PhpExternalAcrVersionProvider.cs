// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    /// <summary>
    /// ACR-based version provider for PHP SDKs via external socket provider.
    /// Parallel to <see cref="PhpExternalVersionProvider"/> (blob) and
    /// <see cref="PhpAcrVersionProvider"/> (direct OCI).
    /// </summary>
    internal class PhpExternalAcrVersionProvider : ExternalAcrVersionProviderBase, IPhpVersionProvider
    {
        public PhpExternalAcrVersionProvider(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            ILoggerFactory loggerFactory)
            : base(commonOptions, loggerFactory)
        {
        }

        public virtual PlatformVersionInfo GetVersionInfo()
        {
            return this.GetAvailableVersionsFromExternalAcr(platformName: ToolNameConstants.PhpName);
        }
    }
}
