// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Common.Tests
{
    public class ImageTestHelperTest
    {
        private const string _imageBaseEnvironmentVariable = "ORYX_TEST_IMAGE_BASE";
        private const string _tagSuffixEnvironmentVariable = "ORYX_TEST_TAG_SUFFIX";
        private const string _defaultImageBase = "oryxdevmcr.azurecr.io/public/oryx";

        private const string _buildRepository = "build";
        private const string _packRepository = "pack";
        private const string _latestTag = "latest";
        private const string _slimTag = "slim";

        private readonly ITestOutputHelper _output;

        public ImageTestHelperTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GetTestRuntimeImage_Validate_ImageBaseEnvironmentVariableSet()
        {
            // Arrange
            var platformName = "test";
            var platformVersion = "1.0";
            var imageBaseValue = "oryxtest";
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var runtimeImage = imageHelper.GetTestRuntimeImage(platformName, platformVersion);

            // Assert
            var expectedImage = $"{imageBaseValue}/{platformName}:{platformVersion}";
            Assert.Equal(expectedImage, runtimeImage);
        }

        [Fact]
        public void GetTestRuntimeImage_Validate_TagSuffixEnvironmentVariableSet()
        {
            // Arrange
            var platformName = "test";
            var platformVersion = "1.0";
            var imageBaseValue = string.Empty;
            var tagSuffixValue = "testSuffix";
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var runtimeImage = imageHelper.GetTestRuntimeImage(platformName, platformVersion);

            // Assert
            var expectedImage = $"{_defaultImageBase}/{platformName}:{platformVersion}{tagSuffixValue}";
            Assert.Equal(expectedImage, runtimeImage);
        }

        [Fact]
        public void GetTestRuntimeImage_Validate_NoEnvironmentVariableSet()
        {
            // Arrange
            var platformName = "test";
            var platformVersion = "1.0";
            var imageBaseValue = string.Empty;
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var runtimeImage = imageHelper.GetTestRuntimeImage(platformName, platformVersion);

            // Assert
            var expectedImage = $"{_defaultImageBase}/{platformName}:{platformVersion}";
            Assert.Equal(expectedImage, runtimeImage);
        }

        [Fact]
        public void GetTestRuntimeImage_Validate_BothEnvironmentVariablesSet()
        {
            // Arrange
            var platformName = "test";
            var platformVersion = "1.0";
            var imageBaseValue = "oryxtest";
            var tagSuffixValue = "testSuffix";
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var runtimeImage = imageHelper.GetTestRuntimeImage(platformName, platformVersion);

            // Assert
            var expectedImage = $"{imageBaseValue}/{platformName}:{platformVersion}{tagSuffixValue}";
            Assert.Equal(expectedImage, runtimeImage);
        }

        [Fact]
        public void GetTestBuildImage_Validate_ImageBaseEnvironmentVariableSet()
        {
            // Arrange
            var imageBaseValue = "oryxtest";
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var buildImage = imageHelper.GetTestBuildImage();

            // Assert
            var expectedImage = $"{imageBaseValue}/{_buildRepository}:{_latestTag}";
            Assert.Equal(expectedImage, buildImage);
        }

        [Fact]
        public void GetTestBuildImage_Validate_TagSuffixEnvironmentVariableSet()
        {
            // Arrange
            var imageBaseValue = string.Empty;
            var tagSuffixValue = "testSuffix";
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var buildImage = imageHelper.GetTestBuildImage();

            // Assert
            var expectedImage = $"{_defaultImageBase}/{_buildRepository}:{tagSuffixValue}";
            Assert.Equal(expectedImage, buildImage);
        }

        [Fact]
        public void GetTestSlimBuildImage_Validate_ImageBaseEnvironmentVariableSet()
        {
            // Arrange
            var imageBaseValue = "oryxtest";
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var buildImage = imageHelper.GetTestSlimBuildImage();

            // Assert
            var expectedImage = $"{imageBaseValue}/{_buildRepository}:{_slimTag}";
            Assert.Equal(expectedImage, buildImage);
        }

        [Fact]
        public void GetTestSlimBuildImage_Validate_TagSuffixEnvironmentVariableSet()
        {
            // Arrange
            var imageBaseValue = string.Empty;
            var tagSuffixValue = "testSuffix";
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var buildImage = imageHelper.GetTestSlimBuildImage();

            // Assert
            var expectedImage = $"{_defaultImageBase}/{_buildRepository}:{_slimTag}{tagSuffixValue}";
            Assert.Equal(expectedImage, buildImage);
        }

        [Fact]
        public void GetTestPackImage_Validate_ImageBaseEnvironmentVariableSet()
        {
            // Arrange
            var imageBaseValue = "oryxtest";
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var packImage = imageHelper.GetTestPackImage();

            // Assert
            var expectedImage = $"{imageBaseValue}/{_packRepository}:{_latestTag}";
            Assert.Equal(expectedImage, packImage);
        }

        [Fact]
        public void GetTestPackImage_Validate_TagSuffixEnvironmentVariableSet()
        {
            // Arrange
            var imageBaseValue = string.Empty;
            var tagSuffixValue = "testSuffix";
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var packImage = imageHelper.GetTestPackImage();

            // Assert
            var expectedImage = $"{_defaultImageBase}/{_packRepository}:{tagSuffixValue}";
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
            var buildImage = imageHelper.GetTestBuildImage(_latestTag);

            // Assert
            var expectedImage = $"{_defaultImageBase}/{_buildRepository}:{_latestTag}";
            Assert.Equal(expectedImage, buildImage);
        }

        [Fact]
        public void GetTestBuildImage_Validate_SlimTag()
        {
            // Arrange
            var imageBaseValue = string.Empty;
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var buildImage = imageHelper.GetTestBuildImage(_slimTag);

            // Assert
            var expectedImage = $"{_defaultImageBase}/{_buildRepository}:{_slimTag}";
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
            Assert.Throws<NotSupportedException>(() => { imageHelper.GetTestBuildImage("invalidTag"); });
        }
    }
}
