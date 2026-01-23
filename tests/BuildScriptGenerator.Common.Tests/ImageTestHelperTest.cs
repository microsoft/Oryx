// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildScriptGenerator.Common.Tests
{
    public class ImageTestHelperTest
    {
        private const string _imageBaseEnvironmentVariable = ImageTestHelperConstants.RepoPrefixEnvironmentVariable;
        private const string _tagSuffixEnvironmentVariable = ImageTestHelperConstants.TagSuffixEnvironmentVariable;
        private const string _defaultImageBase = ImageTestHelperConstants.DefaultRepoPrefix;

        private const string _buildRepository = ImageTestHelperConstants.BuildRepository;
        private const string _packRepository = ImageTestHelperConstants.PackRepository;
        private const string _latestTag = ImageTestHelperConstants.LatestStretchTag;
        private const string _ltsVersionsTag = ImageTestHelperConstants.LtsVersionsStretch;

        private readonly ITestOutputHelper _output;

        public ImageTestHelperTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GetTestRuntimeImage_Validate_ImageBaseSet()
        {
            // Arrange
            var platformName = "test";
            var platformVersion = "1.0";
            var osType = ImageTestHelperConstants.OsTypeDebianBookworm;
            var imageBaseValue = "oryxtest";
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var runtimeImage = imageHelper.GetRuntimeImage(platformName, platformVersion, osType);

            // Assert
            var expectedImage = $"{imageBaseValue}/{platformName}:{platformVersion}-{osType}";
            Assert.Equal(expectedImage, runtimeImage);
        }

        [Fact]
        public void GetTestRuntimeImage_Validate_TagSuffixSet()
        {
            // Arrange
            var platformName = "test";
            var platformVersion = "1.0";
            var osType = ImageTestHelperConstants.OsTypeDebianBookworm;
            var imageBaseValue = string.Empty;
            var tagSuffixValue = "-buildNumber";
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var runtimeImage = imageHelper.GetRuntimeImage(platformName, platformVersion, osType);

            // Assert
            var expectedImage = $"{_defaultImageBase}/{platformName}:{platformVersion}-{osType}{tagSuffixValue}";
            Assert.Equal(expectedImage, runtimeImage);
        }

        [Fact]
        public void GetTestRuntimeImage_Validate_NoImageBaseOrTagSuffixSet()
        {
            // Arrange
            var platformName = "test";
            var platformVersion = "1.0";
            var osType = ImageTestHelperConstants.OsTypeDebianBookworm;
            var imageBaseValue = string.Empty;
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var runtimeImage = imageHelper.GetRuntimeImage(platformName, platformVersion, osType);

            // Assert
            var expectedImage = $"{_defaultImageBase}/{platformName}:{platformVersion}-{osType}";
            Assert.Equal(expectedImage, runtimeImage);
        }

        [Fact]
        public void GetTestRuntimeImage_Validate_BothImageBaseAndTagSuffixSet()
        {
            // Arrange
            var platformName = "test";
            var platformVersion = "1.0";
            var osType = ImageTestHelperConstants.OsTypeDebianBookworm;
            var imageBaseValue = "oryxtest";
            var tagSuffixValue = "-buildNumber";
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var runtimeImage = imageHelper.GetRuntimeImage(platformName, platformVersion, osType);

            // Assert
            var expectedImage = $"{imageBaseValue}/{platformName}:{platformVersion}-{osType}{tagSuffixValue}";
            Assert.Equal(expectedImage, runtimeImage);
        }

        [Fact]
        public void GetTestBuildImage_Validate_ImageBaseSet()
        {
            // Arrange
            var imageBaseValue = "oryxtest";
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var buildImage = imageHelper.GetBuildImage();

            // Assert
            var expectedImage = $"{imageBaseValue}/{_buildRepository}:{_latestTag}";
            Assert.Equal(expectedImage, buildImage);
        }

        [Fact]
        public void GetTestBuildImage_Validate_TagSuffixSet()
        {
            // Arrange
            var imageBaseValue = string.Empty;
            var tagSuffixValue = "-buildNumber";
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var buildImage = imageHelper.GetBuildImage();

            // Assert
            var expectedImage = $"{_defaultImageBase}/{_buildRepository}:{_latestTag}{tagSuffixValue}";
            Assert.Equal(expectedImage, buildImage);
        }

        [Fact]
        public void GetLtsVersionsBuildImage_Validate_ImageBaseSet()
        {
            // Arrange
            var imageBaseValue = "oryxtest";
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var buildImage = imageHelper.GetLtsVersionsBuildImage();

            // Assert
            var expectedImage = $"{imageBaseValue}/{_buildRepository}:{_ltsVersionsTag}";
            Assert.Equal(expectedImage, buildImage);
        }

        [Fact]
        public void GetLtsVersionsBuildImage_Validate_TagSuffixSet()
        {
            // Arrange
            var imageBaseValue = string.Empty;
            var tagSuffixValue = "-buildNumber";
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var buildImage = imageHelper.GetLtsVersionsBuildImage();

            // Assert
            var expectedImage = $"{_defaultImageBase}/{_buildRepository}:{_ltsVersionsTag}{tagSuffixValue}";
            Assert.Equal(expectedImage, buildImage);
        }

        [Fact]
        public void GetTestPackImage_Validate_ImageBaseSet()
        {
            // Arrange
            var imageBaseValue = "oryxtest";
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var packImage = imageHelper.GetPackImage();

            // Assert
            var expectedImage = $"{imageBaseValue}/{_packRepository}:{_latestTag}";
            Assert.Equal(expectedImage, packImage);
        }

        [Fact]
        public void GetTestPackImage_Validate_TagSuffixSet()
        {
            // Arrange
            var imageBaseValue = string.Empty;
            var tagSuffixValue = "-buildNumber";
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var packImage = imageHelper.GetPackImage();

            // Assert
            var expectedImage = $"{_defaultImageBase}/{_packRepository}:{_latestTag}{tagSuffixValue}";
            Assert.Equal(expectedImage, packImage);
        }

        [Fact]
        public void GetTestBuildImage_Validate_LatestTag()
        {
            // Arrange
            var imageBaseValue = string.Empty;
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var buildImage = imageHelper.GetBuildImage(_latestTag);

            // Assert
            var expectedImage = $"{_defaultImageBase}/{_buildRepository}:{_latestTag}";
            Assert.Equal(expectedImage, buildImage);
        }

        [Fact]
        public void GetTestBuildImage_Validate_LatestVersionsTag()
        {
            // Arrange
            var imageBaseValue = string.Empty;
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var buildImage = imageHelper.GetBuildImage(_ltsVersionsTag);

            // Assert
            var expectedImage = $"{_defaultImageBase}/{_buildRepository}:{_ltsVersionsTag}";
            Assert.Equal(expectedImage, buildImage);
        }

        [Fact]
        public void GetTestBuildImage_Validate_InvalidTag()
        {
            // Arrange
            var imageBaseValue = string.Empty;
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Assert
            Assert.Throws<NotSupportedException>(() => { imageHelper.GetBuildImage("invalidTag"); });
        }

        [Fact]
        public void GetsGitHubActionsImageWithRestrictivePermissions()
        {
            // Arrange
            var imageHelper = ImageTestHelper.WithRestrictedPermissions();
            var expected = "oryxtests/build:github-actions-debian-bullseye";

            // Act
            var actual = imageHelper.GetGitHubActionsBuildImage();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetsFullBuildImageWithRestrictivePermissions()
        {
            // Arrange
            var imageHelper = ImageTestHelper.WithRestrictedPermissions();
            var expected = "oryxtests/build:debian-bullseye";

            // Act
            var actual = imageHelper.GetBuildImage();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetsLtsVersionsImageWithRestrictivePermissions()
        {
            // Arrange
            var imageHelper = ImageTestHelper.WithRestrictedPermissions();
            var expected = "oryxtests/build:lts-versions-debian-bullseye";

            // Act
            var actual = imageHelper.GetLtsVersionsBuildImage();

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
