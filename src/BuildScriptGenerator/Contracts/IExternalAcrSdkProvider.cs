// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Interface for external ACR-based SDK provider that communicates with LWASv2
    /// to request SDK downloads from Azure Container Registry (WAWS Images ACR).
    /// This is the ACR equivalent of <see cref="IExternalSdkProvider"/> which uses blob storage.
    /// </summary>
    /// <remarks>
    /// Gated by the <c>ORYX_ENABLE_ACR_SDK_PROVIDER</c> feature flag.
    /// When enabled and LWASv2 is available, this provider tells LWASv2 to pull
    /// the SDK OCI image from the WAWS Images ACR and extract the SDK tarball to disk.
    /// </remarks>
    public interface IExternalAcrSdkProvider
    {
        /// <summary>
        /// Requests LWASv2 to pull an SDK image from the WAWS Images ACR and extract it to the local cache.
        /// </summary>
        /// <param name="platformName">The platform name (e.g., "nodejs", "python", "dotnet", "php").</param>
        /// <param name="version">The SDK version (e.g., "20.19.3").</param>
        /// <param name="debianFlavor">The Debian flavor (e.g., "bookworm", "bullseye").</param>
        /// <returns>True if the SDK was successfully pulled and extracted by LWASv2.</returns>
        Task<bool> RequestSdkFromAcrAsync(string platformName, string version, string debianFlavor);
    }
}
