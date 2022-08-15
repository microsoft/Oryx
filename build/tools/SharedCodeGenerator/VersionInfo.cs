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
                this.SemanticVersion = new SemVer.Version(this.DisplayVersion, loose: true);
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
    }
}
