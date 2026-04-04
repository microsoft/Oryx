// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Fetches SDK tarballs directly from an OCI container registry.
    /// SDK images are single-layer <c>FROM scratch</c> images where
    /// the layer IS the SDK tarball.
    /// </summary>
    /// <remarks>
    /// Gated by the <c>ORYX_ENABLE_ACR_SDK_PROVIDER</c> feature flag.
    /// Alternative to <see cref="IExternalSdkProvider"/> (blob storage via socket).
    /// </remarks>
    public interface IAcrSdkProvider
    {
        /// <summary>
        /// Pulls an SDK image from the registry and saves the tarball to
        /// the dynamic install directory (writable by Oryx).
        /// </summary>
        /// <param name="platformName">The platform name (e.g., "nodejs", "python", "dotnet", "php").</param>
        /// <param name="version">The SDK version (e.g., "20.19.3").</param>
        /// <param name="debianFlavor">The Debian flavor (e.g., "bookworm", "bullseye").</param>
        /// <returns>
        /// The absolute path to the downloaded tarball, or <c>null</c> if the pull failed.
        /// </returns>
        Task<string> RequestSdkFromAcrAsync(string platformName, string version, string debianFlavor);
    }
}
