// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Detector.Ruby;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.Detector.Tests.Ruby
{
    public class RubyDetectorTest : IClassFixture<TestTempDirTestFixture>
    {
        private const string GemFileWithNoVersions = @"
            source 'https://rubygems.org'
            gem 'sinatra'
        ";

        private const string GemFileWithVersion = @"
            source 'https://rubygems.org'
            gem 'sinatra'
            ruby '2.7.1'
        ";

        private const string GemFileLockWithPatchVersion = @"
            PLATFORMS
              ruby
            RUBY VERSION
              ruby 2.3.1p112
        ";

        private const string GemFileLockWithVersion = @"
            PLATFORMS
              ruby
            RUBY VERSION
              ruby 2.3.1
        ";

        private const string GemFileLockWithPreviewVersion = @"
            PLATFORMS
              ruby
            RUBY VERSION
              ruby 2.3.1-preview1
        ";

        private const string GemFileLockWithRcVersion = @"
            PLATFORMS
              ruby
            RUBY VERSION
              ruby 2.3.1.rc1
        ";

        private const string MalformedGemfile = @"
            source 'https://rubygems.org'
            ruby '2.7.1' '3.5'
        ";

        private readonly string _tempDirRoot;

        public RubyDetectorTest(TestTempDirTestFixture testFixture)
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
            var detector = CreateRubyPlatformDetector();
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsNull_ForSourceRepoOnlyWithDotRubyFile()
        {
            // Arrange
            var detector = CreateRubyPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile("", "app.rb");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsNull_ForSourceRepoWithGemfile_NotInRootDirectory()
        {
            // Arrange
            var detector = CreateRubyPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile("", "subDir1", "Gemfile");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsNullVersion_ForGemfileWithNoVersion()
        {
            // Arrange
            var detector = CreateRubyPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(GemFileWithNoVersions, RubyConstants.GemFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(RubyConstants.PlatformName, result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
        }

        [Fact]
        public void Detect_ReturnsNullVersion_ForGemfileLockWithNoVersion()
        {
            // Arrange
            var detector = CreateRubyPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(" ", RubyConstants.GemFileLockName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(RubyConstants.PlatformName, result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
        }

        [Fact]
        public void Detect_ReturnsVersionFromGemfile()
        {
            // Arrange
            var detector = CreateRubyPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(GemFileWithVersion, RubyConstants.GemFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(RubyConstants.PlatformName, result.Platform);
            Assert.Equal("2.7.1", result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
        }

        [Theory]
        [InlineData(GemFileLockWithVersion, "2.3.1")]
        [InlineData(GemFileLockWithPatchVersion, "2.3.1")]
        [InlineData(GemFileLockWithRcVersion, "2.3.1.rc1")]
        [InlineData(GemFileLockWithPreviewVersion, "2.3.1-preview1")]
        public void Detect_ReturnsVersionFromGemfileLock(string fileContents, string expectedVersion)
        {
            // Arrange
            var detector = CreateRubyPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(fileContents, RubyConstants.GemFileLockName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(RubyConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
        }

        [Fact]
        public void Detect_ReturnsNullVersion_ForMalformedGemfile()
        {
            // Arrange
            var detector = CreateRubyPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(MalformedGemfile, RubyConstants.GemFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(RubyConstants.PlatformName, result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
        }

        [Theory]
        [InlineData("default.htm")]
        [InlineData("default.html")]
        [InlineData("default.asp")]
        [InlineData("index.htm")]
        [InlineData("index.html")]
        [InlineData("iisstart.htm")]
        [InlineData("default.aspx")]
        [InlineData("index.php")]
        public void Detect_ReturnsNull_IfIISStartupFileIsPresent(string iisStartupFileName)
        {
            // Arrange
            var sourceRepo = new MemorySourceRepo();
            sourceRepo.AddFile("", iisStartupFileName);
            var detector = CreateRubyPlatformDetector();
            var context = CreateContext(sourceRepo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("default.htm")]
        [InlineData("default.html")]
        [InlineData("default.asp")]
        [InlineData("index.htm")]
        [InlineData("index.html")]
        [InlineData("iisstart.htm")]
        [InlineData("default.aspx")]
        [InlineData("index.php")]
        public void Detect_ReturnsNull_IfGemfileLock_AndConfigFile_AndIISStartupFileIsPresent(string iisStartupFileName)
        {
            // Arrange
            var sourceRepo = new MemorySourceRepo();
            sourceRepo.AddFile("", RubyConstants.GemFileLockName);
            sourceRepo.AddFile("", RubyConstants.ConfigRubyFileName);
            sourceRepo.AddFile("", iisStartupFileName);
            var detector = CreateRubyPlatformDetector();
            var context = CreateContext(sourceRepo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsTrue_IfOnlyConfigYmlFileExists_AndItsStaticWebApp()
        {
            // Arrange
            var options = new DetectorOptions
            {
                AppType = Constants.StaticSiteApplications,
            };
            var detector = CreateRubyPlatformDetector(options);
            var repo = new MemorySourceRepo();
            repo.AddFile("", RubyConstants.ConfigYmlFileName);
            var context = CreateContext(repo);

            // Act
            var result = (RubyPlatformDetectorResult)detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(RubyConstants.PlatformName, result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
            Assert.False(result.GemfileExists);
            Assert.True(result.ConfigYmlFileExists);
        }

        [Fact]
        public void Detect_ReturnsNull_IfOnlyConfigYmlFileExists_AndItsNotStaticWebApp()
        {
            // Arrange
            var detector = CreateRubyPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile("", RubyConstants.ConfigYmlFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsTrue_IfBothConfigYmlFileAndGemfileExist_AndItsNotStaticWebApp()
        {
            // Arrange
            var detector = CreateRubyPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile("", RubyConstants.ConfigYmlFileName);
            repo.AddFile("", RubyConstants.GemFileName);
            var context = CreateContext(repo);

            // Act
            var result = (RubyPlatformDetectorResult)detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(RubyConstants.PlatformName, result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
            Assert.True(result.GemfileExists);
            Assert.True(result.ConfigYmlFileExists);
        }

        [Fact]
        public void Detect_ReturnsTrue_IfOnlyGemfileExist_AndItsStaticWebApp()
        {
            // Arrange
            var options = new DetectorOptions
            {
                AppType = Constants.StaticSiteApplications,
            };
            var detector = CreateRubyPlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile("", RubyConstants.GemFileName);
            var context = CreateContext(repo);

            // Act
            var result = (RubyPlatformDetectorResult)detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(RubyConstants.PlatformName, result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
            Assert.True(result.GemfileExists);
            Assert.False(result.ConfigYmlFileExists);
        }

        private DetectorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new DetectorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private RubyDetector CreateRubyPlatformDetector(DetectorOptions options = null)
        {
            options = options ?? new DetectorOptions();
            return new RubyDetector(NullLogger<RubyDetector>.Instance, Options.Create(options));
        }
    }
}