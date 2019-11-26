// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using SemVer;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public static class SemanticVersionResolver
    {
        public static readonly Range NoRangeMatch = new Range("<0.0.0");

        public static string GetMaxSatisfyingVersion(string providedVersion, IEnumerable<string> supportedVersions)
        {
            try
            {
                var range = new Range(providedVersion);
                var satisfying = range.MaxSatisfying(supportedVersions);
                return satisfying;
            }
            catch
            {
            }

            return null;
        }

        public static int CompareVersions(string providedVersion, string supportedVersion)
        {
            providedVersion = SanitizeVersion(providedVersion);
            supportedVersion = SanitizeVersion(supportedVersion);

            var v1 = new SemVer.Version(providedVersion);
            var v2 = new SemVer.Version(supportedVersion);

            return v1.CompareTo(v2);
        }

        public static SemVer.Range GetMatchingRange(string providedVersion, IEnumerable<string> supportedVersions)
        {
            var result = NoRangeMatch;

            var providedVersionRange = new SemVer.Range(providedVersion);
            foreach (var supportedVersion in supportedVersions)
            {
                var supportedVersionRange = new SemVer.Range(supportedVersion);
                var matchingRange = supportedVersionRange.Intersect(providedVersionRange);
                if (!matchingRange.Equals(SemanticVersionResolver.NoRangeMatch))
                {
                    result = matchingRange;
                    break;
                }
            }

            return result;
        }

        private static string SanitizeVersion(string version)
        {
            bool invalidVersion = false;
            var parts = version.Split('.');
            if (parts.Length < 1 || parts.Length > 3)
            {
                invalidVersion = true;
            }
            else
            {
                foreach (var part in parts)
                {
                    if (!int.TryParse(part, out var number))
                    {
                        invalidVersion = true;
                        break;
                    }
                }
            }

            if (invalidVersion)
            {
                throw new System.InvalidOperationException($"Invalid version '{version}' specified.");
            }

            if (parts.Length == 1)
            {
                return $"{parts[0]}.0.0";
            }
            else if (parts.Length == 2)
            {
                return $"{parts[0]}.{parts[1]}.0";
            }
            else
            {
                return string.Join(".", parts);
            }
        }
    }
}
