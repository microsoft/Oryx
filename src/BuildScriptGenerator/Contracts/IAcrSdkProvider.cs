// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Pulls SDK tarballs directly from an OCI-compliant container registry (ACR)
    /// using the OCI Distribution API. SDK images are single-layer <c>FROM scratch</c>
    /// images where the layer IS the SDK tarball.
    /// </summary>
    /// <remarks>
    /// Gated by the <c>ORYX_ENABLE_ACR_SDK_PROVIDER</c> feature flag.
    /// This is an alternative to <see cref="IExternalSdkProvider"/> (blob storage via socket).
    /// </remarks>
    public interface IAcrSdkProvider
    {
        /// <summary>
        /// Pulls an SDK image from the ACR registry and saves the SDK tarball to the local cache.
        /// </summary>
        /// <param name="platformName">The platform name (e.g., "nodejs", "python", "dotnet", "php").</param>
        /// <param name="version">The SDK version (e.g., "20.19.3").</param>
        /// <param name="debianFlavor">The Debian flavor (e.g., "bookworm", "bullseye").</param>
        /// <returns>True if the SDK was successfully pulled and cached.</returns>
        Task<bool> RequestSdkFromAcrAsync(string platformName, string version, string debianFlavor);
    }
}
