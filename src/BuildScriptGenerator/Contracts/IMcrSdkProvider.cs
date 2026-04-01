// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Interface for pulling SDKs from MCR/ACR container images.
    /// </summary>
    public interface IMcrSdkProvider
    {
        /// <summary>
        /// Pulls the SDK tarball from an MCR/ACR container image and places it
        /// in the external SDKs storage directory so existing installation logic can use it.
        /// </summary>
        /// <param name="platformName">The platform name (e.g. "nodejs", "python", "dotnet", "php").</param>
        /// <param name="version">The platform version (e.g. "18.20.8").</param>
        /// <param name="debianFlavor">The debian flavor (e.g. "bookworm").</param>
        /// <returns>True if the SDK was pulled and cached successfully.</returns>
        Task<bool> PullSdkAsync(string platformName, string version, string debianFlavor);
    }
}
