// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

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
            var detector = CreatePhpPlatformDetector();
            var repo = new MemorySourceRepo(); // No files in source repo
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReutrnsNull_WhenComposerFileDoesNotExist()
        {
            // Arrange
            var detector = CreatePhpPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile("foo.php content", "foo.php");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
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
