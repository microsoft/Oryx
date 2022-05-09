// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    using System.Collections.Generic;
    using System.Linq;
    using SemVer;
    using Version = System.Version;

    internal static class SemanticVersionResolver
    {
        /// <summary>
        /// Gets the latest version from an enumeration that is less than or equal to the provided version.
        /// </summary>
        /// <param name="rangeSpec">The provided range specification to match against. For example, "3.9.0", ">=3.9", "~3.0.0".</param>
        /// <param name="supportedVersions">The enumeration of versions to search through.</param>
        /// <param name="loose">Determines whether or not a looser RegEx is used when searching the versions.</param>
        /// <returns>The latest version less than or equal to the provided version.</returns>
        public static string GetMaxSatisfyingVersion(string rangeSpec, IEnumerable<string> supportedVersions, bool loose = false)
        {
            try
            {
                var preparedVersions = PrepareVersions(supportedVersions);
                var range = new Range(rangeSpec);
                var satisfying = range.MaxSatisfying(preparedVersions.Select(v => v.PreparedVersion), loose);
                if (!string.IsNullOrEmpty(satisfying))
                {
                    satisfying = preparedVersions.Where(v => v.PreparedVersion == satisfying)
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
        /// Since the SemanticVersioning library we're using is very strict about versions being in the form of at least
        /// Major.Minor.Patch, we need to prepare the versions in case they don't follow this model.
        /// For example, 16 => 16.0.0, 10.12 => 10.12.0.
        /// </summary>
        /// <param name="versions">The enumeration of versions to prepare.</param>
        /// <returns>An enumeration of version pairs with the first being the original version and the second
        /// being the prepared version.</returns>
        private static IEnumerable<(string OriginalVersion, string PreparedVersion)> PrepareVersions(IEnumerable<string> versions)
        {
            if (versions == null || !versions.Any())
            {
                return Enumerable.Empty<(string, string)>();
            }

            return versions.Select(v =>
            {
                var segments = v.Split('.');
                var preparedV = v;
                for (int i = 0; i < 3 - segments.Length; i++)
                {
                    preparedV += ".0";
                }

                return (v, preparedV);
            });
        }
    }
}
