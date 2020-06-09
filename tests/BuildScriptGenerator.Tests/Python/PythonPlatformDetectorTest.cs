// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Python
{
    public class PythonPlatformDetectorTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRoot;

        public PythonPlatformDetectorTest(TestTempDirTestFixture testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }

        [Fact]
        public void Detect_ReturnsNull_WhenSourceDirectoryIsEmpty()
        {
            // Arrange
            var version = "100.100.100";
            var detector = CreatePythonPlatformDetector(
                supportedPythonVersions: new[] { version },
                defaultVersion: version,
                new PythonScriptGeneratorOptions());
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
            var detector = CreatePythonPlatformDetector(
                supportedPythonVersions: new[] { version },
                defaultVersion: version,
                new PythonScriptGeneratorOptions());
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
            var detector = CreatePythonPlatformDetector(
                supportedPythonVersions: new[] { version },
                defaultVersion: version,
                new PythonScriptGeneratorOptions());
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
            var detector = CreatePythonPlatformDetector(
                supportedPythonVersions: new[] { defaultVersion, expectedVersion },
                defaultVersion: defaultVersion,
                new PythonScriptGeneratorOptions());
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
            Assert.Equal("python", result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedPythonVersion_FoundInRuntimeFile()
        {
            // Arrange
            var unsupportedVersion = "100.100.100";
            var supportedVersion = "1.2.3";
            var detector = CreatePythonPlatformDetector(
                supportedPythonVersions: new[] { supportedVersion },
                defaultVersion: supportedVersion,
                new PythonScriptGeneratorOptions());
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
            var detector = CreatePythonPlatformDetector(
                supportedPythonVersions: new[] { supportedVersion },
                defaultVersion: supportedVersion,
                new PythonScriptGeneratorOptions());
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
            var detector = CreatePythonPlatformDetector(
                supportedPythonVersions: new[] { "100.100.100", "1.2.1", expectedVersion },
                defaultVersion: expectedVersion,
                new PythonScriptGeneratorOptions());
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "", PythonConstants.RequirementsFileName);
            IOHelpers.CreateFile(sourceDir, $"python-{runtimeTxtVersion}", PythonConstants.RuntimeFileName);
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("python", result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsResult_WhenOnlyMajorAndMinorVersion_AreSpecifiedInRuntimeTxtFile()
        {
            // Arrange
            var runtimeTxtVersion = "1.2";
            var expectedVersion = "1.2.3";
            var detector = CreatePythonPlatformDetector(
                supportedPythonVersions: new[] { "100.100.100", "1.2.1r", expectedVersion },
                defaultVersion: expectedVersion,
                new PythonScriptGeneratorOptions());
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "", PythonConstants.RequirementsFileName);
            IOHelpers.CreateFile(sourceDir, $"python-{runtimeTxtVersion}", PythonConstants.RuntimeFileName);
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("python", result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsResult_WithPythonDefaultVersion_WhenNoRuntimeTextFileExists()
        {
            // Arrange
            var expectedVersion = "1.2.3";
            var detector = CreatePythonPlatformDetector(
                supportedPythonVersions: new[] { "100.100.100", expectedVersion },
                defaultVersion: expectedVersion,
                new PythonScriptGeneratorOptions());
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "content", PythonConstants.RequirementsFileName);
            IOHelpers.CreateFile(sourceDir, "foo.py content", "foo.py");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("python", result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsDefaultVersionOfVersionProvider_IfNoVersionFoundFromApp_OrOptions()
        {
            // Arrange
            var expectedVersion = "1.2.3";
            var detector = CreatePythonPlatformDetector(
                supportedPythonVersions: new[] { "100.100.100", "2.5.0", expectedVersion },
                defaultVersion: expectedVersion,
                new PythonScriptGeneratorOptions());
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "", PythonConstants.RequirementsFileName);
            IOHelpers.CreateFile(sourceDir, "", "app.py");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("python", result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsVersionFromOptions_EvenIfRuntimeTextFileHasVersion()
        {
            // Arrange
            var expectedVersion = "1.2.3";
            var runtimeTextFileVersion = "2.5.0";
            var detector = CreatePythonPlatformDetector(
                supportedPythonVersions: new[] { "100.100.100", runtimeTextFileVersion, expectedVersion },
                defaultVersion: expectedVersion,
                new PythonScriptGeneratorOptions());
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "", PythonConstants.RequirementsFileName);
            IOHelpers.CreateFile(sourceDir, "", "app.py");
            IOHelpers.CreateFile(sourceDir, $"python-{runtimeTextFileVersion}", PythonConstants.RuntimeFileName);
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);
            context.ResolvedPythonVersion = expectedVersion;

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("python", result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsVersionFromRuntimeTextFile_IfOptionsValueIsNotPresent()
        {
            // Arrange
            var expectedVersion = "2.5.0";
            var detector = CreatePythonPlatformDetector(
                supportedPythonVersions: new[] { "100.100.100", expectedVersion, "1.2.3" },
                defaultVersion: expectedVersion,
                new PythonScriptGeneratorOptions());
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "", PythonConstants.RequirementsFileName);
            IOHelpers.CreateFile(sourceDir, "", "app.py");
            IOHelpers.CreateFile(sourceDir, $"python-{expectedVersion}", PythonConstants.RuntimeFileName);
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("python", result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        private BuildScriptGeneratorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private PythonPlatformDetector CreatePythonPlatformDetector(
            string[] supportedPythonVersions,
            string defaultVersion,
            PythonScriptGeneratorOptions pythonScriptGeneratorOptions)
        {
            pythonScriptGeneratorOptions = pythonScriptGeneratorOptions ?? new PythonScriptGeneratorOptions();

            return new PythonPlatformDetector(
                new TestPythonVersionProvider(supportedPythonVersions, defaultVersion),
                Options.Create(pythonScriptGeneratorOptions),
                NullLogger<PythonPlatformDetector>.Instance,
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
