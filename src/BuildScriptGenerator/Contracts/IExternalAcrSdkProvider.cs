// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// ACR-based SDK provider that communicates over a Unix socket to pull
    /// SDK images from ACR. This is the ACR equivalent of <see cref="IExternalSdkProvider"/>
    /// which uses blob storage. The external host proxies the ACR calls.
    /// </summary>
    /// <remarks>
    /// Flow: Oryx → socket → external host → ACR.
    /// Gated by the <c>ORYX_ENABLE_ACR_SDK_PROVIDER</c> feature flag.
    /// </remarks>
    public interface IExternalAcrSdkProvider
    {
        /// <summary>
        /// Gets available SDK versions for a platform from ACR via the external provider.
        /// The external host queries ACR tag listing and returns versions as a newline-delimited string.
        /// </summary>
        /// <param name="platformName">The platform name (e.g., "python", "nodejs", "dotnet", "php").</param>
        /// <param name="debianFlavor">The Debian flavor (e.g., "bookworm", "bullseye").</param>
        /// <returns>A list of available SDK versions.</returns>
        Task<IList<string>> GetVersionsAsync(string platformName, string debianFlavor);

        /// <summary>
        /// Gets the default SDK version for a platform from ACR via the external provider.
        /// </summary>
        /// <param name="platformName">The platform name.</param>
        /// <param name="debianFlavor">The Debian flavor.</param>
        /// <returns>The default version string, or null if not found.</returns>
        Task<string> GetDefaultVersionAsync(string platformName, string debianFlavor);

        /// <summary>
        /// Pulls an SDK tarball from ACR via the external provider and places it in the local cache.
        /// </summary>
        /// <param name="platformName">The platform name.</param>
        /// <param name="version">The SDK version.</param>
        /// <param name="debianFlavor">The Debian flavor.</param>
        /// <returns>True if the SDK was successfully pulled and cached.</returns>
        Task<bool> RequestSdkAsync(string platformName, string version, string debianFlavor);
    }
}
