// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Interface for MCR (Microsoft Container Registry) SDK provider that pulls
    /// SDK tarballs from Docker images hosted in MCR.
    /// </summary>
    public interface IMcrSdkProvider
    {
        /// <summary>
        /// The default base URL for MCR SDK images.
        /// Images follow the convention: {BaseUrl}/{platformName}:{version}-{debianFlavor}.
        /// </summary>
        public const string DefaultMcrSdkImageBaseUrl = "mcr.microsoft.com/oryx/sdks";

        /// <summary>
        /// The directory inside the Docker image where the SDK tarball is stored.
        /// </summary>
        public const string SdkDirectoryInImage = "/sdks";

        /// <summary>
        /// Pulls an SDK tarball from a Docker image in MCR and stores it in the local cache.
        /// The tarball is extracted from the image and placed at the same cache directory
        /// used by <see cref="IExternalSdkProvider"/> so that existing installation scripts
        /// can locate and extract it.
        /// </summary>
        /// <param name="platformName">The name of the platform (e.g., "nodejs", "python", "dotnet").</param>
        /// <param name="version">The version of the SDK to pull.</param>
        /// <param name="debianFlavor">The Debian flavor (e.g., "bookworm", "bullseye").</param>
        /// <returns>True if the SDK was successfully pulled and cached; false otherwise.</returns>
        Task<bool> PullSdkAsync(string platformName, string version, string debianFlavor);
    }
}
