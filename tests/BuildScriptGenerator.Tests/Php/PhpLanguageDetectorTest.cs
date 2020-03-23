// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Php
{
    public class PhpLanguageDetectorTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRoot;

        public PhpLanguageDetectorTest(TestTempDirTestFixture testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }

        [Fact]
        public void Detect_ReturnsNull_WhenSourceDirectoryIsEmpty()
        {
            // Arrange
            var detector = CreatePhpLanguageDetector(supportedPhpVersions: new[] { PhpVersions.Php73Version });
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
            var detector = CreatePhpLanguageDetector(supportedPhpVersions: new[] { PhpVersions.Php73Version });
            var repo = new MemorySourceRepo();
            repo.AddFile("foo.php content", "foo.php");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedPhpVersion_FoundInComposerFile()
        {
            // Arrange
            var detector = CreatePhpLanguageDetector(supportedPhpVersions: new[] { PhpVersions.Php73Version });
            var repo = new MemorySourceRepo();
            var version = "0";
            repo.AddFile("{\"require\":{\"php\":\"" + version + "\"}}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => detector.Detect(context));
            Assert.Equal(
                $"Platform 'php' version '{version}' is unsupported. Supported versions: {PhpVersions.Php73Version}",
                exception.Message);
        }

        [Fact]
        public void Detect_ReturnsVersionFromCliSwitch_EvenIfOptions_AndComposerFileHasVersionSpecified()
        {
            // Arrange
            var detector = CreatePhpLanguageDetector(
                supportedPhpVersions: new[] { "7.3.14", "7.2.5", "5.6.0", "100.100.100" },
                defaultVersion: "7.3.14",
                new PhpScriptGeneratorOptions { PhpVersion = "7.2.5" });
            var repo = new MemorySourceRepo();
            var version = "5.6.0";
            repo.AddFile("{\"require\":{\"php\":\"" + version + "\"}}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);
            context.PhpVersion = "100.100.100";

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("100.100.100", result.LanguageVersion);
        }

        [Fact]
        public void Detect_ReturnsVersion_FromOptions_EvenIfComposerFileHasVersionSpecified()
        {
            // Arrange
            var detector = CreatePhpLanguageDetector(
                supportedPhpVersions: new[] { "7.3.14", "7.2.5", "5.6.0" },
                defaultVersion: "7.3.14",
                new PhpScriptGeneratorOptions { PhpVersion = "7.2.5" });
            var repo = new MemorySourceRepo();
            var version = "5.6.0";
            repo.AddFile("{\"require\":{\"php\":\"" + version + "\"}}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("7.2.5", result.LanguageVersion);
        }

        [Fact]
        public void Detect_ReturnsVersion_FromComposerFile_IfOptionsDoesNotHaveValue()
        {
            // Arrange
            var detector = CreatePhpLanguageDetector(
                supportedPhpVersions: new[] { "7.3.14", "7.2.5", "5.6.0" },
                defaultVersion: "7.3.14",
                new PhpScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            var version = "5.6.0";
            repo.AddFile("{\"require\":{\"php\":\"" + version + "\"}}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("5.6.0", result.LanguageVersion);
        }

        [Fact]
        public void Detect_ReturnsVersion_FromVersionProvider_IfNoVersionFoundInComposerFile_OrOptions()
        {
            // Arrange
            var detector = CreatePhpLanguageDetector(
                supportedPhpVersions: new[] { "7.3.14", "7.2.5", "5.6.0" },
                defaultVersion: "7.3.14",
                new PhpScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            repo.AddFile("{}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("7.3.14", result.LanguageVersion);
        }

        [Theory]
        [InlineData("invalid json")]
        [InlineData("{\"data\": \"valid but meaningless\"}")]
        public void Detect_ReturnsResult_WithPhpDefaultRuntimeVersion_WithComposerFile(string composerFileContent)
        {
            // Arrange
            var detector = CreatePhpLanguageDetector(supportedPhpVersions: new[] { PhpVersions.Php73Version });
            var repo = new MemorySourceRepo();
            repo.AddFile(composerFileContent, PhpConstants.ComposerFileName);
            repo.AddFile("<?php echo true; ?>", "foo.php");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PhpName, result.Language);
            Assert.Equal(PhpVersions.Php73Version, result.LanguageVersion);
        }

        private BuildScriptGeneratorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private PhpLanguageDetector CreatePhpLanguageDetector(string[] supportedPhpVersions)
        {
            return CreatePhpLanguageDetector(
                supportedPhpVersions,
                defaultVersion: null,
                new PhpScriptGeneratorOptions());
        }

        private PhpLanguageDetector CreatePhpLanguageDetector(
            string[] supportedPhpVersions,
            string defaultVersion,
            PhpScriptGeneratorOptions options)
        {
            options = options ?? new PhpScriptGeneratorOptions();

            return new PhpLanguageDetector(
                Options.Create(options),
                new TestPhpVersionProvider(supportedPhpVersions, defaultVersion),
                NullLogger<PhpLanguageDetector>.Instance,
                new DefaultStandardOutputWriter());
        }

        private class TestPhpVersionProvider : IPhpVersionProvider
        {
            private readonly string[] _supportedPhpVersions;
            private readonly string _defaultVersion;

            public TestPhpVersionProvider(string[] supportedPhpVersions, string defaultVersion)
            {
                _supportedPhpVersions = supportedPhpVersions;
                _defaultVersion = defaultVersion;
            }

            public PlatformVersionInfo GetVersionInfo()
            {
                var version = _defaultVersion;
                if (version == null)
                {
                    version = PhpVersions.Php73Version;
                }

                return PlatformVersionInfo.CreateOnDiskVersionInfo(_supportedPhpVersions, version);
            }
        }
    }
}
