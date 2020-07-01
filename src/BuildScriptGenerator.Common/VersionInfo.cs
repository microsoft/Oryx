// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;

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

            if (!displayVersion.Contains('-') && !Regex.IsMatch(displayVersion, @"^\d+\.\d+\.\d+$"))
            {
                var index = displayVersion.Length;
                index = Regex.Match(displayVersion, @"[^\d\.]").Index;
                semanticVersionStr = displayVersion.Insert(index, "-");
            }

            return new SemVer.Version(semanticVersionStr);
        }

        public int CompareTo(VersionInfo other)
        {
            return this.SemanticVersion.CompareTo(other.SemanticVersion);
        }
    }
}
