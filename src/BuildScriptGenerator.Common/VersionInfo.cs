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
        public string displayVersion { get; }
        public SemVer.Version semanticVersion { get;  }

        public VersionInfo(string displayVersion)
        {
            this.displayVersion = displayVersion;
            this.semanticVersion = this.ToSemanticVersion(this.displayVersion);
        }

        private SemVer.Version ToSemanticVersion(string displayVersion)
        {
            string semanticVersionStr = displayVersion;

            if (!displayVersion.Contains('-') && !Regex.IsMatch(displayVersion, @"^\d+\.\d+\.\d+$"))
            {
                int index = displayVersion.Length;
                index = Regex.Match(displayVersion, @"[^\d\.]").Index;
                semanticVersionStr = displayVersion.Insert(index, "-");
            }

            return new SemVer.Version(semanticVersionStr);
        }

        public int CompareTo(VersionInfo other)
        {
            return this.semanticVersion.CompareTo(other.semanticVersion);
        }
    }
}
