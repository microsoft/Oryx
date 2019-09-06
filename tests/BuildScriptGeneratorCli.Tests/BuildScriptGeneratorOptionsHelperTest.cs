// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    public class BuildScriptGeneratorOptionsHelperTest : IClassFixture<TestTempDirTestFixture>
    {
        private static string _testDirPath;

        public BuildScriptGeneratorOptionsHelperTest(TestTempDirTestFixture testFixture)
        {
            _testDirPath = testFixture.RootDirPath;
        }

        [Fact]
        public void ResolvesToCurrentDirectoryAbsolutePath_WhenDotNotationIsUsed()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var currentDir = Directory.GetCurrentDirectory();

            // Act
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceDir: ".",
                destinationDir: ".",
                intermediateDir: ".",
                manifestDir: ".",
                platform: null,
                platformVersion: null,
                shouldPackage: false,
                requiredOsPackages: null,
                scriptOnly: false,
                properties: null);

            // Assert
            Assert.Equal(currentDir, options.SourceDir);
            Assert.Equal(currentDir, options.DestinationDir);
            Assert.Equal(currentDir, options.IntermediateDir);
            Assert.Equal(currentDir, options.ManifestDir);
        }

        [Theory]
        [InlineData("dir1")]
        [InlineData("dir1", "dir2")]
        public void ResolvesToAbsolutePath_WhenRelativePathIsGiven(params string[] paths)
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var providedPath = Path.Combine(paths);
            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), providedPath);

            // Act
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceDir: providedPath,
                destinationDir: providedPath,
                intermediateDir: providedPath,
                manifestDir: providedPath,
                platform: null,
                platformVersion: null,
                shouldPackage: false,
                requiredOsPackages: null,
                scriptOnly: false,
                properties: null);

            // Assert
            Assert.Equal(absolutePath, options.SourceDir);
            Assert.Equal(absolutePath, options.DestinationDir);
            Assert.Equal(absolutePath, options.IntermediateDir);
            Assert.Equal(absolutePath, options.ManifestDir);
        }

        [Fact]
        public void ResolvesToAbsolutePath_WhenAbsolutePathIsGiven()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var absolutePath = Path.GetTempPath();

            // Act
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceDir: absolutePath,
                destinationDir: absolutePath,
                intermediateDir: absolutePath,
                manifestDir: absolutePath,
                platform: null,
                platformVersion: null,
                shouldPackage: false,
                requiredOsPackages: null,
                scriptOnly: false,
                properties: null);

            // Assert
            Assert.Equal(absolutePath, options.SourceDir);
            Assert.Equal(absolutePath, options.DestinationDir);
            Assert.Equal(absolutePath, options.IntermediateDir);
            Assert.Equal(absolutePath, options.ManifestDir);
        }

        [Fact]
        public void ResolvesToAbsolutePath_WhenDoubleDotNotationIsUsed_RelativeToCurrentDir()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var currentDir = Directory.GetCurrentDirectory();
            var expected = new DirectoryInfo(currentDir).Parent.FullName;

            // Act
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceDir: "..",
                destinationDir: "..",
                intermediateDir: "..",
                manifestDir: "..",
                platform: null,
                platformVersion: null,
                shouldPackage: false,
                requiredOsPackages: null,
                scriptOnly: false,
                properties: null);

            // Assert
            Assert.Equal(expected, options.SourceDir);
            Assert.Equal(expected, options.DestinationDir);
            Assert.Equal(expected, options.IntermediateDir);
            Assert.Equal(expected, options.ManifestDir);
        }

        [Fact]
        public void ResolvesToAbsolutePath_WhenDoubleDotNotationIsUsed()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var dir1 = CreateNewDir();
            var dir2 = Directory.CreateDirectory(Path.Combine(dir1, "subDir1")).FullName;
            var expected = Directory.CreateDirectory(Path.Combine(dir1, "subDir2")).FullName;
            var relativePath = Path.Combine(dir2, "..", "subDir2");

            // Act
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceDir: relativePath,
                destinationDir: relativePath,
                intermediateDir: relativePath,
                manifestDir: relativePath,
                platform: null,
                platformVersion: null,
                shouldPackage: false,
                requiredOsPackages: null,
                scriptOnly: false,
                properties: null);

            // Assert
            Assert.Equal(expected, options.SourceDir);
            Assert.Equal(expected, options.DestinationDir);
            Assert.Equal(expected, options.IntermediateDir);
            Assert.Equal(expected, options.ManifestDir);
        }

        [Theory]
        [InlineData("=")]
        [InlineData("==")]
        [InlineData("=true")]
        public void ProcessProperties_Throws_WhenKeyIsNotPresent(string property)
        {
            // Arrange
            var properties = new[] { property };

            // Act & Assert
            var exception = Assert.Throws<InvalidUsageException>(
                () => BuildScriptGeneratorOptionsHelper.ProcessProperties(properties));

            Assert.Equal($"Property key cannot start with '=' for property '{property}'.", exception.Message);
        }

        [Theory]
        [InlineData("a=\"b c\"", "a", "b c")]
        [InlineData("a=\"b \"", "a", "b ")]
        [InlineData("a=\" b\"", "a", " b")]
        [InlineData("a=\" b \"", "a", " b ")]
        [InlineData("\"a b\"=d", "a b", "d")]
        [InlineData("\"a \"=d", "a ", "d")]
        [InlineData("\" a\"=d", " a", "d")]
        public void ProcessProperties_ReturnsProperty_TrimmingTheQuotes(
            string property,
            string key,
            string value)
        {
            // Arrange
            var properties = new[] { property };

            // Act
            var actual = BuildScriptGeneratorOptionsHelper.ProcessProperties(properties);

            // Assert
            Assert.Collection(
                actual,
                (kvp) => { Assert.Equal(key, kvp.Key); Assert.Equal(value, kvp.Value); });
        }

        private string CreateNewDir()
        {
            return Directory.CreateDirectory(CreatePathForNewDir()).FullName;
        }

        private string CreatePathForNewDir()
        {
            return Path.Combine(_testDirPath, Guid.NewGuid().ToString());
        }
    }
}
