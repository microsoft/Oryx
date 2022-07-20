// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Oryx.SharedCodeGenerator
{
    public class VersionInfo : IComparable<VersionInfo>
    {
        private readonly List<string> nonSemverPlatforms = new List<string> { "golang" };

        public VersionInfo(string displayVersion, string platform)
        {
            this.DisplayVersion = displayVersion;
            this.Platform = platform;
            if (this.nonSemverPlatforms.Contains(platform))
            {
                this.Version = new Version(this.DisplayVersion);
            }
            else
            {
                this.SemanticVersion = ToSemanticVersion(this.DisplayVersion);
            }
        }

        public string DisplayVersion { get; }

        public string Platform { get; }

        public SemVer.Version SemanticVersion { get; }

        public Version Version { get; }

        public int CompareTo(VersionInfo other) =>
            this.nonSemverPlatforms.Contains(this.Platform)
            ? this.Version.CompareTo(other.Version)
            : this.SemanticVersion.CompareTo(other.SemanticVersion);

        private static SemVer.Version ToSemanticVersion(string displayVersion)
        {
            var semanticVersionStr = displayVersion;

            // The display version is in preview version format or it's a non-preview version
            // Both cases can be handled by SemVer library.
            // Throws ArgumentException if SemVer library found invalid version format.
            if (displayVersion.Contains('-') || !displayVersion.Any(c => char.IsLetter(c)))
            {
                if (Version.TryParse(semanticVersionStr, out var dotnetVersion))
                {
                    return new SemVer.Version(dotnetVersion.ToString(3));
                }

                return new SemVer.Version(semanticVersionStr);
            }

            // The display version is an invalid preview version
            var index = displayVersion.Length;
            index = displayVersion.ToList().FindIndex(c => char.IsLetter(c));
            semanticVersionStr = displayVersion.Insert(index, "-");
            return new SemVer.Version(semanticVersionStr, loose: true);
        }
    }
}
