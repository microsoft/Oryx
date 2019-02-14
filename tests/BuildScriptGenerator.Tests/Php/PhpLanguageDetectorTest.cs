// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Php;
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
        public void Detect_ReturnsNull_IfSourceDirectory_IsEmpty()
        {
            // Arrange
            var detector = CreatePhpLanguageDetector(supportedPhpVersions: new[] { Common.PhpVersions.Php7Version });
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            // No files in source directory
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReutrnsNull_WhenRequirementsTextFile_IsNotPresent()
        {
            // Arrange
            var detector = CreatePhpLanguageDetector(supportedPhpVersions: new[] { Common.PhpVersions.Php7Version });
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "foo.py content", "foo.py");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReutrnsNull_WhenRequirementsTextFileExists_ButNoPyOrRuntimeFileExists()
        {
            // Arrange
            var detector = CreatePhpLanguageDetector(supportedPhpVersions: new[] { Common.PhpVersions.Php7Version });
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            // No files with '.py' or no runtime.txt file
            IOHelpers.CreateFile(sourceDir, "requirements.txt content", "requirements.txt");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReutrnsResult_WhenNoPyFileExists_ButRuntimeTextFileExists_HavingPythonVersionInIt()
        {
            // Arrange
            var detector = CreatePhpLanguageDetector(supportedPhpVersions: new[] { Common.PhpVersions.Php7Version });
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            // No file with a '.py' extension
            IOHelpers.CreateFile(sourceDir, "", "requirements.txt");
            IOHelpers.CreateFile(sourceDir, $"php-{Common.PhpVersions.Php7Version}", "runtime.txt");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("php", result.Language);
            Assert.Equal(Common.PhpVersions.Php7Version, result.LanguageVersion);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedPythonVersion_FoundInRuntimeTextfile()
        {
            // Arrange
            var detector = CreatePhpLanguageDetector(supportedPhpVersions: new[] { Common.PhpVersions.Php7Version });
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "", "requirements.txt");
            IOHelpers.CreateFile(sourceDir, "python-100.100.100", "runtime.txt");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => detector.Detect(repo));
            Assert.Equal(
                $"Target Python version '100.100.100' is unsupported. Supported versions are: {Common.PhpVersions.Php7Version}",
                exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("foo")]
        [InlineData("php")]
        public void Detect_ReutrnsNull_WhenRuntimeTextFileExists_ButDoesNotTextInExpectedFormat(string fileContent)
        {
            // Arrange
            var detector = CreatePhpLanguageDetector(supportedPhpVersions: new[] { Common.PhpVersions.Php7Version });
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "", "requirements.txt");
            IOHelpers.CreateFile(sourceDir, fileContent, "runtime.txt");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsResult_WithPhpDefaultVersion_WhenNoComposerFileExists()
        {
            // Arrange
            var detector = CreatePhpLanguageDetector(supportedPhpVersions: new[] { Common.PhpVersions.Php7Version });
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "content", "requirements.txt");
            IOHelpers.CreateFile(sourceDir, "foo.py content", "foo.py");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PhpName, result.Language);
            Assert.Equal(Common.PhpVersions.Php7Version, result.LanguageVersion);
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

            return new PhpLanguageDetector(Options.Create(options), new TestPhpVersionProvider(supportedPhpVersions), NullLogger<PhpLanguageDetector>.Instance);
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
