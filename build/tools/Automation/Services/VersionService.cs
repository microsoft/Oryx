// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using NuGet.Versioning;

namespace Microsoft.Oryx.Automation.Services
{
    public class VersionService : IVersionService
    {
        /// <summary>
        /// Determines if the specified version is within the range specified by the minimum and maximum versions,
        /// and not in the list of blocked versions.
        /// </summary>
        /// <param name="version">The version to check.</param>
        /// <param name="minVersion">The minimum allowed version. If not specified, there is no minimum limit.</param>
        /// <param name="maxVersion">The maximum allowed version. If not specified, there is no maximum limit.</param>
        /// <param name="blockedVersions">A list of blocked versions. If not specified, there are no blocked versions.</param>
        /// <returns>True if the version is within the specified range and not in the list of blocked versions. False otherwise.</returns>
        public bool IsVersionWithinRange(string version, string minVersion = null, string maxVersion = null, List<string> blockedVersions = null)
        {
            // Try to parse the version string into a SemanticVersion object and ignores pre-releases that are not dotnet 8
            if (!SemanticVersion.TryParse(version, out var semanticVersion) ||
                (semanticVersion.IsPrerelease && (semanticVersion.Major != 8)))
            {
                return false;
            }

            // Check if the version is less than the minimum version (if specified)
            if (minVersion != null
                && SemanticVersion.TryParse(minVersion, out var minSemanticVersion)
                && semanticVersion < minSemanticVersion)
            {
                return false;
            }

            // Check if the version is greater than the maximum version (if specified)
            if (maxVersion != null
                && SemanticVersion.TryParse(maxVersion, out var maxSemanticVersion)
                && semanticVersion > maxSemanticVersion)
            {
                return false;
            }

            // Check if the version is in the list of blocked versions (if specified)
            if (blockedVersions != null)
            {
                foreach (var blockedVersion in blockedVersions)
                {
                    // Try to parse the exception version string into a SemanticVersion object
                    if (SemanticVersion.TryParse(blockedVersion.Trim(), out var blockedSemanticVersion)
                        && semanticVersion == blockedSemanticVersion)
                    {
                        // If the exception version matches the version, return false
                        Console.WriteLine($"Blocking version: {blockedVersion}");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}