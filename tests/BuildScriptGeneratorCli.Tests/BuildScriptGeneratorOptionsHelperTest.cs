// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Extensions.Logging;
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
                "3.2.1",
                "logFile.txt",
                "trace");

            // Assert
            Assert.StartsWith(
                Path.Combine(Path.GetTempPath(), nameof(Microsoft.Oryx.BuildScriptGenerator)),
                options.TempDirectory);
            Assert.Equal(Directory.GetCurrentDirectory(), options.SourceCodeFolder);
            Assert.Equal(Path.Combine(Directory.GetCurrentDirectory(), "logFile.txt"), options.LogFile);
            Assert.Equal("nodeJs", options.LanguageName);
            Assert.Equal("3.2.1", options.LanguageVersion);
            Assert.Equal(LogLevel.Trace, options.MinimumLogLevel);
            Assert.Null(options.IntermediateFolder);
            Assert.Null(options.OutputFolder);
            Assert.False(options.DoNotUseIntermediateFolder);
        }

        [Fact]
        public void ResolvesToCurrentDirectoryAbsolutePaths_WhenDotNotationIsUsed()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var currentDir = Directory.GetCurrentDirectory();

            // Act
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceCodeFolder: ".",
                outputFolder: currentDir,
                intermediateFolder: currentDir,
                doNotUseIntermediateFolder: true,
                "nodeJs",
                "3.2.1",
                "logFile.txt",
                "trace");

            // Assert
            Assert.Equal(currentDir, options.SourceCodeFolder);
            Assert.Equal(currentDir, options.OutputFolder);
            Assert.Equal(currentDir, options.IntermediateFolder);
            Assert.Equal(Path.Combine(currentDir, "logFile.txt"), options.LogFile);
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
            var logFile = Path.Combine(providedPath, "logFile.txt");

            // Act
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceCodeFolder: providedPath,
                outputFolder: providedPath,
                intermediateFolder: providedPath,
                doNotUseIntermediateFolder: true,
                "nodeJs",
                "3.2.1",
                logFile,
                "trace");

            // Assert
            Assert.Equal(absolutePath, options.SourceCodeFolder);
            Assert.Equal(absolutePath, options.OutputFolder);
            Assert.Equal(absolutePath, options.IntermediateFolder);
            Assert.Equal(Path.Combine(absolutePath, "logFile.txt"), options.LogFile);
        }

        [Fact]
        public void ResolvesToAbsolutePaths_WhenAbsolutePathsAreGiven()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var absolutePath = Path.GetTempPath();
            var logFile = Path.Combine(Path.GetTempPath(), "logFile.txt");

            // Act
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceCodeFolder: absolutePath,
                outputFolder: absolutePath,
                intermediateFolder: absolutePath,
                doNotUseIntermediateFolder: true,
                "nodeJs",
                "3.2.1",
                logFile,
                "trace");

            // Assert
            Assert.Equal(absolutePath, options.SourceCodeFolder);
            Assert.Equal(absolutePath, options.OutputFolder);
            Assert.Equal(absolutePath, options.IntermediateFolder);
            Assert.Equal(logFile, options.LogFile);
        }

        [Theory]
        [InlineData("trace", LogLevel.Trace)]
        [InlineData("debug", LogLevel.Debug)]
        [InlineData("information", LogLevel.Information)]
        [InlineData("warning", LogLevel.Warning)]
        [InlineData("error", LogLevel.Error)]
        [InlineData("critical", LogLevel.Critical)]
        public void ConfiguresOptions_ForAllAllowedLoggingLevels(string logLevel, LogLevel expected)
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();

            // Act
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceCodeFolder: ".",
                outputFolder: ".",
                intermediateFolder: ".",
                doNotUseIntermediateFolder: false,
                "nodeJs",
                "3.2.1",
                Path.Combine(Path.GetTempPath(), "logFile.txt"),
                logLevel);

            // Assert
            Assert.Equal(expected, options.MinimumLogLevel);
        }
    }
}
