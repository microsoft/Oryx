// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Detector.Python;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.Detector.Tests.Python
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
            var detector = CreatePythonPlatformDetector();
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
            var detector = CreatePythonPlatformDetector();
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
            var detector = CreatePythonPlatformDetector();
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
            var detector = CreatePythonPlatformDetector();
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

      
        [Theory]
        [InlineData("")]
        [InlineData("foo")]
        [InlineData("python")]
        public void Detect_ReturnsNull_WhenRuntimeTextFileExists_ButDoesNotTextInExpectedFormat(string fileContent)
        {
            // Arrange
            var supportedVersion = "1.2.3";
            var detector = CreatePythonPlatformDetector();
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
        public void Detect_ReturnsNullVersion_WhenNoRuntimeTextFileExists()
        {
            // Arrange
            var expectedVersion = "3.8.2";
            var detector = CreatePythonPlatformDetector();
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
            Assert.Null(result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsNullVersion_IfNoVersionFoundFromApp_OrOptions()
        {
            // Arrange
            var expectedVersion = "3.8.2";
            var detector = CreatePythonPlatformDetector();
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
            Assert.Null(result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsVersionFromOptions_EvenIfRuntimeTextFileHasVersion()
        {
            // Arrange
            var expectedVersion = "1.2.3";
            var runtimeTextFileVersion = "2.5.0";
            var detector = CreatePythonPlatformDetector();
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
            var detector = CreatePythonPlatformDetector();
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

        private DetectorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new DetectorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private PythonDetector CreatePythonPlatformDetector()
        {

            return new PythonDetector(
                NullLogger<PythonDetector>.Instance);
        }
    }
}
