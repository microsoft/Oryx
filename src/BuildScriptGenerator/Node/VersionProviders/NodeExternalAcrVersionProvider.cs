// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    /// <summary>
    /// ACR-based version provider for Node SDKs via external socket provider.
    /// Parallel to <see cref="NodeExternalVersionProvider"/> (blob) and
    /// <see cref="NodeAcrVersionProvider"/> (direct OCI).
    /// </summary>
    internal class NodeExternalAcrVersionProvider : ExternalAcrVersionProviderBase, INodeVersionProvider
    {
        public NodeExternalAcrVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            ILoggerFactory loggerFactory,
            IStandardOutputWriter outputWriter)
            : base(options, loggerFactory, outputWriter)
        {
        }

        public virtual PlatformVersionInfo GetVersionInfo()
        {
            var version = this.GetCompanionSdkVersion(platformName: "nodejs", debianFlavor: this.DebianFlavor);
            if (string.IsNullOrEmpty(version))
            {
                return null;
            }

            return PlatformVersionInfo.CreateAvailableOnAcr(
                supportedVersions: new[] { version },
                defaultVersion: version);
        }
    }
}
