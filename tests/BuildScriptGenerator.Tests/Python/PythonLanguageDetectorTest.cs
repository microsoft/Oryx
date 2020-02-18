// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
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
            var version = "100.100.100";
            var detector = CreatePythonLanguageDetector(
                supportedPythonVersions: new[] { version },
                defaultVersion: version);
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            // No files in source directory
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReutrnsNull_WhenRequirementsFileDoesNotExist()
        {
            // Arrange
            var version = "100.100.100";
            var detector = CreatePythonLanguageDetector(
                supportedPythonVersions: new[] { version },
                defaultVersion: version);
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "foo.py content", "foo.py");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReutrnsNull_WhenRequirementsTextFileExists_ButNoPyOrRuntimeFileExists()
        {
            // Arrange
            var version = "100.100.100";
            var detector = CreatePythonLanguageDetector(
                supportedPythonVersions: new[] { version },
                defaultVersion: version);
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            // No files with '.py' or no runtime.txt file
            IOHelpers.CreateFile(sourceDir, "requirements.txt content", PythonConstants.RequirementsFileName);
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReutrnsResult_WhenNoPyFileExists_ButRuntimeTextFileExists_HavingPythonVersionInIt()
        {
            // Arrange
            var expectedVersion = "1000.1000.1000";
            var defaultVersion = "1000.1000.1001";
            var detector = CreatePythonLanguageDetector(
                supportedPythonVersions: new[] { defaultVersion, expectedVersion },
                defaultVersion: defaultVersion);
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            // No file with a '.py' extension
            IOHelpers.CreateFile(sourceDir, "", PythonConstants.RequirementsFileName);
            IOHelpers.CreateFile(sourceDir, $"python-{expectedVersion}", "runtime.txt");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("python", result.Language);
            Assert.Equal(expectedVersion, result.LanguageVersion);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedPythonVersion_FoundInRuntimeFile()
        {
            // Arrange
            var unsupportedVersion = "100.100.100";
            var supportedVersion = "1.2.3";
            var detector = CreatePythonLanguageDetector(
                supportedPythonVersions: new[] { supportedVersion },
                defaultVersion: supportedVersion);
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "", PythonConstants.RequirementsFileName);
            IOHelpers.CreateFile(sourceDir, "python-" + unsupportedVersion, PythonConstants.RuntimeFileName);
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => detector.Detect(context));
            Assert.Equal(
                $"Platform 'python' version '{unsupportedVersion}' is unsupported. " +
                $"Supported versions: {supportedVersion}",
                exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("foo")]
        [InlineData("python")]
        public void Detect_ReutrnsNull_WhenRuntimeTextFileExists_ButDoesNotTextInExpectedFormat(string fileContent)
        {
            // Arrange
            var supportedVersion = "1.2.3";
            var detector = CreatePythonLanguageDetector(
                supportedPythonVersions: new[] { supportedVersion },
                defaultVersion: supportedVersion);
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "", PythonConstants.RequirementsFileName);
            IOHelpers.CreateFile(sourceDir, fileContent, "runtime.txt");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsResult_WhenOnlyMajorVersion_IsSpecifiedInRuntimeTxtFile()
        {
            // Arrange
            var runtimeTxtVersion = "1";
            var expectedVersion = "1.2.3";
            var detector = CreatePythonLanguageDetector(
                supportedPythonVersions: new[] { "100.100.100", "1.2.1", expectedVersion },
                defaultVersion: expectedVersion);
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "", PythonConstants.RequirementsFileName);
            IOHelpers.CreateFile(sourceDir, $"python-{runtimeTxtVersion}", PythonConstants.RuntimeFileName);
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("python", result.Language);
            Assert.Equal(expectedVersion, result.LanguageVersion);
        }

        [Fact]
        public void Detect_ReturnsResult_WhenOnlyMajorAndMinorVersion_AreSpecifiedInRuntimeTxtFile()
        {
            // Arrange
            var runtimeTxtVersion = "1.2";
            var expectedVersion = "1.2.3";
            var detector = CreatePythonLanguageDetector(
                supportedPythonVersions: new[] { "100.100.100", "1.2.1r", expectedVersion },
                defaultVersion: expectedVersion);
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "", PythonConstants.RequirementsFileName);
            IOHelpers.CreateFile(sourceDir, $"python-{runtimeTxtVersion}", PythonConstants.RuntimeFileName);
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("python", result.Language);
            Assert.Equal(expectedVersion, result.LanguageVersion);
        }

        [Fact]
        public void Detect_ReturnsResult_WithPythonDefaultVersion_WhenNoRuntimeTextFileExists()
        {
            // Arrange
            var expectedVersion = "1.2.3";
            var detector = CreatePythonLanguageDetector(
                supportedPythonVersions: new[] { "100.100.100", expectedVersion },
                defaultVersion: expectedVersion);
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "content", PythonConstants.RequirementsFileName);
            IOHelpers.CreateFile(sourceDir, "foo.py content", "foo.py");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("python", result.Language);
            Assert.Equal(expectedVersion, result.LanguageVersion);
        }

        private BuildScriptGeneratorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private PythonLanguageDetector CreatePythonLanguageDetector(
            string[] supportedPythonVersions, string defaultVersion)
        {
            return CreatePythonLanguageDetector(supportedPythonVersions, defaultVersion, new TestEnvironment());
        }

        private PythonLanguageDetector CreatePythonLanguageDetector(
            string[] supportedPythonVersions,
            string defaultVersion,
            IEnvironment environment)
        {
            var optionsSetup = new PythonScriptGeneratorOptionsSetup(environment);
            var options = new PythonScriptGeneratorOptions();
            optionsSetup.Configure(options);

            return new PythonLanguageDetector(
                new TestPythonVersionProvider(supportedPythonVersions, defaultVersion),
                NullLogger<PythonLanguageDetector>.Instance,
                new DefaultStandardOutputWriter());
        }

        private class TestPythonVersionProvider : IPythonVersionProvider
        {
            private readonly string[] _supportedPythonVersions;
            private readonly string _defaultVersion;

            public TestPythonVersionProvider(string[] supportedPythonVersions, string defaultVersion)
            {
                _supportedPythonVersions = supportedPythonVersions;
                _defaultVersion = defaultVersion;
            }

            public PlatformVersionInfo GetVersionInfo()
            {
                return PlatformVersionInfo.CreateOnDiskVersionInfo(_supportedPythonVersions, _defaultVersion);
            }
        }
    }
}
