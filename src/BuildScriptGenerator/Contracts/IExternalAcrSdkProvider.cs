// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Pulls SDK tarballs from ACR via an external host over a Unix socket.
    /// This is the ACR equivalent of <see cref="IExternalSdkProvider"/> (blob storage via socket).
    /// </summary>
    /// <remarks>
    /// Flow: Oryx → Unix socket → external host → ACR.
    /// Gated by the <c>ORYX_ENABLE_EXTERNAL_ACR_SDK_PROVIDER</c> feature flag.
    /// Version discovery is handled separately by <see cref="ExternalAcrVersionProviderBase"/>.
    /// </remarks>
    public interface IExternalAcrSdkProvider
    {
        /// <summary>
        /// Pulls an SDK tarball from ACR via the external provider and places it in the local cache.
        /// </summary>
        /// <param name="platformName">The platform name (e.g., "python", "nodejs", "dotnet", "php").</param>
        /// <param name="version">The SDK version.</param>
        /// <param name="debianFlavor">The Debian flavor (e.g., "bookworm", "bullseye").</param>
        /// <returns>True if the SDK was successfully pulled and cached.</returns>
        Task<bool> RequestSdkAsync(string platformName, string version, string debianFlavor);
    }
}
