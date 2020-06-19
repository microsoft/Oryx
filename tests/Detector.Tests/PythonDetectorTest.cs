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
    public class PythonDetectorTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRoot;

        public PythonDetectorTest(TestTempDirTestFixture testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }

        [Fact]
        public void Detect_ReturnsNull_WhenSourceDirectoryIsEmpty()
        {
            // Arrange
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
        public void Detect_ReturnsNull_WhenRequirementsFileDoesNotExist()
        {
            // Arrange
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
        public void Detect_ReturnsNull_WhenRequirementsTextFileExists_ButNoPyOrRuntimeFileExists()
        {
            // Arrange
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
        public void Detect_ReturnsResult_WhenNoPyFileExists_ButRuntimeTextFileExists_HavingPythonVersionInIt()
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
