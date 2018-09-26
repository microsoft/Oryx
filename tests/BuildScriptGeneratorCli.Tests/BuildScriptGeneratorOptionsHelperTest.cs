// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Xunit;

namespace BuildScriptGeneratorCli.Tests
{
    public class BuildScriptGeneratorOptionsHelperTest
    {
        [Fact]
        public void ConfiguresOptions_WithScriptGeneratorRootDirectory_InTemp()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();

            // Act
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceCodeFolder: ".",
                "nodeJs",
                "3.2.1");

            // Assert
            Assert.StartsWith(
                Path.Combine(Path.GetTempPath(), nameof(Microsoft.Oryx.BuildScriptGenerator)),
                options.TempDirectory);
            Assert.Equal(Directory.GetCurrentDirectory(), options.SourceCodeFolder);
            Assert.Equal("nodeJs", options.LanguageName);
            Assert.Equal("3.2.1", options.LanguageVersion);
            Assert.Null(options.IntermediateFolder);
            Assert.Null(options.OutputFolder);
            Assert.False(options.DoNotUseIntermediateFolder);
        }

        [Fact]
        public void ConfiguresOptions2()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var folder = Directory.GetCurrentDirectory();

            // Act
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceCodeFolder: folder,
                outputFolder: folder,
                intermediateFolder: folder,
                doNotUseIntermediateFolder: true,
                "nodeJs",
                "3.2.1");

            // Assert
            Assert.Equal(folder, options.SourceCodeFolder);
            Assert.Equal(folder, options.OutputFolder);
            Assert.Equal(folder, options.IntermediateFolder);
            Assert.Equal("nodeJs", options.LanguageName);
            Assert.Equal("3.2.1", options.LanguageVersion);
            Assert.StartsWith(
                Path.Combine(Path.GetTempPath(), nameof(Microsoft.Oryx.BuildScriptGenerator)),
                options.TempDirectory);
            Assert.True(options.DoNotUseIntermediateFolder);
        }

        [Fact]
        public void ResolvesToCurrentDirectoryAbsolutePaths_WhenDotNotationIsUsed()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var currentDir = ".";
            var expected = Directory.GetCurrentDirectory();

            // Act
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceCodeFolder: currentDir,
                outputFolder: currentDir,
                intermediateFolder: currentDir,
                doNotUseIntermediateFolder: true,
                "nodeJs",
                "3.2.1");

            // Assert
            Assert.Equal(expected, options.SourceCodeFolder);
            Assert.Equal(expected, options.OutputFolder);
            Assert.Equal(expected, options.IntermediateFolder);
        }

        [Theory]
        [InlineData("dir1")]
        [InlineData("dir1", "dir2")]
        public void ResolvesToAbsolutePaths_WhenRelativePathsAreGiven(params string[] paths)
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var providedPath = Path.Combine(paths);
            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), providedPath);

            // Act
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceCodeFolder: providedPath,
                outputFolder: providedPath,
                intermediateFolder: providedPath,
                doNotUseIntermediateFolder: true,
                "nodeJs",
                "3.2.1");

            // Assert
            Assert.Equal(absolutePath, options.SourceCodeFolder);
            Assert.Equal(absolutePath, options.OutputFolder);
            Assert.Equal(absolutePath, options.IntermediateFolder);
        }

        [Fact]
        public void ResolvesToAbsolutePaths_WhenAbsolutePathsAreGiven()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var absolutePath = Path.GetTempPath();

            // Act
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceCodeFolder: absolutePath,
                outputFolder: absolutePath,
                intermediateFolder: absolutePath,
                doNotUseIntermediateFolder: true,
                "nodeJs",
                "3.2.1");

            // Assert
            Assert.Equal(absolutePath, options.SourceCodeFolder);
            Assert.Equal(absolutePath, options.OutputFolder);
            Assert.Equal(absolutePath, options.IntermediateFolder);
        }
    }
}
