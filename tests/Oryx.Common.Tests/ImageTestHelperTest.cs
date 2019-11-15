// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.Common.Tests
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
            var previousImageBaseValue = Environment.GetEnvironmentVariable(_imageBaseEnvironmentVariable);
            var previousTagSuffixValue = Environment.GetEnvironmentVariable(_tagSuffixEnvironmentVariable);
            try
            {
                // Arrange
                var platformName = "test";
                var platformVersion = "1.0";
                var imageBaseValue = "oryxtest";
                var tagSuffixValue = string.Empty;
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, imageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, tagSuffixValue);

                var imageHelper = new ImageTestHelper(_output);

                // Act
                var runtimeImage = imageHelper.GetTestRuntimeImage(platformName, platformVersion);

                // Assert
                var expectedImage = $"{imageBaseValue}/{platformName}:{platformVersion}";
                Assert.Equal(expectedImage, runtimeImage);
            }
            finally
            {
                // Set the environment variables back to their original value
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, previousImageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, previousTagSuffixValue);
            }
        }

        [Fact]
        public void GetTestRuntimeImage_Validate_TagSuffixEnvironmentVariableSet()
        {
            var previousImageBaseValue = Environment.GetEnvironmentVariable(_imageBaseEnvironmentVariable);
            var previousTagSuffixValue = Environment.GetEnvironmentVariable(_tagSuffixEnvironmentVariable);
            try
            {
                // Arrange
                var platformName = "test";
                var platformVersion = "1.0";
                var imageBaseValue = string.Empty;
                var tagSuffixValue = "testSuffix";
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, imageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, tagSuffixValue);

                var imageHelper = new ImageTestHelper(_output);

                // Act
                var runtimeImage = imageHelper.GetTestRuntimeImage(platformName, platformVersion);

                // Assert
                var expectedImage = $"{_defaultImageBase}/{platformName}:{platformVersion}{tagSuffixValue}";
                Assert.Equal(expectedImage, runtimeImage);
            }
            finally
            {
                // Set the environment variables back to their original value
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, previousImageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, previousTagSuffixValue);
            }
        }

        [Fact]
        public void GetTestRuntimeImage_Validate_NoEnvironmentVariableSet()
        {
            var previousImageBaseValue = Environment.GetEnvironmentVariable(_imageBaseEnvironmentVariable);
            var previousTagSuffixValue = Environment.GetEnvironmentVariable(_tagSuffixEnvironmentVariable);
            try
            {
                // Arrange
                var platformName = "test";
                var platformVersion = "1.0";
                var imageBaseValue = string.Empty;
                var tagSuffixValue = string.Empty;
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, imageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, tagSuffixValue);

                var imageHelper = new ImageTestHelper(_output);

                // Act
                var runtimeImage = imageHelper.GetTestRuntimeImage(platformName, platformVersion);

                // Assert
                var expectedImage = $"{_defaultImageBase}/{platformName}:{platformVersion}";
                Assert.Equal(expectedImage, runtimeImage);
            }
            finally
            {
                // Set the environment variables back to their original value
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, previousImageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, previousTagSuffixValue);
            }
        }

        [Fact]
        public void GetTestRuntimeImage_Validate_BothEnvironmentVariablesSet()
        {
            var previousImageBaseValue = Environment.GetEnvironmentVariable(_imageBaseEnvironmentVariable);
            var previousTagSuffixValue = Environment.GetEnvironmentVariable(_tagSuffixEnvironmentVariable);
            try
            {
                // Arrange
                var platformName = "test";
                var platformVersion = "1.0";
                var imageBaseValue = "oryxtest";
                var tagSuffixValue = "testSuffix";
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, imageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, tagSuffixValue);

                var imageHelper = new ImageTestHelper(_output);

                // Act
                var runtimeImage = imageHelper.GetTestRuntimeImage(platformName, platformVersion);

                // Assert
                var expectedImage = $"{imageBaseValue}/{platformName}:{platformVersion}{tagSuffixValue}";
                Assert.Equal(expectedImage, runtimeImage);
            }
            finally
            {
                // Set the environment variables back to their original value
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, previousImageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, previousTagSuffixValue);
            }
        }

        [Fact]
        public void GetTestBuildImage_Validate_ImageBaseEnvironmentVariableSet()
        {
            var previousImageBaseValue = Environment.GetEnvironmentVariable(_imageBaseEnvironmentVariable);
            var previousTagSuffixValue = Environment.GetEnvironmentVariable(_tagSuffixEnvironmentVariable);
            try
            {
                // Arrange
                var imageBaseValue = "oryxtest";
                var tagSuffixValue = string.Empty;
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, imageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, tagSuffixValue);

                var imageHelper = new ImageTestHelper(_output);

                // Act
                var buildImage = imageHelper.GetTestBuildImage();

                // Assert
                var expectedImage = $"{imageBaseValue}/{_buildRepository}:{_latestTag}";
                Assert.Equal(expectedImage, buildImage);
            }
            finally
            {
                // Set the environment variables back to their original value
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, previousImageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, previousTagSuffixValue);
            }
        }

        [Fact]
        public void GetTestBuildImage_Validate_TagSuffixEnvironmentVariableSet()
        {
            var previousImageBaseValue = Environment.GetEnvironmentVariable(_imageBaseEnvironmentVariable);
            var previousTagSuffixValue = Environment.GetEnvironmentVariable(_tagSuffixEnvironmentVariable);
            try
            {
                // Arrange
                var imageBaseValue = string.Empty;
                var tagSuffixValue = "testSuffix";
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, imageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, tagSuffixValue);

                var imageHelper = new ImageTestHelper(_output);

                // Act
                var buildImage = imageHelper.GetTestBuildImage();

                // Assert
                var expectedImage = $"{_defaultImageBase}/{_buildRepository}:{tagSuffixValue}";
                Assert.Equal(expectedImage, buildImage);
            }
            finally
            {
                // Set the environment variables back to their original value
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, previousImageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, previousTagSuffixValue);
            }
        }

        [Fact]
        public void GetTestSlimBuildImage_Validate_ImageBaseEnvironmentVariableSet()
        {
            var previousImageBaseValue = Environment.GetEnvironmentVariable(_imageBaseEnvironmentVariable);
            var previousTagSuffixValue = Environment.GetEnvironmentVariable(_tagSuffixEnvironmentVariable);
            try
            {
                // Arrange
                var imageBaseValue = "oryxtest";
                var tagSuffixValue = string.Empty;
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, imageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, tagSuffixValue);

                var imageHelper = new ImageTestHelper(_output);

                // Act
                var buildImage = imageHelper.GetTestSlimBuildImage();

                // Assert
                var expectedImage = $"{imageBaseValue}/{_buildRepository}:{_slimTag}";
                Assert.Equal(expectedImage, buildImage);
            }
            finally
            {
                // Set the environment variables back to their original value
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, previousImageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, previousTagSuffixValue);
            }
        }

        [Fact]
        public void GetTestSlimBuildImage_Validate_TagSuffixEnvironmentVariableSet()
        {
            var previousImageBaseValue = Environment.GetEnvironmentVariable(_imageBaseEnvironmentVariable);
            var previousTagSuffixValue = Environment.GetEnvironmentVariable(_tagSuffixEnvironmentVariable);
            try
            {
                // Arrange
                var imageBaseValue = string.Empty;
                var tagSuffixValue = "testSuffix";
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, imageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, tagSuffixValue);

                var imageHelper = new ImageTestHelper(_output);

                // Act
                var buildImage = imageHelper.GetTestSlimBuildImage();

                // Assert
                var expectedImage = $"{_defaultImageBase}/{_buildRepository}:{_slimTag}{tagSuffixValue}";
                Assert.Equal(expectedImage, buildImage);
            }
            finally
            {
                // Set the environment variables back to their original value
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, previousImageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, previousTagSuffixValue);
            }
        }

        [Fact]
        public void GetTestPackImage_Validate_ImageBaseEnvironmentVariableSet()
        {
            var previousImageBaseValue = Environment.GetEnvironmentVariable(_imageBaseEnvironmentVariable);
            var previousTagSuffixValue = Environment.GetEnvironmentVariable(_tagSuffixEnvironmentVariable);
            try
            {
                // Arrange
                var imageBaseValue = "oryxtest";
                var tagSuffixValue = string.Empty;
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, imageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, tagSuffixValue);

                var imageHelper = new ImageTestHelper(_output);

                // Act
                var packImage = imageHelper.GetTestPackImage();

                // Assert
                var expectedImage = $"{imageBaseValue}/{_packRepository}:{_latestTag}";
                Assert.Equal(expectedImage, packImage);
            }
            finally
            {
                // Set the environment variables back to their original value
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, previousImageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, previousTagSuffixValue);
            }
        }

        [Fact]
        public void GetTestPackImage_Validate_TagSuffixEnvironmentVariableSet()
        {
            var previousImageBaseValue = Environment.GetEnvironmentVariable(_imageBaseEnvironmentVariable);
            var previousTagSuffixValue = Environment.GetEnvironmentVariable(_tagSuffixEnvironmentVariable);
            try
            {
                // Arrange
                var imageBaseValue = string.Empty;
                var tagSuffixValue = "testSuffix";
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, imageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, tagSuffixValue);

                var imageHelper = new ImageTestHelper(_output);

                // Act
                var packImage = imageHelper.GetTestPackImage();

                // Assert
                var expectedImage = $"{_defaultImageBase}/{_packRepository}:{tagSuffixValue}";
                Assert.Equal(expectedImage, packImage);
            }
            finally
            {
                // Set the environment variables back to their original value
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, previousImageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, previousTagSuffixValue);
            }
        }

        [Fact]
        public void GetTestBuildImage_Validate_LatestTag()
        {
            var previousImageBaseValue = Environment.GetEnvironmentVariable(_imageBaseEnvironmentVariable);
            var previousTagSuffixValue = Environment.GetEnvironmentVariable(_tagSuffixEnvironmentVariable);
            try
            {
                // Arrange
                var imageBaseValue = string.Empty;
                var tagSuffixValue = string.Empty;
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, imageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, tagSuffixValue);

                var imageHelper = new ImageTestHelper(_output);

                // Act
                var buildImage = imageHelper.GetTestBuildImage(_latestTag);

                // Assert
                var expectedImage = $"{_defaultImageBase}/{_buildRepository}:{_latestTag}";
                Assert.Equal(expectedImage, buildImage);
            }
            finally
            {
                // Set the environment variables back to their original value
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, previousImageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, previousTagSuffixValue);
            }
        }

        [Fact]
        public void GetTestBuildImage_Validate_SlimTag()
        {
            var previousImageBaseValue = Environment.GetEnvironmentVariable(_imageBaseEnvironmentVariable);
            var previousTagSuffixValue = Environment.GetEnvironmentVariable(_tagSuffixEnvironmentVariable);
            try
            {
                // Arrange
                var imageBaseValue = string.Empty;
                var tagSuffixValue = string.Empty;
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, imageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, tagSuffixValue);

                var imageHelper = new ImageTestHelper(_output);

                // Act
                var buildImage = imageHelper.GetTestBuildImage(_slimTag);

                // Assert
                var expectedImage = $"{_defaultImageBase}/{_buildRepository}:{_slimTag}";
                Assert.Equal(expectedImage, buildImage);
            }
            finally
            {
                // Set the environment variables back to their original value
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, previousImageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, previousTagSuffixValue);
            }
        }

        [Fact]
        public void GetTestBuildImage_Validate_InvalidTag()
        {
            var previousImageBaseValue = Environment.GetEnvironmentVariable(_imageBaseEnvironmentVariable);
            var previousTagSuffixValue = Environment.GetEnvironmentVariable(_tagSuffixEnvironmentVariable);
            try
            {
                // Arrange
                var imageBaseValue = string.Empty;
                var tagSuffixValue = string.Empty;
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, imageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, tagSuffixValue);

                var imageHelper = new ImageTestHelper(_output);

                // Assert
                Assert.Throws<NotSupportedException>(() => { imageHelper.GetTestBuildImage("invalidTag"); });
            }
            finally
            {
                // Set the environment variables back to their original value
                Environment.SetEnvironmentVariable(_imageBaseEnvironmentVariable, previousImageBaseValue);
                Environment.SetEnvironmentVariable(_tagSuffixEnvironmentVariable, previousTagSuffixValue);
            }
        }
    }
}
