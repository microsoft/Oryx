// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.DotNetCore
{
    public class GlobalJsonSdkResolverTest
    {
        [Theory]
        [InlineData("3.1")]
        [InlineData("3.1.*")]
        [InlineData("3.1.102.23")]
        [InlineData("3.1.102.preview1")]
        [InlineData("3.1.102-pre1")]
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
            Assert.Null(actual.PreviewVersion);
        }

        [Theory]
        [InlineData("3.1.200-preview1-014995", 3, 1, 2, 0, "preview1-014995")]
        [InlineData("5.0.100-preview.1.20155.7", 5, 0, 1, 0, "preview.1.20155.7")]
        public void SdkVersionInfo_ParsesPreviewVersionAsExpected(
            string rawString,
            int expectedMajor,
            int expectedMinor,
            int expectedFeature,
            int expectedPatch,
            string expectedPreview)
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
            Assert.Equal(expectedPreview, actual.PreviewVersion);
        }

        [Theory]
        [InlineData("3.1.200-preview1-014995", "3.1.200")]
        [InlineData("5.0.100-preview.1.20155.7", "5.0.100")]
        public void ComparingVersions_PreviewVersionsAreLesserThanNonPreviewOnes(
            string previewVersion,
            string nonPreviewVersion)
        {
            // Arrange
            var expected = -1;
            var previewVersionInfo = SdkVersionInfo.Parse(previewVersion);
            var nonPreviewVersionInfo = SdkVersionInfo.Parse(nonPreviewVersion);

            // Act
            var actual = previewVersionInfo.CompareTo(nonPreviewVersionInfo);

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
        public void PreviewVersions_AreCompared_UsingStringComparisionRules(
            string previewVersion,
            string nonPreviewVersion,
            int expected)
        {
            // Arrange
            var previewVersionInfo = SdkVersionInfo.Parse(previewVersion);
            var nonPreviewVersionInfo = SdkVersionInfo.Parse(nonPreviewVersion);

            // Act
            var actual = previewVersionInfo.CompareTo(nonPreviewVersionInfo);

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
        public void PreviewVersions_AreCompared_CaseInsensitively(
            string previewVersion,
            string nonPreviewVersion,
            int expected)
        {
            // Arrange
            var previewVersionInfo = SdkVersionInfo.Parse(previewVersion);
            var nonPreviewVersionInfo = SdkVersionInfo.Parse(nonPreviewVersion);

            // Act
            var actual = previewVersionInfo.CompareTo(nonPreviewVersionInfo);

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

        [Fact]
        public void Disable_ReturnsSameVersion_IfSameVersionIsFound()
        {
            // Arrange
            var expectedVersion = "3.1.100";
            var availableVersions = new[] { "2.1.100", "3.1.100", "3.1.101", "3.1.102-preview1-03444" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.Disable,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Theory]
        [InlineData("true")]
        [InlineData(null)]
        public void Disable_ReturnsSamePrereleaseVersion_IfSameVersionIsFound(string allowPrerelease)
        {
            // Arrange
            var expectedVersion = "3.1.102-preview1-03444";
            var availableVersions = new[] { "3.1.100", "3.1.101", "3.1.102-preview1-03444", "3.1.102-preview2-03444" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.102-preview1-03444",
                RollForward = GlobalJsonSdkResolver.Disable,
                AllowPreRelease = allowPrerelease,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void Disable_ReturnsNull_IfSameVersionIsNotFound()
        {
            // Arrange
            var availableVersions = new[] { "2.1.100", "3.1.101" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.Disable,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void Patch_ReturnsSameVersion_IfLatestPatchVersionsArePresent()
        {
            // Arrange
            var expectedVersion = "3.1.100";
            var availableVersions = new[] { "2.1.100", "3.1.100", "3.1.101" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.Patch,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void Patch_ReturnsLatestPatchVersion_IfSpecifiedPatchVersionDoesNotExist()
        {
            // Arrange
            var expectedVersion = "3.1.102";
            var availableVersions = new[] { "2.1.100", "3.1.101", "3.1.102", "3.2.103" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.Patch,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void Patch_ReturnsNull_IfSpecifiedPatchOrLatestPatchVersionDoesNotExist()
        {
            // Arrange
            var availableVersions = new[] { "2.1.100", "3.1.101", "3.1.102", "3.2.103" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.103",
                RollForward = GlobalJsonSdkResolver.Patch,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void Feature_ReturnsLatestPatchLevel_IfSameFeatureIsPresent()
        {
            // Arrange
            var expectedVersion = "3.1.101";
            var availableVersions = new[]
            {
                "2.1.100",
                "3.1.100",
                "3.1.101",
                "3.2.100",
                "3.2.101",
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.Feature,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void Feature_ReturnsNextHigherFeatureAndLatestPatchLevel_IfSpecifiedFeatureIsNotPresent()
        {
            // Arrange
            var expectedVersion = "3.1.201";
            var availableVersions = new[]
            {
                "2.1.100",
                "3.0.100",
                "3.0.101",
                "3.1.200",
                "3.1.201",
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.Feature,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void Feature_ReturnsNull_IfNextHigherFeatureAndLatestPatchLevelIsNotPresent()
        {
            // Arrange
            var availableVersions = new[]
            {
                "2.1.100",
                "3.0.100",
                "3.0.101",
                "3.1.200",
                "3.1.201",
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.0.102",
                RollForward = GlobalJsonSdkResolver.Feature,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void Minor_ReturnsLatestPatchLevel_IfSpecifiedMinorAndFeatureArePresent()
        {
            // Arrange
            var expectedVersion = "3.1.101";
            var availableVersions = new[] { "2.1.100", "3.1.100", "3.1.101" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.Minor,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void Minor_ReturnsNextHigherFeatureAndLatestPatchLevel_IfSpecifiedMinorIsPresentAndNotFeature()
        {
            // Arrange
            var expectedVersion = "3.1.201";
            var availableVersions = new[]
            {
                "2.1.100",
                "3.1.200",
                "3.1.201"
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.Minor,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void Minor_ReturnsNextMinorAndNextHigherFeatureAndLatestPatchLevel_IfSpecifiedMinorIsNotPresent()
        {
            // Arrange
            var expectedVersion = "3.2.101";
            var availableVersions = new[]
            {
                "2.1.100",
                "3.0.100",
                "3.0.101",
                "3.2.100",
                "3.2.101",
                "3.2.200",
                "3.2.201",
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.Minor,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void Minor_ReturnsNull_IfNextMinorAndNextHigherFeatureAndLatestPatchLevelIsNotPresent()
        {
            // Arrange
            var availableVersions = new[]
            {
                "2.1.100",
                "3.0.100",
                "3.0.101",
                "3.2.100",
                "3.2.101",
                "3.2.200",
                "3.2.201",
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.2.202",
                RollForward = GlobalJsonSdkResolver.Minor,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void Major_ReturnsLatestPatchVersion_ifSpecifiedMajorMinorAndFeatureArePresent()
        {
            // Arrange
            var expectedVersion = "3.1.101";
            var availableVersions = new[] { "2.1.100", "3.1.100", "3.1.101" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.Major,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void Major_ReturnsNextHigherFeatureAndNextHigherPatchVersion_ifSpecifiedMajorMinorArePresentButNotFeature()
        {
            // Arrange
            var expectedVersion = "3.1.201";
            var availableVersions = new[]
            {
                "2.1.100",
                "3.1.200",
                "3.1.201"
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.Major,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void Major_ReturnsNextMinorAndNextHigherFeatureAndLatestPatchLevel_IfSpecifiedMajorIsPresentButNotMinorAndFeature()
        {
            // Arrange
            var expectedVersion = "3.2.101";
            var availableVersions = new[]
            {
                "2.1.100",
                "3.0.100",
                "3.0.101",
                "3.2.100",
                "3.2.101",
                "3.2.200",
                "3.2.201",
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.Major,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void Major_ReturnsNextMajorAndNextMinorAndNextHigherFeatureAndLatestPatchLevel_IfSpecifiedMajorIsNotPresent()
        {
            // Arrange
            var expectedVersion = "4.1.101";
            var availableVersions = new[]
            {
                "2.1.100",
                "4.1.100",
                "4.1.101",
                "4.2.200",
                "4.2.201",
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.Major,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void Major_ReturnsNull_IfNextMajorAndNextMinorAndNextHigherFeatureAndLatestPatchLevelAreNotPresent()
        {
            // Arrange
            var availableVersions = new[]
            {
                "2.1.100",
                "4.1.100",
                "4.1.101",
                "4.2.200",
                "4.2.201",
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "4.3.100",
                RollForward = GlobalJsonSdkResolver.Major,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void LatestPatch_ReturnsSameVersion_IfNoLatestPatchVersionIsAvailable()
        {
            // Arrange
            var expectedVersion = "3.1.100";
            var availableVersions = new[] { "2.1.100", "3.1.100" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.LatestPatch,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void LatestPatch_ReturnsSamePatchVersion_IfLatestPatchVersionsWithinSameFeatureAreNotAvailable()
        {
            // Arrange
            var expectedVersion = "3.1.100";
            var availableVersions = new[] { "2.1.100", "3.1.100", "3.1.200", "3.1.201" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.LatestPatch,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void LatestPatch_ReturnsLatestPatchVersion_IfLatestPatchVersionsWithinSameFeatureAreAvailable()
        {
            // Arrange
            var expectedVersion = "3.1.103";
            var availableVersions = new[] { "2.1.100", "3.1.101", "3.1.102", "3.1.103", "3.2.104" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.LatestPatch,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Theory]
        [InlineData("true")]
        [InlineData(null)]
        public void LatestPatch_ReturnsLatestPreviewVersion_IfAllowPrereleaseIsTrue(string allowPrerelease)
        {
            // Arrange
            var expectedVersion = "3.1.100-preview3-01445";
            var availableVersions = new[]
            {
                "2.1.100",
                "3.1.100-preview1-01445",
                "3.1.100-preview2-01445",
                "3.1.100-preview3-01445",
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100-preview1-01445",
                RollForward = GlobalJsonSdkResolver.LatestPatch,
                AllowPreRelease = allowPrerelease,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void LatestPatch_ReturnsReleasedVersionOverPreviewVersion_IfReleasedVersionIsAvailableAndAllowPrereleaseIsTrue()
        {
            // Arrange
            var expectedVersion = "3.1.100";
            var availableVersions = new[]
            {
                "2.1.100",
                "3.1.100-preview1-01445",
                "3.1.100-preview2-01445",
                "3.1.100-preview3-01445",
                "3.1.100"
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100-preview1-01445",
                RollForward = GlobalJsonSdkResolver.LatestPatch,
                AllowPreRelease = "true",
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void RollForward_ReturnsLatestPatchAsDefault_IfRollForwardIsNotPresent()
        {
            // Arrange
            var expectedVersion = "3.1.101";
            var availableVersions = new[] { "2.1.100", "3.1.100", "3.1.101" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void LatestFeature_ReturnsSameVersion_IfNoLatestFeatureVersionIsAvailable()
        {
            // Arrange
            var expectedVersion = "3.1.100";
            var availableVersions = new[] { "2.1.100", "3.1.100" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.LatestFeature,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void LatestFeature_ReturnsSameFeatureAndLatestPatchVersionIfAvailable()
        {
            // Arrange
            var expectedVersion = "3.1.102";
            var availableVersions = new[] { "2.1.100", "3.1.100", "3.1.101", "3.1.102" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.LatestFeature,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void LatestFeature_ReturnsLatestFeatureAndLatestPatchVersion_IfAvailable()
        {
            // Arrange
            var expectedVersion = "3.1.202";
            var availableVersions = new[]
            {
                "2.1.100",
                "3.1.100",
                "3.1.101",
                "3.1.102",
                "3.1.200",
                "3.1.202"
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.LatestFeature,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void LatestFeature_ReturnsReleasedVersionOverPreviewVersion_IfReleasedVersionIsAvailableAndAllowPrereleaseIsTrue()
        {
            // Arrange
            var expectedVersion = "3.1.202";
            var availableVersions = new[]
            {
                "2.1.100",
                "3.1.100",
                "3.1.101",
                "3.1.102",
                "3.1.200",
                "3.1.202-preview1-01334",
                "3.1.202-preview2-01554",
                "3.1.202",
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.LatestFeature,
                AllowPreRelease = "true",
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void LatestFeature_ReturnsLatestFeatureAndLatestPatchVersionOfPreviewRelease_IfAllowPrereleaseIsSetToTrue()
        {
            // Arrange
            var expectedVersion = "3.1.202-preview2-01554";
            var availableVersions = new[]
            {
                "2.1.100",
                "3.1.100",
                "3.1.101",
                "3.1.102",
                "3.1.200",
                "3.1.202-preview1-01334",
                "3.1.202-preview2-01554",
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.LatestFeature,
                AllowPreRelease = "true",
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void LatestMinor_ReturnsSameVersion_IfNoLatestFeatureVersionIsAvailable()
        {
            // Arrange
            var expectedVersion = "3.1.100";
            var availableVersions = new[] { "2.1.100", "3.1.100" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.LatestMinor,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void LatestMinor_ReturnsSameMinorAndLatestFeaturePatchVersion_IfAvailable()
        {
            // Arrange
            var expectedVersion = "3.1.202";
            var availableVersions = new[] { "2.1.100", "3.1.100", "3.1.101", "3.1.102", "3.1.200", "3.1.202" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.LatestMinor,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void LatestMinor_ReturnsLatestMinorAndLatestFeaturePatchVersion_IfAvailable()
        {
            // Arrange
            var expectedVersion = "3.2.201";
            var availableVersions = new[]
            {
                "2.1.100",
                "3.1.100",
                "3.1.101",
                "3.2.100",
                "3.2.102",
                "3.2.200",
                "3.2.201"
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.LatestMinor,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void LatestMajor_ReturnsSameVersion_IfNoLatestMajorVersionIsAvailable()
        {
            // Arrange
            var expectedVersion = "3.1.100";
            var availableVersions = new[] { "2.1.100", "3.1.100" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.LatestMajor,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void LatestMajor_ReturnsSameMajorAndLatestMinorFeaturePatchVersion_IfAvailable()
        {
            // Arrange
            var expectedVersion = "3.3.101";
            var availableVersions = new[]
            {
                "2.1.100",
                "3.1.100",
                "3.1.101",
                "3.2.101",
                "3.2.102",
                "3.2.200",
                "3.2.201",
                "3.3.100",
                "3.3.101",
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.LatestMajor,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void LatestMajor_ReturnsLatestMajorAndLatestMinorFeaturePatchVersion_IfAvailable()
        {
            // Arrange
            var expectedVersion = "4.3.102";
            var availableVersions = new[]
            {
                "2.1.100",
                "3.1.100",
                "3.1.101",
                "3.2.100",
                "3.2.101",
                "3.2.102",
                "4.1.101",
                "4.2.100",
                "4.2.102",
                "4.3.101",
                "4.3.102",
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.LatestMajor,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Theory]
        [InlineData("true")]
        [InlineData(null)]
        public void LatestMajor_ReturnsLatestPreview_IfAvailable(string allowPrerelease)
        {
            // Arrange
            var expectedVersion = "4.2.100-preview3-01322";
            var availableVersions = new[]
            {
                "2.1.100",
                "3.1.100",
                "3.1.101",
                "4.2.100-preview1-01453",
                "4.2.100-preview2-01975",
                "4.2.100-preview3-01322",
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.LatestMajor,
                AllowPreRelease = allowPrerelease,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void LatestMajor_DoesNotReturnLatestPreview_IfAllowPrereleaseIsFalse()
        {
            // Arrange
            var expectedVersion = "4.1.101";
            var availableVersions = new[]
            {
                "2.1.100",
                "3.1.100",
                "3.1.101",
                "4.1.101",
                "5.0.100-preview1-01453",
                "5.0.100-preview2-01975",
                "5.0.100-preview3-01322",
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new Sdk
            {
                Version = "3.1.100",
                RollForward = GlobalJsonSdkResolver.LatestMajor,
                AllowPreRelease = "false",
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void NoSdkInGlobalJson_ReturnsLatestMajorAndLatestMinorFeaturePatchVersion_IfAvailable()
        {
            // Arrange
            var expectedVersion = "4.3.102";
            var availableVersions = new[]
            {
                "2.1.100",
                "3.1.100",
                "3.1.101",
                "3.2.100",
                "3.2.101",
                "3.2.102",
                "4.1.101",
                "4.2.100",
                "4.2.102",
                "4.3.101",
                "4.3.102",
            };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = null; // no sdk was specified by end user
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void NoGlobalJson_ReturnsLatestMajorAndLatestMinorFeaturePatchVersion_IfAvailable()
        {
            // Arrange
            var expectedVersion = "4.3.102";
            var availableVersions = new[]
            {
                "2.1.100",
                "3.1.100",
                "3.1.101",
                "3.2.100",
                "3.2.101",
                "3.2.102",
                "4.1.101",
                "4.2.100",
                "4.2.102",
                "4.3.101",
                "4.3.102",
            };
            GlobalJsonModel globalJson = null; // no global json provided by end user
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        private GlobalJsonSdkResolver GetGlobalJsonHelper()
        {
            return new GlobalJsonSdkResolver(NullLogger<GlobalJsonSdkResolver>.Instance);
        }
    }
}
