// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.DotNetCore
{
    public class GlobalJsonSdkResolverTest
    {
        [Theory]
        [InlineData("version")]
        [InlineData("Version")]
        public void GlobalJsonDeserialization_IsCaseInsensitiveForVersionKey(string versionKey)
        {
            // Arrange
            var globalJsonTemplate = @"
            {
                ""sdk"": {
                    ""#version-key#"": ""3.1.100""
                }
            }";
            var expectedVersion = "3.1.101";
            var availableVersions = new[] { "2.1.100", "3.1.100", "3.1.101" };
            var runtimeVersion = "2.1.12";
            var globalJson = globalJsonTemplate.Replace("#version-key#", versionKey);
            var sourceRepo = new MemorySourceRepo();
            sourceRepo.AddFile(globalJson, DotNetCoreConstants.GlobalJsonFileName);
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(sourceRepo, runtimeVersion, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Theory]
        [InlineData("rollForward", "Feature")]
        [InlineData("rollforward", "feature")]
        public void GlobalJsonDeserialization_IsCaseInsensitiveForRollForwardKeyAndValue(
            string rollForwardKey,
            string rollForwardValue)
        {
            // Arrange
            var globalJsonTemplate = @"
            {
                ""sdk"": {
                    ""version"": ""3.1.100"",
                    ""#rollForward-key#"": ""#rollforward-value#"",
                }
            }";
            var expectedVersion = "3.1.202";
            var availableVersions = new[] { "2.1.100", "3.1.200", "3.1.202", "3.1.300" };
            var runtimeVersion = "2.1.12";
            var globalJson = globalJsonTemplate
                .Replace("#rollForward-key#", rollForwardKey)
                .Replace("#rollforward-value#", rollForwardValue);
            var sourceRepo = new MemorySourceRepo();
            sourceRepo.AddFile(globalJson, DotNetCoreConstants.GlobalJsonFileName);
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(sourceRepo, runtimeVersion, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Theory]
        [InlineData("AllowPrerelease")]
        [InlineData("allowPrerelease")]
        [InlineData("allowprerelease")]
        public void GlobalJsonDeserialization_IsCaseInsensitiveForAllowPrereleaseKey(string allowPrereleaseKey)
        {
            // Arrange
            var globalJsonTemplate = @"
            {
                ""sdk"": {
                    ""version"": ""3.1.100"",
                    ""#allowPrerelease-key#"": true,
                }
            }";
            var globalJson = globalJsonTemplate
                .Replace("#allowPrerelease-key#", allowPrereleaseKey);
            var sourceRepo = new MemorySourceRepo();
            sourceRepo.AddFile(globalJson, DotNetCoreConstants.GlobalJsonFileName);
            var expectedVersion = "3.1.102-preview2-03444";
            var availableVersions = new[] { "3.1.100", "3.1.101", "3.1.102-preview1-03444", "3.1.102-preview2-03444" };
            var runtimeVersion = "2.1.12";
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(sourceRepo, runtimeVersion, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Theory]
        [InlineData("3.0.3", "3.0.202")]
        [InlineData("2.1.16", "2.1.202")]
        [InlineData("5.1.0", "5.1.100-preview3-01445")]
        [InlineData("5.0.0", null)]
        public void GetSatisfyingSdkVersion_MustReturnLatestFeaturePatchOfRuntimeVersion_IfGlobalJsonFileIsNotFoundInTheRepo(
            string runtimeVersion,
            string expectedSdkVersion)
        {
            // Arrange
            var availableVersions = new[]
            {
                "2.1.100",
                "2.1.202",
                "2.3.200",
                "2.3.202",
                "3.0.202",
                "3.1.100",
                "3.1.101",
                "3.2.100",
                "3.2.102",
                "3.2.200",
                "3.2.201",
                "5.1.100-preview1-01445",
                "5.1.100-preview2-01445",
                "5.1.100-preview3-01445",
                "5.2.100-preview3-01445",
                "5.2.100-rc1-01445",
            };
            var sourceRepo = new MemorySourceRepo();
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(sourceRepo, runtimeVersion, availableVersions);

            // Assert
            Assert.Equal(expectedSdkVersion, actual);
        }

        [Fact]
        public void NewGlobalJsonModel_SetsAllowPrereleaseProperty_ToTrueByDefault()
        {
            // Arrange & Act
            var model = new GlobalJsonModel();
            model.Sdk = new SdkModel();

            // Assert
            Assert.True(model.Sdk.AllowPreRelease);
        }

        [Fact]
        public void NewSdk_SetsRollForwardPolicy_ToLatestPatchByDefault()
        {
            // Arrange & Act
            var model = new GlobalJsonModel();
            model.Sdk = new SdkModel();

            // Assert
            Assert.Equal(RollForwardPolicy.LatestPatch, model.Sdk.RollForward);
        }

        [Fact]
        public void Disable_ReturnsSameVersion_IfSameVersionIsFound()
        {
            // Arrange
            var expectedVersion = "3.1.100";
            var availableVersions = new[] { "2.1.100", "3.1.100", "3.1.101", "3.1.102-preview1-03444" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.Disable,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void Disable_ReturnsSamePrereleaseVersion_IfSameVersionIsFound()
        {
            // Arrange
            var expectedVersion = "3.1.102-preview1-03444";
            var availableVersions = new[] { "3.1.100", "3.1.101", "3.1.102-preview1-03444", "3.1.102-preview2-03444" };
            var globalJson = new GlobalJsonModel();
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.102-preview1-03444",
                RollForward = RollForwardPolicy.Disable,
                AllowPreRelease = true,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.Disable,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.Patch,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.Patch,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.103",
                RollForward = RollForwardPolicy.Patch,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.Feature,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.Feature,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.0.102",
                RollForward = RollForwardPolicy.Feature,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.Minor,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.Minor,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.Minor,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.2.202",
                RollForward = RollForwardPolicy.Minor,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.Major,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.Major,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.Major,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.Major,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "4.3.100",
                RollForward = RollForwardPolicy.Major,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.LatestPatch,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.LatestPatch,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.LatestPatch,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void LatestPatch_ReturnsLatestPreviewVersion_IfAllowPrereleaseIsTrue()
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100-preview1-01445",
                RollForward = RollForwardPolicy.LatestPatch,
                AllowPreRelease = true,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100-preview1-01445",
                RollForward = RollForwardPolicy.LatestPatch,
                AllowPreRelease = true,
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
            globalJson.Sdk = new SdkModel
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.LatestFeature,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.LatestFeature,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.LatestFeature,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.LatestFeature,
                AllowPreRelease = true,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.LatestFeature,
                AllowPreRelease = true,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.LatestMinor,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.LatestMinor,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.LatestMinor,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.LatestMajor,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.LatestMajor,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.LatestMajor,
            };
            var globalJsonHelper = GetGlobalJsonHelper();

            // Act
            var actual = globalJsonHelper.GetSatisfyingSdkVersion(globalJson, availableVersions);

            // Assert
            Assert.Equal(expectedVersion, actual);
        }

        [Fact]
        public void LatestMajor_ReturnsLatestPreview_IfAvailable()
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.LatestMajor,
                AllowPreRelease = true,
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
            globalJson.Sdk = new SdkModel
            {
                Version = "3.1.100",
                RollForward = RollForwardPolicy.LatestMajor,
                AllowPreRelease = false,
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
