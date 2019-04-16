// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using SemVer;

namespace Microsoft.Oryx.BuildScriptGenerator
{
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

        public static int CompareVersion(string providedVersion, string supportedVersion)
        {
            try
            {
                var v1 = new Version(providedVersion);
                var v2 = new Version(supportedVersion);
                return v1.CompareTo(v2);
            }
            catch
            {
            }

            return int.MinValue;
        }
    }
}
