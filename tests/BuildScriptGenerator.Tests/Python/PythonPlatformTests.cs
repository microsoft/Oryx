﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Golang;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Python;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Python
{
    public class PythonPlatformTests : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRoot;

        public PythonPlatformTests(TestTempDirTestFixture testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }

        [Fact]
        public void GeneratedSnippet_DoesNotHaveInstallScript_IfDynamicInstallIsDisabled()
        {
            // Arrange
            var pythonScriptGeneratorOptions = new PythonScriptGeneratorOptions();
            var commonOptions = new BuildScriptGeneratorOptions() { EnableDynamicInstall = false };
            var installerScriptSnippet = "##INSTALLER_SCRIPT##";
            var versionProvider = new TestPythonVersionProvider(new[] { "3.7.5", "3.8.0" }, defaultVersion: "3.7.5");
            var platformInstaller = new TestPythonPlatformInstaller(
                isVersionAlreadyInstalled: false,
                installerScript: installerScriptSnippet,
                Options.Create(commonOptions),
                NullLoggerFactory.Instance);
            var platform = CreatePlatform(
                versionProvider,
                platformInstaller,
                commonOptions,
                pythonScriptGeneratorOptions);
            var repo = new MemorySourceRepo();
            repo.AddFile("", PythonConstants.RequirementsFileName);
            repo.AddFile("print(1)", "bla.py");
            var context = new BuildScriptGeneratorContext { SourceRepo = repo };
            var detectorResult = new PythonPlatformDetectorResult
            {
                Platform = PythonConstants.PlatformName,
                PlatformVersion = "3.7.5",
            };

            // Act
            var snippet = platform.GetInstallerScriptSnippet(context, detectorResult);

            // Assert
            Assert.Null(snippet);
        }

        [Fact]
        public void GeneratedSnippet_HasInstallationScript_IfDynamicInstallIsEnabled()
        {
            // Arrange
            var pythonScriptGeneratorOptions = new PythonScriptGeneratorOptions();
            var commonOptions = new BuildScriptGeneratorOptions() { EnableDynamicInstall = true };
            var installerScriptSnippet = "##INSTALLER_SCRIPT##";
            var versionProvider = new TestPythonVersionProvider(new[] { "3.7.5", "3.8.0" }, defaultVersion: "3.7.5");
            var platformInstaller = new TestPythonPlatformInstaller(
                isVersionAlreadyInstalled: false,
                installerScript: installerScriptSnippet,
                Options.Create(commonOptions),
                NullLoggerFactory.Instance);
            var platform = CreatePlatform(
                versionProvider,
                platformInstaller,
                commonOptions,
                pythonScriptGeneratorOptions);
            var repo = new MemorySourceRepo();
            repo.AddFile("", PythonConstants.RequirementsFileName);
            repo.AddFile("print(1)", "bla.py");
            var context = new BuildScriptGeneratorContext { SourceRepo = repo };
            var detectorResult = new PythonPlatformDetectorResult
            {
                Platform = PythonConstants.PlatformName,
                PlatformVersion = "3.7.5",
            };

            // Act
            var snippet = platform.GetInstallerScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(installerScriptSnippet, snippet);
        }

        [Fact]
        public void GeneratedSnippet_DoesNotHaveInstallScript_IfVersionIsAlreadyPresentOnDisk()
        {
            // Arrange
            var pythonScriptGeneratorOptions = new PythonScriptGeneratorOptions();
            var commonOptions = new BuildScriptGeneratorOptions() { EnableDynamicInstall = true };
            var installerScriptSnippet = "##INSTALLER_SCRIPT##";
            var versionProvider = new TestPythonVersionProvider(new[] { "3.7.5", "3.8.0" }, defaultVersion: "3.7.5");
            var platformInstaller = new TestPythonPlatformInstaller(
                isVersionAlreadyInstalled: true,
                installerScript: installerScriptSnippet,
                Options.Create(commonOptions),
                NullLoggerFactory.Instance);
            var platform = CreatePlatform(
                versionProvider,
                platformInstaller,
                commonOptions,
                pythonScriptGeneratorOptions);
            var repo = new MemorySourceRepo();
            repo.AddFile("", PythonConstants.RequirementsFileName);
            repo.AddFile("print(1)", "bla.py");
            var context = new BuildScriptGeneratorContext { SourceRepo = repo };
            var detectorResult = new PythonPlatformDetectorResult
            {
                Platform = PythonConstants.PlatformName,
                PlatformVersion = "3.7.5",
            };

            // Act
            var snippet = platform.GetInstallerScriptSnippet(context, detectorResult);

            // Assert
            Assert.Null(snippet);
        }

        [Fact]
        public void GeneratedSnippet_HaveInstallScript_IfCustomRequirementsTxtPathSpecified()
        {
            // Arrange
            var pythonScriptGeneratorOptions = new PythonScriptGeneratorOptions() { CustomRequirementsTxtPath = "foo/requirements.txt" };
            var commonOptions = new BuildScriptGeneratorOptions() 
            { 
                EnableDynamicInstall = true,
                CustomRequirementsTxtPath = "foo/requirements.txt"
            };
            var installerScriptSnippet = "##INSTALLER_SCRIPT##";
            var versionProvider = new TestPythonVersionProvider(new[] { "3.7.5", "3.8.0" }, defaultVersion: "3.7.5");
            var platformInstaller = new TestPythonPlatformInstaller(
                isVersionAlreadyInstalled: false,
                installerScript: installerScriptSnippet,
                Options.Create(commonOptions),
                NullLoggerFactory.Instance);
            var platform = CreatePlatform(
                versionProvider,
                platformInstaller,
                commonOptions,
                pythonScriptGeneratorOptions);
            var repo = new MemorySourceRepo();
            repo.AddFile("", "foo/requirements.txt");
            repo.AddFile("print(1)", "bla.py");
            var context = new BuildScriptGeneratorContext { SourceRepo = repo };
            var detectorResult = new PythonPlatformDetectorResult
            {
                Platform = PythonConstants.PlatformName,
                PlatformVersion = "3.7.5",
                HasRequirementsTxtFile = true,
            };

            // Act
            var snippet = platform.GetInstallerScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(snippet);
        }

        [Fact]
        public void GeneratedScript_DoesNotUseVenv()
        {
            // Arrange
            var scriptGenerator = CreatePlatform();
            var repo = new MemorySourceRepo();
            repo.AddFile("", PythonConstants.RequirementsFileName);
            repo.AddFile("print(1)", "bla.py");
            var context = new BuildScriptGeneratorContext { SourceRepo = repo };
            var detectorResult = new PythonPlatformDetectorResult
            {
                Platform = PythonConstants.PlatformName,
                PlatformVersion = "3.7.5",
            };

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(snippet);
            Assert.Contains("Python Virtual Environment", snippet.BashBuildScriptSnippet);
            Assert.True(scriptGenerator.IsCleanRepo(repo));
        }

        [Theory]
        [InlineData(null, "bla.tar.gz")]
        [InlineData("tar-gz", "bla.tar.gz")]
        [InlineData("zip", "bla.zip")]
        public void ExlcudedDirs_DoesNotContainVirtualEnvDir_IfCompressVirtualEnv_IsEnabled(
            string compressOption,
            string compressedVirtualEnvFileName)
        {
            // Arrange
            var scriptGenerator = CreatePlatform();
            var repo = new MemorySourceRepo();
            repo.AddFile("", PythonConstants.RequirementsFileName);
            var venvName = "bla";
            var context = new BuildScriptGeneratorContext
            {
                SourceRepo = repo,
                Properties = new Dictionary<string, string> {
                    { "virtualenv_name", venvName },
                    { "compress_virtualenv", compressOption }
                }
            };

            // Act
            var excludedDirs = scriptGenerator.GetDirectoriesToExcludeFromCopyToBuildOutputDir(context);

            // Assert
            Assert.NotNull(excludedDirs);
            Assert.Contains(venvName, excludedDirs);
            Assert.DoesNotContain(compressedVirtualEnvFileName, excludedDirs);
        }

        [Fact]
        public void Detect_ReturnsDefaultVersion_IfNoVersionFoundFromApp_OrOptions()
        {
            // Arrange
            var expectedVersion = "1.2.3";
            var platform = CreatePlatform(defaultVersion: expectedVersion, detectedVersion: null);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PythonConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsVersionFromOptions_EvenIfDetectorReturnsAVersion()
        {
            // Arrange
            var expectedVersion = "1.2.3";
            var detectedVersion = "2.5.0";
            var pythonScriptGeneratorOptions = new PythonScriptGeneratorOptions()
            {
                PythonVersion = expectedVersion
            };
            var platform = CreatePlatform(
                supportedVersions: new[] { expectedVersion },
                defaultVersion: expectedVersion,
                detectedVersion: detectedVersion,
                pythonScriptGeneratorOptions: pythonScriptGeneratorOptions);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PythonConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("1.2")]
        public void Detect_ReturnsResult_WhenOnlyPartialVersionIsProvidedByDetector(string detectedVersion)
        {
            // Arrange
            var expectedVersion = "1.2.3";
            var defaultVersion = "3.4.5";
            var platform = CreatePlatform(
                supportedVersions: new[] { defaultVersion, expectedVersion },
                detectedVersion: detectedVersion,
                defaultVersion: defaultVersion);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PythonConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedPythonVersion_IsProvidedByDetector()
        {
            // Arrange
            var detectedVersion = "100.100.100";
            var supportedVersion = "1.2.3";
            var platform = CreatePlatform(
                supportedVersions: new[] { supportedVersion },
                defaultVersion: supportedVersion,
                detectedVersion: detectedVersion);
            var context = CreateContext();

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => platform.Detect(context));
            Assert.Equal(
                $"Platform 'python' version '{detectedVersion}' is unsupported. " +
                $"Supported versions: {supportedVersion}",
                exception.Message);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedPythonVersion_IsSetInOptions()
        {
            // Arrange
            var optionsVersion = "100.100.100";
            var detectedVersion = "1.2.3";
            var supportedVersion = "1.2.3";
            var pythonScriptGeneratorOptions = new PythonScriptGeneratorOptions()
            {
                PythonVersion = optionsVersion
            };
            var platform = CreatePlatform(
                supportedVersions: new[] { supportedVersion },
                defaultVersion: supportedVersion,
                detectedVersion: detectedVersion,
                pythonScriptGeneratorOptions: pythonScriptGeneratorOptions);
            var context = CreateContext();

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => platform.Detect(context));
            Assert.Equal(
                $"Platform 'python' version '{optionsVersion}' is unsupported. " +
                $"Supported versions: {supportedVersion}",
                exception.Message);

        }

        [Theory]
        [InlineData(null, "3.8.12", null, "3.8.12")]
        [InlineData(null, "3.8.12", "3.7.9", "3.8.12")]
        [InlineData(null, null, "3.7.9", "3.7.9")]
        [InlineData("3.9.7", "3.8.12", "3.7.9", "3.9.7")]
        public void Detect_ReturnsExpectedVersion_BasedOnHierarchy(
            string detectedVersion,
            string envVarDefaultVersion,
            string detectedDefaultVersion,
            string expectedSdkVersion)
        {
            // Arrange
            var context = CreateContext();
            var options = new PythonScriptGeneratorOptions();
            options.DefaultVersion = envVarDefaultVersion;
            var platform = CreatePlatform(
                detectedVersion: detectedVersion,
                defaultVersion: detectedDefaultVersion,
                pythonScriptGeneratorOptions: options,
                supportedVersions: new[] { detectedVersion, detectedDefaultVersion, envVarDefaultVersion }.Where(x => !x.IsNullOrEmpty()).ToArray());

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PythonConstants.PlatformName, result.Platform);
            Assert.Equal(expectedSdkVersion, result.PlatformVersion);
        }
        private PythonPlatform CreatePlatform(
            IPythonVersionProvider pythonVersionProvider,
            PythonPlatformInstaller platformInstaller,
            BuildScriptGeneratorOptions commonOptions = null,
            PythonScriptGeneratorOptions pythonScriptGeneratorOptions = null)
        {
            commonOptions = commonOptions ?? new BuildScriptGeneratorOptions();
            pythonScriptGeneratorOptions = pythonScriptGeneratorOptions ?? new PythonScriptGeneratorOptions();

            return new PythonPlatform(
                Options.Create(commonOptions),
                Options.Create(pythonScriptGeneratorOptions),
                pythonVersionProvider,
                NullLogger<PythonPlatform>.Instance,
                detector: null,
                platformInstaller);
        }

        private PythonPlatform CreatePlatform(
            string[] supportedVersions = null,
            string defaultVersion = null,
            string detectedVersion = null,
            BuildScriptGeneratorOptions commonOptions = null,
            PythonScriptGeneratorOptions pythonScriptGeneratorOptions = null)
        {
            supportedVersions = supportedVersions ?? new[] { defaultVersion };
            defaultVersion = defaultVersion ?? Common.PythonVersions.Python37Version;
            var versionProvider = new TestPythonVersionProvider(
                supportedPythonVersions: supportedVersions,
                defaultVersion: defaultVersion);
            commonOptions = commonOptions ?? new BuildScriptGeneratorOptions();
            pythonScriptGeneratorOptions = pythonScriptGeneratorOptions ?? new PythonScriptGeneratorOptions();
            var detector = new TestPythonPlatformDetector(detectedVersion: detectedVersion);
            return new PythonPlatform(
                Options.Create(commonOptions),
                Options.Create(pythonScriptGeneratorOptions),
                versionProvider,
                NullLogger<PythonPlatform>.Instance,
                detector,
                new PythonPlatformInstaller(Options.Create(commonOptions), NullLoggerFactory.Instance));
        }

        private BuildScriptGeneratorContext CreateContext(ISourceRepo sourceRepo = null)
        {
            sourceRepo = sourceRepo ?? new MemorySourceRepo();

            return new BuildScriptGeneratorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private class TestPythonPlatformInstaller : PythonPlatformInstaller
        {
            private readonly bool _isVersionAlreadyInstalled;
            private readonly string _installerScript;

            public TestPythonPlatformInstaller(
                bool isVersionAlreadyInstalled,
                string installerScript,
                IOptions<BuildScriptGeneratorOptions> commonOptions,
                ILoggerFactory loggerFactory)
                : base(commonOptions, loggerFactory)
            {
                _isVersionAlreadyInstalled = isVersionAlreadyInstalled;
                _installerScript = installerScript;
            }

            public override bool IsVersionAlreadyInstalled(string version)
            {
                return _isVersionAlreadyInstalled;
            }

            public override string GetInstallerScriptSnippet(string version)
            {
                return _installerScript;
            }
        }
    }
}