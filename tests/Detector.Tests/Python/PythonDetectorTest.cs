// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
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
        public void Detect_ReturnsResult_WhenRequirementsFileDoesNotExist_ButDotPyFilesExist()
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
            Assert.NotNull(result);
            Assert.Equal(PythonConstants.PlatformName, result.Platform);
            Assert.Null(result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsResult_WhenOnlyRequirementsTextFileExists_ButNoPyOrRuntimeFileExists()
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
            Assert.NotNull(result);
            Assert.Equal(PythonConstants.PlatformName, result.Platform);
            Assert.Null(result.PlatformVersion);
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
            Assert.Equal(PythonConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Theory]
        [InlineData("")]
        [InlineData("foo")]
        [InlineData(PythonConstants.PlatformName)]
        public void Detect_ReturnsNull_WhenOnlyRuntimeTextFileExists_ButDoesNotHaveTextInExpectedFormat(
            string fileContent)
        {
            // Arrange
            var detector = CreatePythonPlatformDetector();
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            // No files with '.py' or no requirements.txt file
            IOHelpers.CreateFile(sourceDir, fileContent, PythonConstants.RuntimeFileName);
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("3")]
        [InlineData("3.7")]
        [InlineData("3.7.5")]
        [InlineData("3.7.5b01")]
        public void Detect_ReturnsVersionFromRuntimeTextFile(string expectedVersion)
        {
            // Arrange
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
            Assert.Equal(PythonConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsResult_WhenDotPyFilesExistInSubFolders()
        {
            // Arrange
            var detector = CreatePythonPlatformDetector();
            var sourceDir = Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString("N")))
                .FullName;
            var subDirStr = Guid.NewGuid().ToString("N");
            var subDir = Directory.CreateDirectory(Path.Combine(sourceDir, subDirStr)).FullName;
            IOHelpers.CreateFile(subDir, "foo.py content", "foo.py");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PythonConstants.PlatformName, result.Platform);
            Assert.Equal(subDirStr, result.AppDirectory);
            Assert.Null(result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsNull_WhenDotPyFilesExistInSubFolders_AndDeepProbingIsDisabled()
        {
            // Arrange
            var options = new DetectorOptions
            {
                DisableRecursiveLookUp = true,
            };
            var detector = CreatePythonPlatformDetector(options);
            var sourceDir = Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString("N")))
                .FullName;
            var subDir = Directory.CreateDirectory(Path.Combine(sourceDir, Guid.NewGuid().ToString("N"))).FullName;
            IOHelpers.CreateFile(subDir, "foo.py content", "foo.py");
            IOHelpers.CreateFile(subDir, "foo==1.1", PythonConstants.RequirementsFileName);
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsResult_WhenRequirementsFileExistsAtRoot_AndDeepProbingIsDisabled()
        {
            // Arrange
            var options = new DetectorOptions
            {
                DisableRecursiveLookUp = true,
            };
            var detector = CreatePythonPlatformDetector(options);
            var sourceDir = Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString("N")))
                .FullName;
            IOHelpers.CreateFile(sourceDir, "foo==1.1", PythonConstants.RequirementsFileName);
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PythonConstants.PlatformName, result.Platform);
            Assert.Equal(string.Empty, result.AppDirectory);
            Assert.Null(result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsResult_WhenCustomRequirementsFileExists()
        {
            // Arrange
            var options = new DetectorOptions
            {
                CustomRequirementsTxtPath = "foo/requirements.txt",
            };
            var detector = CreatePythonPlatformDetector(options);
            var sourceDir = Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString("N")))
                .FullName;
            var subDirStr = "foo";
            var subDir = Directory.CreateDirectory(Path.Combine(sourceDir, subDirStr)).FullName;
            IOHelpers.CreateFile(subDir, "foo==1.1",  "requirements.txt");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            var pythonPlatformResult = Assert.IsType<PythonPlatformDetectorResult>(result);
            Assert.NotNull(pythonPlatformResult);
            Assert.Equal(PythonConstants.PlatformName, pythonPlatformResult.Platform);
            Assert.Equal(string.Empty, pythonPlatformResult.AppDirectory);
            Assert.True(pythonPlatformResult.HasRequirementsTxtFile);
            Assert.Null(pythonPlatformResult.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsNull_WhenCustomRequirementsFileDoesNotExist()
        {
            // Arrange
            var options = new DetectorOptions
            {
                CustomRequirementsTxtPath = "foo/requirements.txt",
            };
            var detector = CreatePythonPlatformDetector(options);
            var sourceDir = Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString("N")))
                .FullName;
            IOHelpers.CreateFile(sourceDir, "foo==1.1", "requirements.txt");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsResult_WhenOnlyJupyterNotebookFilesExist()
        {
            // Arrange
            var detector = CreatePythonPlatformDetector();
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(
                sourceDir,
                "notebook content",
                $"notebook1.{PythonConstants.JupyterNotebookFileExtensionName}");
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            var pythonPlatformResult = Assert.IsType<PythonPlatformDetectorResult>(result);
            Assert.Equal(PythonConstants.PlatformName, pythonPlatformResult.Platform);
            Assert.Null(pythonPlatformResult.PlatformVersion);
            Assert.True(pythonPlatformResult.HasJupyterNotebookFiles);
            Assert.False(pythonPlatformResult.HasCondaEnvironmentYmlFile);
        }

        [Fact]
        public void Detect_ReturnsResult_WhenPyprojectTomlFileExists()
        {
            // Arrange
            var detector = CreatePythonPlatformDetector();
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "", PythonConstants.PyprojectTomlFileName);
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            var pythonPlatformResult = Assert.IsType<PythonPlatformDetectorResult>(result);
            Assert.Equal(PythonConstants.PlatformName, pythonPlatformResult.Platform);
            Assert.Null(pythonPlatformResult.PlatformVersion);
            Assert.False(pythonPlatformResult.HasJupyterNotebookFiles);
            Assert.False(pythonPlatformResult.HasCondaEnvironmentYmlFile);
            Assert.True(pythonPlatformResult.HasPyprojectTomlFile);
        }

        [Theory]
        [InlineData(PythonConstants.CondaEnvironmentYmlFileName)]
        [InlineData(PythonConstants.CondaEnvironmentYamlFileName)]
        public void Detect_ReturnsResult_WhenValidCondaEnvironmentFileExists(string environmentFileName)
        {
            // Arrange
            var detector = CreatePythonPlatformDetector();
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "channels:", environmentFileName);
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            var pythonPlatformResult = Assert.IsType<PythonPlatformDetectorResult>(result);
            Assert.Equal(PythonConstants.PlatformName, pythonPlatformResult.Platform);
            Assert.Null(pythonPlatformResult.PlatformVersion);
            Assert.False(pythonPlatformResult.HasJupyterNotebookFiles);
            Assert.True(pythonPlatformResult.HasCondaEnvironmentYmlFile);
        }

        [Theory]
        [InlineData(PythonConstants.CondaEnvironmentYmlFileName)]
        [InlineData(PythonConstants.CondaEnvironmentYamlFileName)]
        public void Detect_ReturnsFalse_WhenValidCondaEnvironmentFileDoesNotExist(string environmentFileName)
        {
            // Arrange
            var detector = CreatePythonPlatformDetector();
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "foo:", environmentFileName);
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsResult_WithAllPropertiesPopulatedWithExpectedInformation()
        {
            // Arrange
            var expectedPythonVersion = "3.5.6";
            var detector = CreatePythonPlatformDetector();
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "channels:", PythonConstants.CondaEnvironmentYmlFileName);
            IOHelpers.CreateFile(sourceDir, "requirements.txt content", PythonConstants.RequirementsFileName);
            IOHelpers.CreateFile(
                sourceDir,
                "notebook content",
                $"notebook1.{PythonConstants.JupyterNotebookFileExtensionName}");
            IOHelpers.CreateFile(sourceDir, $"python-{expectedPythonVersion}", PythonConstants.RuntimeFileName);
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            var pythonPlatformResult = Assert.IsType<PythonPlatformDetectorResult>(result);
            Assert.Equal(PythonConstants.PlatformName, pythonPlatformResult.Platform);
            Assert.Equal(expectedPythonVersion, pythonPlatformResult.PlatformVersion);
            Assert.True(pythonPlatformResult.HasJupyterNotebookFiles);
            Assert.True(pythonPlatformResult.HasCondaEnvironmentYmlFile);
        }

        [Theory]
        [InlineData(PythonConstants.CondaEnvironmentYmlFileName)]
        [InlineData(PythonConstants.CondaEnvironmentYamlFileName)]
        public void Detect_ReturnsNull_ForMalformedCondaYamlFiles(string environmentFileName)
        {
            // Arrange
            var detector = CreatePythonPlatformDetector();
            var sourceDir = IOHelpers.CreateTempDir(_tempDirRoot);
            IOHelpers.CreateFile(sourceDir, "\"invalid text", environmentFileName);
            var repo = new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        private DetectorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new DetectorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private PythonDetector CreatePythonPlatformDetector(DetectorOptions options = null)
        {
            options = options ?? new DetectorOptions();
            return new PythonDetector(NullLogger<PythonDetector>.Instance, Options.Create(options));
        }
    }
}
