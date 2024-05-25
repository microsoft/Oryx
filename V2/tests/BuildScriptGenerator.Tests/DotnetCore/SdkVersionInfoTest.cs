// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.DotNetCore
{
    public class SdkVersionInfoTest
    {
        [Theory]
        [InlineData("3.1")]
        [InlineData("3.1.*")]
        [InlineData("3.1.102.23")]
        [InlineData("3.1.102.preview1")]
        public void SdkVersionInfo_TryParseReturnsFalse_ForInvalidVersionFormat(string version)
        {
            // Arrange & Act
            var isValid = SdkVersionInfo.TryParse(version, out var result);

            // Assert
            Assert.False(isValid);
            Assert.Null(result);
        }

        [Theory]
        [InlineData("3.1.0", 3, 1, 0, 0)]
        [InlineData("3.1.102", 3, 1, 1, 2)]
        [InlineData("3.1.122", 3, 1, 1, 22)]
        [InlineData("3.1.222", 3, 1, 2, 22)]
        public void SdkVersionInfo_ParsesVersionAsExpected(
            string rawString,
            int expectedMajor,
            int expectedMinor,
            int expectedFeature,
            int expectedPatch)
        {
            // Arrange & Act
            var actual = SdkVersionInfo.Parse(rawString);

            // Assert
            Assert.Equal(expectedMajor, actual.Major);
            Assert.Equal(expectedMinor, actual.Minor);
            Assert.Equal(expectedFeature, actual.Feature);
            Assert.Equal(expectedPatch, actual.Patch);
            Assert.Equal(rawString, actual.RawString);
            Assert.False(actual.IsPrerelease);
            Assert.Null(actual.PrereleaseVersion);
        }

        [Theory]
        [InlineData("3.1.200-preview1-014995", 3, 1, 2, 0, "preview1-014995")]
        [InlineData("5.0.100-preview.1.20155.7", 5, 0, 1, 0, "preview.1.20155.7")]
        [InlineData("5.0.100-rc1.20155.7", 5, 0, 1, 0, "rc1.20155.7")]
        public void SdkVersionInfo_ParsesPrereleaseVersionAsExpected(
            string rawString,
            int expectedMajor,
            int expectedMinor,
            int expectedFeature,
            int expectedPatch,
            string expectedPrerelease)
        {
            // Arrange & Act
            var actual = SdkVersionInfo.Parse(rawString);

            // Assert
            Assert.Equal(expectedMajor, actual.Major);
            Assert.Equal(expectedMinor, actual.Minor);
            Assert.Equal(expectedFeature, actual.Feature);
            Assert.Equal(expectedPatch, actual.Patch);
            Assert.Equal(rawString, actual.RawString);
            Assert.True(actual.IsPrerelease);
            Assert.Equal(expectedPrerelease, actual.PrereleaseVersion);
        }

        [Theory]
        [InlineData("3.1.200-preview1-014995", "3.1.200")]
        [InlineData("5.0.100-preview.1.20155.7", "5.0.100")]
        [InlineData("5.0.100-rc.1.20155.7", "5.0.100")]
        public void ComparingVersions_PrereleaseVersionsAreLesserThanNonPrereleaseOnes(
            string prereleaseVersion,
            string nonPrereleaseVersion)
        {
            // Arrange
            var expected = -1;
            var prereleaseVersionInfo = SdkVersionInfo.Parse(prereleaseVersion);
            var nonPrereleaseVersionInfo = SdkVersionInfo.Parse(nonPrereleaseVersion);

            // Act
            var actual = prereleaseVersionInfo.CompareTo(nonPrereleaseVersionInfo);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("3.1.200-preview1-014995", "3.1.200-preview1-014995", 0)]
        [InlineData("3.1.200-preview1-014995", "3.1.200-preview2-014995", -1)]
        [InlineData("3.1.200-preview1-014995", "3.1.200-preview1-014996", -1)]
        [InlineData("5.0.100-preview.1.20155.7", "5.0.100-preview.1.20155.7", 0)]
        [InlineData("5.0.100-preview.1.20155.7", "5.0.100-preview.2.20155.7", -1)]
        [InlineData("5.0.100-preview.1.20155.7", "5.0.100-preview.1.20155.8", -1)]
        [InlineData("5.0.100-preview.1.20155.7", "5.0.100-rc.1.20155.8", -1)]
        [InlineData("5.0.100-rc.1.20155.7", "5.0.100-rc.2.20155.8", -1)]
        public void PrereleaseVersions_AreCompared_UsingStringComparisionRules(
            string prereleaseVersion,
            string nonPrereleaseVersion,
            int expected)
        {
            // Arrange
            var prereleaseVersionInfo = SdkVersionInfo.Parse(prereleaseVersion);
            var nonPrereleaseVersionInfo = SdkVersionInfo.Parse(nonPrereleaseVersion);

            // Act
            var actual = prereleaseVersionInfo.CompareTo(nonPrereleaseVersionInfo);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("3.1.200-preview1-014995", "3.1.200-PreView1-014995", 0)]
        [InlineData("3.1.200-preview1-014995", "3.1.200-PreView2-014995", -1)]
        [InlineData("3.1.200-preview1-014995", "3.1.200-PreView1-014996", -1)]
        [InlineData("5.0.100-preview.1.20155.7", "5.0.100-PreView.1.20155.7", 0)]
        [InlineData("5.0.100-preview.1.20155.7", "5.0.100-PreView.2.20155.7", -1)]
        [InlineData("5.0.100-preview.1.20155.7", "5.0.100-PreView.1.20155.8", -1)]
        public void PrereleaseVersions_AreCompared_CaseInsensitively(
            string prereleaseVersion,
            string nonPrereleaseVersion,
            int expected)
        {
            // Arrange
            var prereleaseVersionInfo = SdkVersionInfo.Parse(prereleaseVersion);
            var nonPrereleaseVersionInfo = SdkVersionInfo.Parse(nonPrereleaseVersion);

            // Act
            var actual = prereleaseVersionInfo.CompareTo(nonPrereleaseVersionInfo);

            // Assert
            Assert.Equal(expected, actual);
        }

        public static TheoryData<IEnumerable<string>, IEnumerable<string>> SortedVersionsData
        {
            get
            {
                var data = new TheoryData<IEnumerable<string>, IEnumerable<string>>();
                data.Add(new[] { "4.1.100", "3.1.100" }, new[] { "3.1.100", "4.1.100" });
                data.Add(new[] { "3.2.100", "3.1.100" }, new[] { "3.1.100", "3.2.100" });
                data.Add(new[] { "3.1.200", "3.1.100" }, new[] { "3.1.100", "3.1.200" });
                data.Add(new[] { "3.1.102", "3.1.100" }, new[] { "3.1.100", "3.1.102" });
                data.Add(new[] { "3.1.100", "3.1.100-preview1-1000" }, new[] { "3.1.100-preview1-1000", "3.1.100" });
                data.Add(new[]
                {
                    "2.2.401",
                    "2.1.803",
                    "2.1.801",
                    "3.1.100",
                    "2.1.802",
                    "3.1.200",
                    "3.1.101",
                    "3.1.100-preview1-014459",
                    "5.0.100-preview.1.20155.7",
                    "3.1.100-preview3-014645",
                    "3.1.100-preview2-014569",
                    "3.1.201",
                },
                new[]
                {
                    "2.1.801",
                    "2.1.802",
                    "2.1.803",
                    "2.2.401",
                    "3.1.100-preview1-014459",
                    "3.1.100-preview2-014569",
                    "3.1.100-preview3-014645",
                    "3.1.100",
                    "3.1.101",
                    "3.1.200",
                    "3.1.201",
                    "5.0.100-preview.1.20155.7",
                });
                return data;
            }
        }

        [Theory]
        [MemberData(nameof(SortedVersionsData))]
        public void VersionsAreSorted_AsPerComparisonRules(
            IEnumerable<string> inputVersions,
            IEnumerable<string> expectedSortedVersions)
        {
            // Arrange
            var originalList = inputVersions.Select(sdk => SdkVersionInfo.Parse(sdk));

            // Act
            var actual = originalList.OrderBy(sdk => sdk);

            // Assert
            var actualSortedVersions = actual.Select(sdk => sdk.RawString);
            Assert.Equal(expectedSortedVersions, actualSortedVersions, StringComparer.OrdinalIgnoreCase);
        }
    }
}
