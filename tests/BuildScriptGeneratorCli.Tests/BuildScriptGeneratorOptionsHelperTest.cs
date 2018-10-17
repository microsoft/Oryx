// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Xunit;

namespace BuildScriptGeneratorCli.Tests
{
    public class BuildScriptGeneratorOptionsHelperTest
        : IClassFixture<BuildScriptGeneratorOptionsHelperTest.TestFixture>
    {
        private static string _testDirPath;

        public BuildScriptGeneratorOptionsHelperTest(TestFixture testFixutre)
        {
            _testDirPath = testFixutre.RootDirPath;
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
                language: null,
                languageVersion: null,
                logFile: "logFile.txt",
                scriptOnly: false);

            // Assert
            Assert.Equal(currentDir, options.SourceDir);
            Assert.Equal(currentDir, options.DestinationDir);
            Assert.Equal(currentDir, options.IntermediateDir);
            Assert.Equal(Path.Combine(currentDir, "logFile.txt"), options.LogFile);
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
            var logFile = Path.Combine(Directory.GetCurrentDirectory(), "logFile.txt");

            // Act
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceDir: providedPath,
                destinationDir: providedPath,
                intermediateDir: providedPath,
                language: null,
                languageVersion: null,
                logFile: "logFile.txt",
                scriptOnly: false);

            // Assert
            Assert.Equal(absolutePath, options.SourceDir);
            Assert.Equal(absolutePath, options.DestinationDir);
            Assert.Equal(absolutePath, options.IntermediateDir);
            Assert.Equal(logFile, options.LogFile);
        }

        [Fact]
        public void ResolvesToAbsolutePath_WhenAbsolutePathIsGiven()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var absolutePath = Path.GetTempPath();
            var logFile = Path.Combine(Path.GetTempPath(), "logFile.txt");

            // Act
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceDir: absolutePath,
                destinationDir: absolutePath,
                intermediateDir: absolutePath,
                language: null,
                languageVersion: null,
                logFile: logFile,
                scriptOnly: false);

            // Assert
            Assert.Equal(absolutePath, options.SourceDir);
            Assert.Equal(absolutePath, options.DestinationDir);
            Assert.Equal(absolutePath, options.IntermediateDir);
            Assert.Equal(logFile, options.LogFile);
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
                language: null,
                languageVersion: null,
                logFile: Path.Combine("..", "logFile.txt"),
                scriptOnly: false);

            // Assert
            Assert.Equal(expected, options.SourceDir);
            Assert.Equal(expected, options.DestinationDir);
            Assert.Equal(expected, options.IntermediateDir);
            Assert.Equal(Path.Combine(expected, "logFile.txt"), options.LogFile);
        }

        [Fact]
        public void ResolvesToAbsolutePath_WhenDoubleDotNotationIsUsed()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var dir1 = CreateNewDir();
            var dir2 = Directory.CreateDirectory(Path.Combine(dir1, "subDir1")).FullName;
            var expected = Directory.CreateDirectory(Path.Combine(dir1, "subDir2")).FullName;

            // Act
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceDir: Path.Combine(dir2, "..", "subDir2"),
                destinationDir: Path.Combine(dir2, "..", "subDir2"),
                intermediateDir: Path.Combine(dir2, "..", "subDir2"),
                language: null,
                languageVersion: null,
                logFile: Path.Combine(dir2, "..", "subDir2", "logFile.txt"),
                scriptOnly: false);

            // Assert
            Assert.Equal(expected, options.SourceDir);
            Assert.Equal(expected, options.DestinationDir);
            Assert.Equal(expected, options.IntermediateDir);
            Assert.Equal(Path.Combine(expected, "logFile.txt"), options.LogFile);
        }

        private string CreateNewDir()
        {
            return Directory.CreateDirectory(CreatePathForNewDir()).FullName;
        }

        private string CreatePathForNewDir()
        {
            return Path.Combine(_testDirPath, Guid.NewGuid().ToString());
        }

        public class TestFixture : IDisposable
        {
            public TestFixture()
            {
                RootDirPath = Path.Combine(
                    Path.GetTempPath(),
                    nameof(BuildScriptGeneratorOptionsHelperTest));

                Directory.CreateDirectory(RootDirPath);
            }

            public string RootDirPath { get; }

            public void Dispose()
            {
                if (Directory.Exists(RootDirPath))
                {
                    try
                    {
                        Directory.Delete(RootDirPath, recursive: true);
                    }
                    catch
                    {
                        // Do not throw in dispose
                    }
                }
            }
        }
    }
}
