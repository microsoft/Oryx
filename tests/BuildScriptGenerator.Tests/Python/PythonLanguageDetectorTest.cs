// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Python
{
    public class PythonLanguageDetectorTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRoot;

        public PythonLanguageDetectorTest(TestTempDirTestFixture testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }

        [Fact]
        public void Detect_ReturnsNull_WhenSourceDirectoryIsEmpty()
        {
            // Arrange
            var detector = CreatePythonLanguageDetector(
                supportedPythonVersions: new[] { Common.PythonVersions.Python37Version });
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            // No files in source directory
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReutrnsNull_WhenRequirementsFileDoesNotExist()
        {
            // Arrange
            var detector = CreatePythonLanguageDetector(
                supportedPythonVersions: new[] { Common.PythonVersions.Python37Version });
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
            var detector = CreatePythonLanguageDetector(
                supportedPythonVersions: new[] { Common.PythonVersions.Python37Version });
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            // No files with '.py' or no runtime.txt file
            IOHelpers.CreateFile(sourceDir, "requirements.txt content", PythonConstants.RequirementsFileName);
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
            var detector = CreatePythonLanguageDetector(
                supportedPythonVersions: new[] { Common.PythonVersions.Python37Version });
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            // No file with a '.py' extension
            IOHelpers.CreateFile(sourceDir, "", PythonConstants.RequirementsFileName);
            IOHelpers.CreateFile(sourceDir, $"python-{Common.PythonVersions.Python37Version}", "runtime.txt");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("python", result.Language);
            Assert.Equal(Common.PythonVersions.Python37Version, result.LanguageVersion);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedPythonVersion_FoundInRuntimeFile()
        {
            // Arrange
            var badVersion = "100.100.100";
            var detector = CreatePythonLanguageDetector(
                supportedPythonVersions: new[] { Common.PythonVersions.Python37Version });
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "", PythonConstants.RequirementsFileName);
            IOHelpers.CreateFile(sourceDir, "python-" + badVersion, PythonConstants.RuntimeFileName);
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => detector.Detect(repo));
            Assert.Equal(
                $"Platform 'python' version '{badVersion}' is unsupported. " +
                $"Supported versions: {Common.PythonVersions.Python37Version}",
                exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("foo")]
        [InlineData("python")]
        public void Detect_ReutrnsNull_WhenRuntimeTextFileExists_ButDoesNotTextInExpectedFormat(string fileContent)
        {
            // Arrange
            var detector = CreatePythonLanguageDetector(
                supportedPythonVersions: new[] { Common.PythonVersions.Python37Version });
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "", PythonConstants.RequirementsFileName);
            IOHelpers.CreateFile(sourceDir, fileContent, "runtime.txt");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsResult_WithPythonDefaultVersion_WhenNoRuntimeTextFileExists()
        {
            // Arrange
            var detector = CreatePythonLanguageDetector(
                supportedPythonVersions: new[] { Common.PythonVersions.Python37Version });
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "content", PythonConstants.RequirementsFileName);
            IOHelpers.CreateFile(sourceDir, "foo.py content", "foo.py");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("python", result.Language);
            Assert.Equal(Common.PythonVersions.Python37Version, result.LanguageVersion);
        }

        private PythonLanguageDetector CreatePythonLanguageDetector(string[] supportedPythonVersions)
        {
            return CreatePythonLanguageDetector(supportedPythonVersions, new TestEnvironment());
        }

        private PythonLanguageDetector CreatePythonLanguageDetector(
            string[] supportedPythonVersions,
            IEnvironment environment)
        {
            var optionsSetup = new PythonScriptGeneratorOptionsSetup(environment);
            var options = new PythonScriptGeneratorOptions();
            optionsSetup.Configure(options);

            return new PythonLanguageDetector(
                Options.Create(options),
                new TestPythonVersionProvider(supportedPythonVersions),
                NullLogger<PythonLanguageDetector>.Instance);
        }

        private class TestPythonVersionProvider : IPythonVersionProvider
        {
            public TestPythonVersionProvider(string[] supportedPythonVersions)
            {
                SupportedPythonVersions = supportedPythonVersions;
            }

            public IEnumerable<string> SupportedPythonVersions { get; }
        }
    }
}
