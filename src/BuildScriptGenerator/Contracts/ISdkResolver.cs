// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Resolves SDK binaries by trying multiple sources in priority order:
    /// MCR Docker images → External SDK provider (blob storage) → CDN fallback.
    /// </summary>
    public interface ISdkResolver
    {
        /// <summary>
        /// Attempts to fetch the SDK tarball for the given platform and version from available
        /// SDK sources (MCR, External SDK provider). If a source succeeds, the tarball will be
        /// available in the shared SDK cache directory and the caller can generate an installation
        /// script that skips the binary download.
        /// </summary>
        /// <param name="platformName">The name of the platform (e.g., "nodejs", "python", "dotnet").</param>
        /// <param name="version">The version of the SDK to fetch.</param>
        /// <param name="debianFlavor">The Debian flavor (e.g., "bookworm", "bullseye").</param>
        /// <returns>
        /// True if the SDK was successfully fetched and cached from any source;
        /// false if no source could provide the SDK (caller should fall back to CDN download).
        /// </returns>
        bool TryFetchSdk(string platformName, string version, string debianFlavor);
    }
}
