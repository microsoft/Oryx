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
    public class PhpPlatformDetectorTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRoot;

        public PhpPlatformDetectorTest(TestTempDirTestFixture testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }

        [Fact]
        public void Detect_ReturnsNull_WhenSourceDirectoryIsEmpty()
        {
            // Arrange
            var detector = CreatePhpPlatformDetector(supportedPhpVersions: new[] { PhpVersions.Php73Version });
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
            var detector = CreatePhpPlatformDetector(supportedPhpVersions: new[] { PhpVersions.Php73Version });
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
            var detector = CreatePhpPlatformDetector(supportedPhpVersions: new[] { PhpVersions.Php73Version });
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
        public void Detect_ReturnsVersionFromCliSwitch_EvenIfEnvironmentVariable_AndComposerFileHasVersionSpecified()
        {
            // Arrange
            var environment = new TestEnvironment();
            environment.Variables[PhpConstants.PhpRuntimeVersionEnvVarName] = "7.2.5";
            var detector = CreatePhpPlatformDetector(
                supportedPhpVersions: new[] { "7.3.14", "7.2.5", "5.6.0", "100.100.100" },
                defaultVersion: "7.3.14",
                environment);
            var repo = new MemorySourceRepo();
            var version = "5.6.0";
            repo.AddFile("{\"require\":{\"php\":\"" + version + "\"}}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);
            context.PhpVersion = "100.100.100";

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("100.100.100", result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsVersion_FromEnvironmentVariable_EvenIfComposerFileHasVersionSpecified()
        {
            // Arrange
            var environment = new TestEnvironment();
            environment.Variables[PhpConstants.PhpRuntimeVersionEnvVarName] = "7.2.5";
            var detector = CreatePhpPlatformDetector(
                supportedPhpVersions: new[] { "7.3.14", "7.2.5", "5.6.0" },
                defaultVersion: "7.3.14",
                environment);
            var repo = new MemorySourceRepo();
            var version = "5.6.0";
            repo.AddFile("{\"require\":{\"php\":\"" + version + "\"}}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("7.2.5", result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsVersion_FromComposerFile_IfEnvironmentVariableDoesNotHaveValue()
        {
            // Arrange
            var environment = new TestEnvironment();
            var detector = CreatePhpPlatformDetector(
                supportedPhpVersions: new[] { "7.3.14", "7.2.5", "5.6.0" },
                defaultVersion: "7.3.14",
                environment);
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

        [Fact]
        public void Detect_ReturnsVersion_FromVersionProvider_IfNoVersionFoundInComposerFile_OrEnvVariable()
        {
            // Arrange
            var environment = new TestEnvironment();
            var detector = CreatePhpPlatformDetector(
                supportedPhpVersions: new[] { "7.3.14", "7.2.5", "5.6.0" },
                defaultVersion: "7.3.14",
                environment);
            var repo = new MemorySourceRepo();
            repo.AddFile("{}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("7.3.14", result.PlatformVersion);
        }

        [Theory]
        [InlineData("invalid json")]
        [InlineData("{\"data\": \"valid but meaningless\"}")]
        public void Detect_ReturnsResult_WithPhpDefaultRuntimeVersion_WithComposerFile(string composerFileContent)
        {
            // Arrange
            var detector = CreatePhpPlatformDetector(supportedPhpVersions: new[] { PhpVersions.Php73Version });
            var repo = new MemorySourceRepo();
            repo.AddFile(composerFileContent, PhpConstants.ComposerFileName);
            repo.AddFile("<?php echo true; ?>", "foo.php");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PlatformName, result.Platform);
            Assert.Equal(PhpVersions.Php73Version, result.PlatformVersion);
        }

        private BuildScriptGeneratorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private PhpPlatformDetector CreatePhpPlatformDetector(string[] supportedPhpVersions)
        {
            return CreatePhpPlatformDetector(supportedPhpVersions, defaultVersion: null, new TestEnvironment());
        }

        private PhpPlatformDetector CreatePhpPlatformDetector(
            string[] supportedPhpVersions, 
            string defaultVersion,
            IEnvironment environment)
        {
            var optionsSetup = new PhpScriptGeneratorOptionsSetup(environment);
            var options = new PhpScriptGeneratorOptions();
            optionsSetup.Configure(options);

            return new PhpPlatformDetector(
                Options.Create(options),
                new TestPhpVersionProvider(supportedPhpVersions, defaultVersion),
                NullLogger<PhpPlatformDetector>.Instance,
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
