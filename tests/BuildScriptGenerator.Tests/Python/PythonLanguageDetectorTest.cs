// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Oryx.Tests.Infrastructure;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Python
{
    public class PythonLanguageDetectorTest : IClassFixture<TestTempDirTestFixure>
    {
        private readonly string _tempDirRoot;

        public PythonLanguageDetectorTest(TestTempDirTestFixure testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }

        [Fact]
        public void Detect_ReturnsNull_IfSourceDirectory_IsEmpty()
        {
            // Arrange
            var detector = CreatePythonLanguageDetector(supportedPythonVersions: new[] { Settings.Python37Version });
            var sourceDir = CreateNewDir();
            // No files in source directory
            var repo = new LocalSourceRepo(sourceDir);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReutrnsNull_WhenRequirementsTextFile_IsNotPresent()
        {
            // Arrange
            var detector = CreatePythonLanguageDetector(supportedPythonVersions: new[] { Settings.Python37Version });
            var sourceDir = CreateNewDir();
            CreateFile(sourceDir, "foo.py content", "foo.py");
            var repo = new LocalSourceRepo(sourceDir);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReutrnsNull_WhenRequirementsTextFileExists_ButNoPyOrRuntimeFileExists()
        {
            // Arrange
            var detector = CreatePythonLanguageDetector(supportedPythonVersions: new[] { Settings.Python37Version });
            var sourceDir = CreateNewDir();
            // No files with '.py' or no runtime.txt file
            CreateFile(sourceDir, "requirements.txt content", "requirements.txt");
            var repo = new LocalSourceRepo(sourceDir);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReutrnsResult_WhenNoPyFileExists_ButRuntimeTextFileExists_HavingPythonVersionInIt()
        {
            // Arrange
            var detector = CreatePythonLanguageDetector(supportedPythonVersions: new[] { Settings.Python37Version });
            var sourceDir = CreateNewDir();
            // No file with a '.py' extension
            CreateFile(sourceDir, "", "requirements.txt");
            CreateFile(sourceDir, $"python-{Settings.Python37Version}", "runtime.txt");
            var repo = new LocalSourceRepo(sourceDir);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("python", result.Language);
            Assert.Equal(Settings.Python37Version, result.LanguageVersion);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedPythonVersion_FoundInRuntimeTextfile()
        {
            // Arrange
            var detector = CreatePythonLanguageDetector(supportedPythonVersions: new[] { Settings.Python37Version });
            var sourceDir = CreateNewDir();
            CreateFile(sourceDir, "", "requirements.txt");
            CreateFile(sourceDir, "python-100.100.100", "runtime.txt");
            var repo = new LocalSourceRepo(sourceDir);

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => detector.Detect(repo));
            Assert.Equal(
                $"The target python version '100.100.100' is not supported. Supported versions are: {Settings.Python37Version}",
                exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("foo")]
        [InlineData("python")]
        public void Detect_ReutrnsNull_WhenRuntimeTextFileExists_ButDoesNotTextInExpectedFormat(string fileContent)
        {
            // Arrange
            var detector = CreatePythonLanguageDetector(supportedPythonVersions: new[] { Settings.Python37Version });
            var sourceDir = CreateNewDir();
            CreateFile(sourceDir, "", "requirements.txt");
            CreateFile(sourceDir, fileContent, "runtime.txt");
            var repo = new LocalSourceRepo(sourceDir);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsResult_WithPythonDefaultVersion_WhenNoRuntimeTextFileExists()
        {
            // Arrange
            var detector = CreatePythonLanguageDetector(supportedPythonVersions: new[] { Settings.Python37Version });
            var sourceDir = CreateNewDir();
            CreateFile(sourceDir, "content", "requirements.txt");
            CreateFile(sourceDir, "foo.py content", "foo.py");
            var repo = new LocalSourceRepo(sourceDir);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("python", result.Language);
            Assert.Equal(Settings.Python37Version, result.LanguageVersion);
        }

        private string CreateNewDir()
        {
            return Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString("N"))).FullName;
        }

        private void CreateFile(string sourceDir, string fileContent, params string[] filePaths)
        {
            File.WriteAllText(Path.Combine(sourceDir, Path.Combine(filePaths)), fileContent);
        }

        private PythonLanguageDetector CreatePythonLanguageDetector(string[] supportedPythonVersions)
        {
            return CreatePythonLanguageDetector(supportedPythonVersions, new TestEnvironemnt());
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

        private class TestEnvironemnt : IEnvironment
        {
            public Dictionary<string, string> Variables { get; } = new Dictionary<string, string>();

            public string GetEnvironmentVariable(string name)
            {
                if (Variables.TryGetValue(name, out var value))
                {
                    return value;
                }
                return null;
            }
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
