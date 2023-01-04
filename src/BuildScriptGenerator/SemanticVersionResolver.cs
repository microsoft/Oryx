// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    using System.Collections.Generic;
    using System.Linq;
    using SemanticVersioning;
    using Version = System.Version;

    internal static class SemanticVersionResolver
    {
        /// <summary>
        /// Gets the latest version from an enumeration that is less than or equal to the provided version.
        /// </summary>
        /// <param name="rangeSpec">The provided range specification to match against. For example, "3.9.0", ">=3.9", "~3.0.0".</param>
        /// <param name="supportedVersions">The enumeration of versions to search through.</param>
        /// <returns>The latest version less than or equal to the provided version.</returns>
        public static string GetMaxSatisfyingVersion(string rangeSpec, IEnumerable<string> supportedVersions)
        {
            try
            {
                var preparedVersions = FormatVersions(supportedVersions);
                var range = new Range(rangeSpec);
                var satisfying = range.MaxSatisfying(preparedVersions.Select(v => v.FormattedVersion));
                if (!string.IsNullOrEmpty(satisfying))
                {
                    satisfying = preparedVersions.Where(v => v.FormattedVersion == satisfying)
                                                 .Select(v => v.OriginalVersion)
                                                 .FirstOrDefault();
                }

                return satisfying;
            }
            catch
            {
            }

            return null;
        }

        public static int CompareVersions(string providedVersion, string supportedVersion)
        {
            try
            {
                var v1 = Version.Parse(providedVersion);
                var v2 = Version.Parse(supportedVersion);

                return v1.CompareTo(v2);
            }
            catch
            {
            }

            return int.MinValue;
        }

        /// <summary>
        /// Returns whether or not the provided version is valid.
        /// Note: We first format the version by appending ".0" at most twice to ensure it has at least a patch version.
        /// </summary>
        /// <param name="version">The version to check.</param>
        /// <returns>True if the provided version is valid, false otherwise.</returns>
        public static bool IsValidVersion(string version)
        {
            var versionPair = FormatVersion(version);
            return Version.TryParse(versionPair.FormattedVersion, out _);
        }

        /// <summary>
        /// Since the SemanticVersioning library we're using is very strict about versions being in the form of at least
        /// Major.Minor.Patch, we need to format the versions in case they don't follow this model.
        /// For example, 16 => 16.0.0, 10.12 => 10.12.0.
        /// </summary>
        /// <param name="versions">The enumeration of versions to format.</param>
        /// <returns>An enumeration of version pairs with the first being the original version and the second
        /// being the formatted version.</returns>
        private static IEnumerable<(string OriginalVersion, string FormattedVersion)> FormatVersions(IEnumerable<string> versions)
        {
            if (versions == null || !versions.Any())
            {
                return Enumerable.Empty<(string, string)>();
            }

            return versions.Select(v => FormatVersion(v));
        }

        /// <summary>
        /// Since the SemanticVersioning library we're using is very strict about versions being in the form of at least
        /// Major.Minor.Patch, we need to format the version in case it doesn't follow this model.
        /// For example, 16 => 16.0.0, 10.12 => 10.12.0.
        /// </summary>
        /// <param name="version">The versions to format.</param>
        /// <returns>A version pair with the first item being the original version and the second item being the formatted version.</returns>
        private static (string OriginalVersion, string FormattedVersion) FormatVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return (string.Empty, string.Empty);
            }

            var formattedVersion = version;
            var segments = version.Split('.');
            for (int i = 0; i < 3 - segments.Length; i++)
            {
                formattedVersion += ".0";
            }

            return (version, formattedVersion);
        }
    }
}
