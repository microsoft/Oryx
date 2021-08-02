// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Detector.Go;
using Microsoft.Oryx.Tests.Common;
using Xunit;
namespace Microsoft.Oryx.Detector.Tests.Go
{
    public class GoDetectorTest : IClassFixture<TestTempDirTestFixture>
    {

        private const string GoFileWithVersion = @"
            module hello

            go 1.16
        ";

        private const string GoFileWithNoVersions = @"
            module hello
        ";
        private readonly string _tempDirRoot;

        public GoDetectorTest(TestTempDirTestFixture testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }

        // test empty source repo
        [Fact]
        public void Detect_ReturnsNull_WhenSourceDirectoryIsEmpty()
        {
            // Arrange
            // No files in source repo
            var sourceDir = Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString())).FullName;
            var repo = new LocalSourceRepo(sourceDir);
            var detector = CreateGoPlatformDetector();
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsNull_ForSourceRepoOnlyWithDotGoFile()
        {
            // Arrange
            var detector = CreateGoPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile("", "main.go");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsNullVersion_ForModfileWithNoVersion()
        {
            // Arrange
            var detector = CreateGoPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(GoFileWithNoVersions, GoConstants.GoDotModFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(GoConstants.PlatformName, result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
        }

        [Fact]
        public void Detect_ReturnsVersionFromGofile()
        {
            // Arrange
            var detector = CreateGoPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(GoFileWithVersion, GoConstants.GoDotModFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(GoConstants.PlatformName, result.Platform);
            Assert.Equal("1.16", result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
        }


        private DetectorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new DetectorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private GoDetector CreateGoPlatformDetector(DetectorOptions options = null)
        {
            options = options ?? new DetectorOptions();
            return new GoDetector(NullLogger<GoDetector>.Instance, Options.Create(options));
        }
    }
}
