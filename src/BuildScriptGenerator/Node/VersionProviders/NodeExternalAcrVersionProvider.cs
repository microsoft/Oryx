// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

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
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IExternalAcrSdkProvider externalAcrSdkProvider,
            ILoggerFactory loggerFactory)
            : base(commonOptions, externalAcrSdkProvider, loggerFactory)
        {
        }

        public virtual PlatformVersionInfo GetVersionInfo()
        {
            return this.GetAvailableVersionsFromExternalAcr(platformName: "nodejs");
        }
    }
}
