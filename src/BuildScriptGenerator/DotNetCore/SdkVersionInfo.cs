// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    public class SdkVersionInfo : IComparable<SdkVersionInfo>, IEquatable<SdkVersionInfo>
    {
        public int Major { get; set; }

        public int Minor { get; set; }

        public int Feature { get; set; }

        public int Patch { get; set; }

        public int FeaturePatch { get; set; }

        public string RawString { get; set; }

        public bool IsPrerelease { get; set; }

        public string PreviewVersion { get; set; }

        public static SdkVersionInfo Parse(string sdkVersion)
        {
            TryParse(sdkVersion, out var result);
            return result;
        }

        public static bool TryParse(string sdkVersion, out SdkVersionInfo result)
        {
            result = null;

            var originalString = sdkVersion;

            // Example: 3.1.100-preview1
            var isPrerelease = false;
            string previewVersion = null;
            var index = sdkVersion.IndexOf("-preview", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                isPrerelease = true;
                previewVersion = sdkVersion.Substring(index + 1, sdkVersion.Length - (index + 1));
                sdkVersion = sdkVersion.Substring(0, index);
            }

            // Example: 3.1.102
            var sdkVersionParts = sdkVersion.Split(".");
            if (sdkVersionParts.Length != 3)
            {
                return false;
            }

            try
            {
                var major = int.Parse(sdkVersionParts[0]);
                var minor = int.Parse(sdkVersionParts[1]);
                var featurePatch = int.Parse(sdkVersionParts[2]);
                var feature = featurePatch / 100;
                var patch = featurePatch % 100;

                result = new SdkVersionInfo
                {
                    Major = major,
                    Minor = minor,
                    FeaturePatch = featurePatch,
                    Feature = feature,
                    Patch = patch,
                    RawString = originalString,
                    IsPrerelease = isPrerelease,
                    PreviewVersion = previewVersion,
                };

                return true;
            }
            catch
            {
                return false;
            }
        }

        public int CompareTo(SdkVersionInfo other)
        {
            var majorComparison = Major.CompareTo(other.Major);
            if (majorComparison != 0)
            {
                return majorComparison;
            }

            var minorComparison = Minor.CompareTo(other.Minor);
            if (minorComparison != 0)
            {
                return minorComparison;
            }

            var featureComparison = Feature.CompareTo(other.Feature);
            if (featureComparison != 0)
            {
                return featureComparison;
            }

            var patchComparison = Patch.CompareTo(other.Patch);
            if (patchComparison != 0)
            {
                return patchComparison;
            }

            // 3.1.100-preview1-01445 is lesser than 3.1.100
            if (IsPrerelease && !other.IsPrerelease)
            {
                return -1;
            }

            if (!IsPrerelease && other.IsPrerelease)
            {
                return 1;
            }

            if (IsPrerelease && other.IsPrerelease)
            {
                // We are not parsing the preview part here and are only using string comparison
                // One more thing to complicate thing is that the preview part format has changed between
                // 3.* and 5.*, so this comparison is just fine.
                return string.Compare(PreviewVersion, other.PreviewVersion, ignoreCase: true);
            }

            return 0;
        }

        public bool Equals(SdkVersionInfo other)
        {
            return CompareTo(other) == 0;
        }
    }
}
