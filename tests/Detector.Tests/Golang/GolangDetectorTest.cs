// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Detector.Golang;
using Microsoft.Oryx.Tests.Common;
using Xunit;
namespace Microsoft.Oryx.Detector.Tests.Golang
{
    public class GolangDetectorTest : IClassFixture<TestTempDirTestFixture>
    {
        // TODO: parameterize go.mod to remove redundant code
        private const string GolangModFileWithMajorMinorVersion = @"
            module hello

            go 1.16
        ";

        private const string GolangModFileWithOnlyMajorVersion = @"
            module hello

            go 1
        ";

        private const string GolangModFileWithMajorMinorPatchVersion = @"
            module hello

            go 1.16.7
        ";

        private const string GolangFileWithNoVersions = @"
            module hello
        ";

        private readonly string _tempDirRoot;

        public GolangDetectorTest(TestTempDirTestFixture testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }

        // test empty source repo
        [Fact]
        public void Detect_ReturnsNull_WhenSourceDirectory_IsEmpty()
        {
            // Arrange
            // No files in source repo
            var sourceDir = Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString())).FullName;
            var repo = new LocalSourceRepo(sourceDir);
            var detector = CreateGolangPlatformDetector();
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsNull_ForSourceRepoHasOnlyGoFile()
        {
            // Arrange
            var detector = CreateGolangPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile("", "main.go");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ModFileWithMajorMinorVersion()
        {
            // Arrange
            var detector = CreateGolangPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(GolangModFileWithMajorMinorVersion, GolangConstants.GoModFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(GolangConstants.PlatformName, result.Platform);
            Assert.Equal("1.16", result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
        }

        [Fact]
        public void Detect_ModFileWithMajorMinorPatchVersion()
        {
            // Arrange
            var detector = CreateGolangPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(GolangModFileWithMajorMinorPatchVersion, GolangConstants.GoModFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(GolangConstants.PlatformName, result.Platform);
            Assert.Equal("1.16.7", result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
        }

        [Fact]
        public void Detect_ModFileWithOnlyMajorVersion()
        {
            // Arrange
            var detector = CreateGolangPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(GolangModFileWithOnlyMajorVersion, GolangConstants.GoModFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(GolangConstants.PlatformName, result.Platform);
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

        private GolangDetector CreateGolangPlatformDetector(DetectorOptions options = null)
        {
            options = options ?? new DetectorOptions();
            return new GolangDetector(NullLogger<GolangDetector>.Instance, Options.Create(options));
        }
    }
}
