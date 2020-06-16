// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Php
{
    public class PhpPlatformTest
    {
        [Theory]
        [InlineData("invalid json")]
        [InlineData("{\"data\": \"valid but meaningless\"}")]
        public void Detect_ReturnsResult_WithPhpDefaultVersion_WithComposerFile(string composerFileContent)
        {
            // Arrange
            var expectedVersion = "10.10.10";
            var platform = CreatePhpPlatform(
                supportedPhpVersions: new[] { expectedVersion },
                defaultVersion: expectedVersion);
            var repo = new MemorySourceRepo();
            repo.AddFile(composerFileContent, PhpConstants.ComposerFileName);
            repo.AddFile("<?php echo true; ?>", "foo.php");
            var context = CreateContext(repo);

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Theory]
        [InlineData(new[] { "8.11.2", "5.6.9", "8.11.13" }, "5.6.9", ">5", "8.11.13")]
        [InlineData(new[] { "8.9.5", "8.11.2", "5.6.9", "8.11.13" }, "5.6.9", ">8.9", "8.11.13")]
        [InlineData(new[] { "8.9.5", "8.11.2", "11.12.0", "13.12.12", "8.11.13" }, "5.6.9", ">8.9 <13", "11.12.0")]
        public void Detect_ReturnsVersionFromComposerFile_UsingMaximumSatisfyingVersionRules(
            string[] supportedVersions,
            string defaultVersion,
            string versionInComposerFile,
            string expectedVersion)
        {
            // Arrange
            var platform = CreatePhpPlatform(
                supportedPhpVersions: supportedVersions,
                defaultVersion: defaultVersion);
            var repo = new MemorySourceRepo();
            repo.AddFile("{\"require\":{\"php\":\"" + versionInComposerFile + "\"}}", PhpConstants.ComposerFileName);
            repo.AddFile("<?php echo true; ?>", "foo.php");
            var context = CreateContext(repo);

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Theory]
        [InlineData(new[] { "8.11.2", "5.6.9", "8.11.13" }, "5.6.9", ">5", "8.11.13")]
        [InlineData(new[] { "8.9.5", "8.11.2", "5.6.9", "8.11.13" }, "5.6.9", ">8.9", "8.11.13")]
        [InlineData(new[] { "8.9.5", "8.11.2", "11.12.0", "13.12.12", "8.11.13" }, "5.6.9", ">8.9 <13", "11.12.0")]
        public void Detect_ReturnsVersionFromOptions_UsingMaximumSatisfyingVersionRules(
            string[] supportedVersions,
            string defaultVersion,
            string versionInOptions,
            string expectedVersion)
        {
            // Arrange
            var phpScriptGeneratorOptions = new PhpScriptGeneratorOptions
            {
                PhpVersion = versionInOptions
            };
            var platform = CreatePhpPlatform(
                supportedPhpVersions: supportedVersions,
                defaultVersion: defaultVersion,
                phpScriptGeneratorOptions: phpScriptGeneratorOptions);
            var repo = new MemorySourceRepo();
            repo.AddFile("{\"require\":{\"php\":\"1.1.1\"}}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsVersionFromOptions_EvenIfComposerFileHasVersionSpecified()
        {
            // Arrange
            var expectedVersion = "100.100.100";
            var versionInComposerFile = "5.6.0";
            var defaultVersion = "7.3.14";
            var phpScriptGeneratorOptions = new PhpScriptGeneratorOptions
            {
                PhpVersion = expectedVersion
            };
            var platform = CreatePhpPlatform(
                supportedPhpVersions: new[] { defaultVersion, "7.2.5", versionInComposerFile, expectedVersion },
                defaultVersion: defaultVersion,
                phpScriptGeneratorOptions: phpScriptGeneratorOptions);
            var repo = new MemorySourceRepo();
            repo.AddFile("{\"require\":{\"php\":\"" + versionInComposerFile + "\"}}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedPhpVersion_FoundInComposerFile()
        {
            // Arrange
            var unsupportedVersion = "0";
            var supportedVersion = "7.3.5";
            var platform = CreatePhpPlatform(
                supportedPhpVersions: new[] { supportedVersion },
                defaultVersion: supportedVersion);
            var repo = new MemorySourceRepo();
            repo.AddFile("{\"require\":{\"php\":\"" + unsupportedVersion + "\"}}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => platform.Detect(context));
            Assert.Equal(
                $"Platform 'php' version '{unsupportedVersion}' is unsupported. " +
                $"Supported versions: {supportedVersion}",
                exception.Message);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedPhpVersion_IsSetInOptions()
        {
            // Arrange
            var unsupportedVersion = "0";
            var supportedVersion = "7.3.5";
            var phpScriptGeneratorOptions = new PhpScriptGeneratorOptions
            {
                PhpVersion = unsupportedVersion
            };
            var platform = CreatePhpPlatform(
                supportedPhpVersions: new[] { supportedVersion },
                defaultVersion: supportedVersion,
                phpScriptGeneratorOptions: phpScriptGeneratorOptions);
            var repo = new MemorySourceRepo();
            repo.AddFile("{\"require\":{\"php\":\"" + supportedVersion + "\"}}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => platform.Detect(context));
            Assert.Equal(
                $"Platform 'php' version '{unsupportedVersion}' is unsupported. " +
                $"Supported versions: {supportedVersion}",
                exception.Message);
        }

        [Fact]
        public void Detect_ReturnsDefaultVersion_IfNoVersionFoundInComposerFile_OrOptions()
        {
            // Arrange
            var expectedVersion = "7.3.14";
            var platform = CreatePhpPlatform(
                supportedPhpVersions: new[] { "5.6.0", expectedVersion, "7.2.5" },
                defaultVersion: expectedVersion);
            var repo = new MemorySourceRepo();
            repo.AddFile("{}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        private PhpPlatform CreatePhpPlatform(
            string[] supportedPhpVersions,
            string defaultVersion,
            BuildScriptGeneratorOptions commonOptions = null,
            PhpScriptGeneratorOptions phpScriptGeneratorOptions = null)
        {
            commonOptions = commonOptions ?? new BuildScriptGeneratorOptions();
            phpScriptGeneratorOptions = phpScriptGeneratorOptions ?? new PhpScriptGeneratorOptions();
            var versionProvider = new TestPhpVersionProvider(supportedPhpVersions, defaultVersion);
            var detector = new PhpPlatformDetector(
                Options.Create(phpScriptGeneratorOptions),
                versionProvider,
                NullLogger<PhpPlatformDetector>.Instance,
                new DefaultStandardOutputWriter());
            return new TestPhpPlatform(
                Options.Create(phpScriptGeneratorOptions),
                Options.Create(commonOptions),
                versionProvider,
                NullLogger<TestPhpPlatform>.Instance,
                detector);
        }

        private BuildScriptGeneratorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private class TestPhpPlatform : PhpPlatform
        {
            public TestPhpPlatform(
                IOptions<PhpScriptGeneratorOptions> phpScriptGeneratorOptions,
                IOptions<BuildScriptGeneratorOptions> commonOptions,
                IPhpVersionProvider phpVersionProvider,
                ILogger<PhpPlatform> logger,
                PhpPlatformDetector detector)
                : base(phpScriptGeneratorOptions, commonOptions, phpVersionProvider, logger, detector)
            {
            }
        }
    }
}
