// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    using System.Collections.Generic;
    using SemVer;
    using Version = System.Version;

    internal static class SemanticVersionResolver
    {
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
    }
}
