// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    /// <summary>
    /// ACR-based version provider for .NET SDKs via external socket provider.
    /// Unlike simple platforms that implement I{X}VersionProvider, this exposes
    /// a raw SDK version string. <see cref="DotNetCoreVersionProvider"/> handles
    /// the adaptation to the runtime→SDK map.
    /// </summary>
    public class DotNetCoreExternalAcrVersionProvider : ExternalAcrVersionProviderBase
    {
        private string resolvedVersion;
        private bool resolved;

        public DotNetCoreExternalAcrVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            ILoggerFactory loggerFactory)
            : base(options, loggerFactory)
        {
        }

        /// <summary>
        /// Gets the single SDK version dictated by the external host, or null if unavailable.
        /// </summary>
        public string GetSdkVersion()
        {
            if (!this.resolved)
            {
                this.resolvedVersion = this.GetCompanionSdkVersion(DotNetCoreConstants.PlatformName, debianFlavor: this.DebianFlavor);
                this.resolved = true;
            }

            return this.resolvedVersion;
        }
    }
}
