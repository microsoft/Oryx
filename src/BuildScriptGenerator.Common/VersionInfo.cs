// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Linq;

namespace Microsoft.Oryx.BuildScriptGenerator.Common
{
    public class VersionInfo : IComparable<VersionInfo>
    {
        public string DisplayVersion { get; }

        public SemVer.Version SemanticVersion { get;  }

        public VersionInfo(string displayVersion)
        {
            this.DisplayVersion = displayVersion;
            this.SemanticVersion = this.ToSemanticVersion(this.DisplayVersion);
        }

        private SemVer.Version ToSemanticVersion(string displayVersion)
        {
            var semanticVersionStr = displayVersion;

            // The display version is in preview version format or it's a non-preview version
            // Both cases can be handled by SemVer library.
            // Throws ArgumentException if SemVer library found invalid version format.
            if (displayVersion.Contains('-') || !displayVersion.Any(c => char.IsLetter(c)))
            {
                return new SemVer.Version(semanticVersionStr);
            }

            // The display version is an invalid preview version
            var index = displayVersion.Length;
            index = displayVersion.ToList().FindIndex(c => char.IsLetter(c));
            semanticVersionStr = displayVersion.Insert(index, "-");
            return new SemVer.Version(semanticVersionStr);
        }

        public int CompareTo(VersionInfo other)
        {
            return this.SemanticVersion.CompareTo(other.SemanticVersion);
        }
    }
}
