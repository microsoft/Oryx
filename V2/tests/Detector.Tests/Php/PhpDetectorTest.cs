// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Oryx.Detector.Php;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.Detector.Tests.Php
{
    public class PhpDetectorTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRoot;

        public PhpDetectorTest(TestTempDirTestFixture testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }

        [Fact]
        public void Detect_ReturnsNull_WhenSourceDirectoryIsEmpty()
        {
            // Arrange
            // No files in source repo
            var sourceDir = Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString())).FullName;
            var repo = new LocalSourceRepo(sourceDir);
            var detector = CreatePhpPlatformDetector();
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReutrnsResult_WhenComposerFileDoesNotExist_ButFilesWithPhpExtensionExist()
        {
            // Arrange
            var sourceDir = Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString()))
                .FullName;
            File.WriteAllText(Path.Combine(sourceDir, "foo.php"), "php file content");
            var repo = new LocalSourceRepo(sourceDir);
            var detector = CreatePhpPlatformDetector();
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PlatformName, result.Platform);
            Assert.Equal(string.Empty, result.AppDirectory);
            Assert.Null(result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReutrnsResult_WhenComposerFileDoesNotExist_ButFilesWithPhpExtensionExistInSubDirectories()
        {
            // Arrange
            var sourceDir = Directory.CreateDirectory(
                Path.Combine(_tempDirRoot, Guid.NewGuid().ToString()))
                .FullName;
            var subDirStr = Guid.NewGuid().ToString();
            var subDir = Directory.CreateDirectory(Path.Combine(sourceDir, subDirStr)).FullName;
            File.WriteAllText(Path.Combine(subDir, "foo.php"), "php file content");
            var repo = new LocalSourceRepo(sourceDir);
            var detector = CreatePhpPlatformDetector();
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PlatformName, result.Platform);
            Assert.Equal(subDirStr, result.AppDirectory);
            Assert.Null(result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsVersion_FromComposerFile()
        {
            // Arrange
            var detector = CreatePhpPlatformDetector();
            var repo = new MemorySourceRepo();
            var version = "5.6.0";
            repo.AddFile("{\"require\":{\"php\":\"" + version + "\"}}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("5.6.0", result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
        }

        [Theory]
        [InlineData("invalid json")]
        [InlineData("{\"data\": \"valid but meaningless\"}")]
        public void Detect_ReturnsNullVersion_WithComposerFile(string composerFileContent)
        {
            // Arrange
            var detector = CreatePhpPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(composerFileContent, PhpConstants.ComposerFileName);
            repo.AddFile("<?php echo true; ?>", "foo.php");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PlatformName, result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
        }

        private DetectorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new DetectorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private PhpDetector CreatePhpPlatformDetector()
        {
            return new PhpDetector(
                NullLogger<PhpDetector>.Instance);
        }
    }
}
