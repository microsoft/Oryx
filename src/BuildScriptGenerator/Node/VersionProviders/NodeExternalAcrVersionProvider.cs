// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;

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
            ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        public virtual PlatformVersionInfo GetVersionInfo()
        {
            var version = this.GetCompanionSdkVersion(platformName: "nodejs");
            return PlatformVersionInfo.CreateOnDiskVersionInfo(
                supportedVersions: version != null ? new[] { version } : Array.Empty<string>(),
                defaultVersion: version);
        }
    }
}
