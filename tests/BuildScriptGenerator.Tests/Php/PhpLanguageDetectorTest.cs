// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
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

            // Act
            var result = detector.Detect(repo);

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

            // Act
            var result = detector.Detect(repo);

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

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => detector.Detect(repo));
            Assert.Equal(
                $"Platform 'php' version '{version}' is unsupported. Supported versions: {PhpVersions.Php73Version}",
                exception.Message);
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

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PhpName, result.Language);
            Assert.Equal(PhpVersions.Php73Version, result.LanguageVersion);
        }

        private PhpLanguageDetector CreatePhpLanguageDetector(string[] supportedPhpVersions)
        {
            return CreatePhpLanguageDetector(supportedPhpVersions, new TestEnvironment());
        }

        private PhpLanguageDetector CreatePhpLanguageDetector(string[] supportedPhpVersions, IEnvironment environment)
        {
            var optionsSetup = new PhpScriptGeneratorOptionsSetup(environment);
            var options = new PhpScriptGeneratorOptions();
            optionsSetup.Configure(options);

            return new PhpLanguageDetector(
                Options.Create(options),
                new TestPhpVersionProvider(supportedPhpVersions),
                NullLogger<PhpLanguageDetector>.Instance);
        }

        private class TestPhpVersionProvider : IPhpVersionProvider
        {
            public TestPhpVersionProvider(string[] supportedPhpVersions)
            {
                SupportedPhpVersions = supportedPhpVersions;
            }

            public IEnumerable<string> SupportedPhpVersions { get; }
        }
    }
}
