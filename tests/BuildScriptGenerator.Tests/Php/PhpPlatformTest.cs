// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Castle.Core.Internal;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Golang;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Php;
using System.Linq;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Php
{
    public class PhpPlatformTest
    {
        [Theory]
        [InlineData("invalid json")]
        [InlineData("{\"data\": \"valid but meaningless\"}")]
        public void Detect_ReturnsResult_WithPhpDefaultVersion_WithComposerFile(string composerFileContent)
        {
            // Arrange
            var expectedVersion = "10.10.10";
            var platform = CreatePhpPlatform(
                supportedPhpVersions: new[] { expectedVersion },
                defaultVersion: expectedVersion);
            var repo = new MemorySourceRepo();
            repo.AddFile(composerFileContent, PhpConstants.ComposerFileName);
            repo.AddFile("<?php echo true; ?>", "foo.php");
            var context = CreateContext(repo);

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Theory]
        [InlineData(new[] { "8.11.2", "5.6.9", "8.11.13" }, "5.6.9", ">5", "8.11.13")]
        [InlineData(new[] { "8.9.5", "8.11.2", "5.6.9", "8.11.13" }, "5.6.9", ">8.9", "8.11.13")]
        [InlineData(new[] { "8.9.5", "8.11.2", "11.12.0", "13.12.12", "8.11.13" }, "5.6.9", ">8.9 <13", "11.12.0")]
        public void Detect_ReturnsVersionFromComposerFile_UsingMaximumSatisfyingVersionRules(
            string[] supportedVersions,
            string defaultVersion,
            string versionInComposerFile,
            string expectedVersion)
        {
            // Arrange
            var platform = CreatePhpPlatform(
                supportedPhpVersions: supportedVersions,
                defaultVersion: defaultVersion,
                detectedVersion: versionInComposerFile);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Theory]
        [InlineData(new[] { "8.11.2", "5.6.9", "8.11.13" }, "5.6.9", ">5", "8.11.13")]
        [InlineData(new[] { "8.9.5", "8.11.2", "5.6.9", "8.11.13" }, "5.6.9", ">8.9", "8.11.13")]
        [InlineData(new[] { "8.9.5", "8.11.2", "11.12.0", "13.12.12", "8.11.13" }, "5.6.9", ">8.9 <13", "11.12.0")]
        public void Detect_ReturnsVersionFromOptions_UsingMaximumSatisfyingVersionRules(
            string[] supportedVersions,
            string defaultVersion,
            string versionInOptions,
            string expectedVersion)
        {
            // Arrange
            var phpScriptGeneratorOptions = new PhpScriptGeneratorOptions
            {
                PhpVersion = versionInOptions
            };
            var platform = CreatePhpPlatform(
                supportedPhpVersions: supportedVersions,
                defaultVersion: defaultVersion,
                detectedVersion: "1.1.1",
                phpScriptGeneratorOptions: phpScriptGeneratorOptions);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Theory]
        [InlineData(new[] { "7.3.20RC1", "7.4.0beta4", "7.4.0RC6", "8.0.0alpha1" }, "7.4.0beta4", "7.4.0beta4")]
        [InlineData(new[] { "7.3.20RC1", "7.4.0beta4", "7.4.0RC6", "8.0.0alpha1" }, "7.3.20RC1", "7.3.20RC1")]
        [InlineData(new[] { "7.3.20RC1", "7.4.0beta4", "7.4.0RC6", "8.0.0alpha1" }, "8.0.0alpha1", "8.0.0alpha1")]
        public void Detect_ReturnsPreviewVersion_UsingMaximumSatisfyingVersionRules(
            string[] supportedVersions,
            string versionInComposerFile,
            string expectedVersion)
        {
            // Arrange
            var platform = CreatePhpPlatform(
                detectedVersion: versionInComposerFile,
                supportedPhpVersions: supportedVersions);
            var repo = new MemorySourceRepo();
            var context = CreateContext(repo);

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Theory]
        [InlineData(new[] { "7.3.20RC1", "7.4.0beta4", "7.4.0RC6", "8.0.0alpha1" }, "7.4.0beta4", "7.4.0beta4")]
        [InlineData(new[] { "7.3.20RC1", "7.4.0beta4", "7.4.0RC6", "8.0.0alpha1" }, "7.3.20RC1", "7.3.20RC1")]
        [InlineData(new[] { "7.3.20RC1", "7.4.0beta4", "7.4.0RC6", "8.0.0alpha1" }, "8.0.0alpha1", "8.0.0alpha1")]
        public void Detect_WhenPreviewPhpVersion_IsSetInOptions(string[] supportedVersions, string previewVersion, string expectedVersion)
        {
            // Arrange
            var phpScriptGeneratorOptions = new PhpScriptGeneratorOptions
            {
                PhpVersion = previewVersion
            };
            var platform = CreatePhpPlatform(
                supportedPhpVersions: supportedVersions,
                defaultVersion: previewVersion,
                phpScriptGeneratorOptions: phpScriptGeneratorOptions);
            var repo = new MemorySourceRepo();
            repo.AddFile("{\"require\":{\"php\":\"" + previewVersion + "\"}}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);

            // Act
            var result = platform.Detect(context);

            // Act & Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsVersionFromOptions_EvenIfDetectorReturnsVersion()
        {
            // Arrange
            var expectedVersion = "100.100.100";
            var detectedVersion = "5.6.0";
            var defaultVersion = "7.3.14";
            var phpScriptGeneratorOptions = new PhpScriptGeneratorOptions
            {
                PhpVersion = expectedVersion
            };
            var platform = CreatePhpPlatform(
                supportedPhpVersions: new[] { defaultVersion, "7.2.5", detectedVersion, expectedVersion },
                defaultVersion: defaultVersion,
                detectedVersion: detectedVersion,
                phpScriptGeneratorOptions: phpScriptGeneratorOptions);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedPhpVersion_ReturnedByDetector()
        {
            // Arrange
            var detectedVersion = "0";
            var supportedVersion = "7.3.5";
            var platform = CreatePhpPlatform(
                supportedPhpVersions: new[] { supportedVersion },
                defaultVersion: supportedVersion,
                detectedVersion: detectedVersion);
            var context = CreateContext();

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => platform.Detect(context));
            Assert.Equal(
                $"Platform 'php' version '{detectedVersion}' is unsupported. " +
                $"Supported versions: {supportedVersion}",
                exception.Message);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedPhpVersion_IsSetInOptions()
        {
            // Arrange
            var versionFromOptions = "0";
            var supportedVersion = "7.3.5";
            var phpScriptGeneratorOptions = new PhpScriptGeneratorOptions
            {
                PhpVersion = versionFromOptions
            };
            var platform = CreatePhpPlatform(
                supportedPhpVersions: new[] { supportedVersion },
                defaultVersion: supportedVersion,
                detectedVersion: supportedVersion,
                phpScriptGeneratorOptions: phpScriptGeneratorOptions);
            var context = CreateContext();

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => platform.Detect(context));
            Assert.Equal(
                $"Platform 'php' version '{versionFromOptions}' is unsupported. " +
                $"Supported versions: {supportedVersion}",
                exception.Message);
        }

        [Fact]
        public void Detect_ReturnsDefaultVersion_IfNoVersionFoundReturnedByDetector_OrOptions()
        {
            // Arrange
            var expectedVersion = "7.3.14";
            var platform = CreatePhpPlatform(
                supportedPhpVersions: new[] { "5.6.0", expectedVersion, "7.2.5" },
                defaultVersion: expectedVersion,
                detectedVersion: null);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void HasPhpInstallScript_IfDynamicInstallIsEnabled_AndPhpVersionIsNotAlreadyInstalled()
        {
            // Arrange
            var expectedScript = "test-script";
            var commonOptions = new BuildScriptGeneratorOptions();
            commonOptions.EnableDynamicInstall = true;
            var phpPlatform = CreatePhpPlatform(
                commonOptions: commonOptions,
                isPhpVersionAlreadyInstalled: false,
                phpInstallationScript: expectedScript);
            var repo = new MemorySourceRepo();
            repo.AddFile("{}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);
            var detectedResult = new PhpPlatformDetectorResult
            {
                Platform = PhpConstants.PlatformName,
                PlatformVersion = "7.3.5",
            };

            // Act
            var actualScriptSnippet = phpPlatform.GetInstallerScriptSnippet(context, detectedResult);

            // Assert
            Assert.NotNull(actualScriptSnippet);
            Assert.Contains(expectedScript, actualScriptSnippet);
        }

        [Fact]
        public void HasNoPhpInstallScript_IfDynamicInstallIsEnabled_AndPhpVersionIsAlreadyInstalled()
        {
            // Arrange
            var installationScript = "test-script";
            var commonOptions = new BuildScriptGeneratorOptions();
            commonOptions.EnableDynamicInstall = true;
            var phpPlatform = CreatePhpPlatform(
                commonOptions: commonOptions,
                isPhpVersionAlreadyInstalled: true,
                phpInstallationScript: installationScript);
            var repo = new MemorySourceRepo();
            repo.AddFile("{}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);
            var detectedResult = new PhpPlatformDetectorResult
            {
                Platform = PhpConstants.PlatformName,
                PlatformVersion = "7.3.5",
            };

            // Act
            var actualScriptSnippet = phpPlatform.GetInstallerScriptSnippet(context, detectedResult);

            // Assert
            Assert.Null(actualScriptSnippet);
        }

        [Fact]
        public void DoesNotHavePhpInstallScript_IfDynamicInstallNotEnabled_AndPhpVersionIsNotAlreadyInstalled()
        {
            // Arrange
            var installationScript = "test-script";
            var commonOptions = new BuildScriptGeneratorOptions();
            commonOptions.EnableDynamicInstall = false;
            var phpPlatform = CreatePhpPlatform(
                commonOptions: commonOptions,
                isPhpVersionAlreadyInstalled: false,
                phpInstallationScript: installationScript);
            var repo = new MemorySourceRepo();
            repo.AddFile("{}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);
            var detectedResult = new PhpPlatformDetectorResult
            {
                Platform = PhpConstants.PlatformName,
                PlatformVersion = "7.3.5",
            };

            // Act
            var actualScriptSnippet = phpPlatform.GetInstallerScriptSnippet(context, detectedResult);

            // Assert
            Assert.Null(actualScriptSnippet);
        }

        [Fact]
        public void HasPhpComposerInstallScript_IfDynamicInstallIsEnabled_AndPhpComposerVersionIsNotAlreadyInstalled()
        {
            // Arrange
            var expectedScript = "test-script";
            var commonOptions = new BuildScriptGeneratorOptions();
            commonOptions.EnableDynamicInstall = true;
            var phpPlatform = CreatePhpPlatform(
                commonOptions: commonOptions,
                isPhpComposerAlreadyInstalled: false,
                phpComposerInstallationScript: expectedScript);
            var repo = new MemorySourceRepo();
            repo.AddFile("{}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);
            var detectedResult = new PhpPlatformDetectorResult
            {
                Platform = PhpConstants.PlatformName,
                PlatformVersion = "7.3.5",
            };

            // Act
            var actualScriptSnippet = phpPlatform.GetInstallerScriptSnippet(context, detectedResult);

            // Assert
            Assert.NotNull(actualScriptSnippet);
            Assert.Contains(expectedScript, actualScriptSnippet);
        }

        [Fact]
        public void HasNoPhpComposerInstallScript_IfDynamicInstallIsEnabled_AndPhpComposerVersionIsAlreadyInstalled()
        {
            // Arrange
            var installationScript = "test-script";
            var commonOptions = new BuildScriptGeneratorOptions();
            commonOptions.EnableDynamicInstall = true;
            var phpPlatform = CreatePhpPlatform(
                commonOptions: commonOptions,
                isPhpComposerAlreadyInstalled: true,
                phpComposerInstallationScript: installationScript);
            var repo = new MemorySourceRepo();
            repo.AddFile("{}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);
            var detectedResult = new PhpPlatformDetectorResult
            {
                Platform = PhpConstants.PlatformName,
                PlatformVersion = "7.3.5",
            };

            // Act
            var actualScriptSnippet = phpPlatform.GetInstallerScriptSnippet(context, detectedResult);

            // Assert
            Assert.Null(actualScriptSnippet);
        }

        [Fact]
        public void DoesNotHavePhpComposerInstallScript_IfDynamicInstallNotEnabled_AndPhpComposerVersionIsNotAlreadyInstalled()
        {
            // Arrange
            var installationScript = "test-script";
            var commonOptions = new BuildScriptGeneratorOptions();
            commonOptions.EnableDynamicInstall = false;
            var phpPlatform = CreatePhpPlatform(
                commonOptions: commonOptions,
                isPhpComposerAlreadyInstalled: false,
                phpComposerInstallationScript: installationScript);
            var repo = new MemorySourceRepo();
            repo.AddFile("{}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);
            var detectedResult = new PhpPlatformDetectorResult
            {
                Platform = PhpConstants.PlatformName,
                PlatformVersion = "7.3.5",
            };

            // Act
            var actualScriptSnippet = phpPlatform.GetInstallerScriptSnippet(context, detectedResult);

            // Assert
            Assert.Null(actualScriptSnippet);
        }

        [Fact]
        public void HasPhpAndComposerInstallScript_IfDynamicInstallIsEnabled_AndPhpAndComposerVersionIsNotAlreadyInstalled()
        {
            // Arrange
            var expectedPhpScript = "test-php-installation-script";
            var expectedPhpComposerScript = "test-php-composer-installation-script";
            var commonOptions = new BuildScriptGeneratorOptions();
            commonOptions.EnableDynamicInstall = true;
            var phpPlatform = CreatePhpPlatform(
                commonOptions: commonOptions,
                isPhpVersionAlreadyInstalled: false,
                phpInstallationScript: expectedPhpScript,
                isPhpComposerAlreadyInstalled: false,
                phpComposerInstallationScript: expectedPhpComposerScript);
            var repo = new MemorySourceRepo();
            repo.AddFile("{}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);
            var detectedResult = new PhpPlatformDetectorResult
            {
                Platform = PhpConstants.PlatformName,
                PlatformVersion = "7.3.5",
            };

            // Act
            var actualScriptSnippet = phpPlatform.GetInstallerScriptSnippet(context, detectedResult);

            // Assert
            Assert.NotNull(actualScriptSnippet);
            Assert.Contains(expectedPhpScript, actualScriptSnippet);
            Assert.Contains(expectedPhpComposerScript, actualScriptSnippet);
        }

        [Theory]
        [InlineData(null, "7.4.30", null, "7.4.30")]
        [InlineData(null, "7.4.30", "7.3.1", "7.4.30")]
        [InlineData(null, null, "7.3.1", "7.3.1")]
        [InlineData("8.0.6", "7.4.30", "7.3.1", "8.0.6")]
        public void Detect_ReturnsExpectedVersion_BasedOnHierarchy(
            string detectedVersion,
            string envVarDefaultVersion,
            string detectedDefaultVersion,
            string expectedSdkVersion)
        {
            // Arrange
            var repo = new MemorySourceRepo();
            repo.AddFile("<?php echo true; ?>", "foo.php");
            var context = CreateContext(repo);
            var options = new PhpScriptGeneratorOptions();
            options.PhpDefaultVersion = envVarDefaultVersion;
            var platform = CreatePhpPlatform(
                detectedVersion: detectedVersion,
                defaultVersion: detectedDefaultVersion,
                phpScriptGeneratorOptions: options,
                supportedPhpVersions: new[] { detectedVersion, detectedDefaultVersion, envVarDefaultVersion }.Where(x => !string.IsNullOrEmpty(x)).ToArray());

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PlatformName, result.Platform);
            Assert.Equal(expectedSdkVersion, result.PlatformVersion);
        }

        private PhpPlatform CreatePhpPlatform(
            string[] supportedPhpVersions = null,
            string[] supportedPhpComposerVersions = null,
            string defaultVersion = null,
            string defaultComposerVersion = null,
            string detectedVersion = null,
            BuildScriptGeneratorOptions commonOptions = null,
            PhpScriptGeneratorOptions phpScriptGeneratorOptions = null,
            bool? isPhpVersionAlreadyInstalled = null,
            string phpInstallationScript = null,
            bool? isPhpComposerAlreadyInstalled = null,
            string phpComposerInstallationScript = null)
        {
            commonOptions = commonOptions ?? new BuildScriptGeneratorOptions();
            phpScriptGeneratorOptions = phpScriptGeneratorOptions ?? new PhpScriptGeneratorOptions();
            isPhpVersionAlreadyInstalled = isPhpVersionAlreadyInstalled ?? true;
            phpInstallationScript = phpInstallationScript ?? "default-php-installation-script";
            isPhpComposerAlreadyInstalled = isPhpComposerAlreadyInstalled ?? true;
            phpComposerInstallationScript = phpComposerInstallationScript ?? "default-php-composer-installation-script";
            var versionProvider = new TestPhpVersionProvider(supportedPhpVersions, defaultVersion);
            var externalSdkProvider = new ExternalSdkProvider(NullLogger<ExternalSdkProvider>.Instance);
            supportedPhpComposerVersions = supportedPhpComposerVersions ?? new[] { PhpVersions.ComposerDefaultVersion };
            defaultComposerVersion = defaultComposerVersion ?? PhpVersions.ComposerDefaultVersion;
            var composerVersionProvider = new TestPhpComposerVersionProvider(
                supportedPhpComposerVersions,
                defaultComposerVersion);
            var detector = new TestPhpPlatformDetector(detectedVersion: detectedVersion);
            var phpInstaller = new TestPhpPlatformInstaller(
                Options.Create(commonOptions),
                isPhpVersionAlreadyInstalled.Value,
                phpInstallationScript);
            var phpComposerInstaller = new TestPhpComposerInstaller(
                Options.Create(commonOptions),
                isPhpComposerAlreadyInstalled.Value,
                phpComposerInstallationScript);   
            return new TestPhpPlatform(
                Options.Create(phpScriptGeneratorOptions),
                Options.Create(commonOptions),
                versionProvider,
                composerVersionProvider,
                NullLogger<TestPhpPlatform>.Instance,
                detector,
                phpInstaller,
                phpComposerInstaller,
                externalSdkProvider,
                TelemetryClientHelper.GetTelemetryClient());
        }

        private BuildScriptGeneratorContext CreateContext(ISourceRepo sourceRepo = null)
        {
            sourceRepo = sourceRepo ?? new MemorySourceRepo();

            return new BuildScriptGeneratorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private class TestPhpPlatform : PhpPlatform
        {
            public TestPhpPlatform(
                IOptions<PhpScriptGeneratorOptions> phpScriptGeneratorOptions,
                IOptions<BuildScriptGeneratorOptions> commonOptions,
                IPhpVersionProvider phpVersionProvider,
                IPhpComposerVersionProvider phpComposerVersionProvider,
                ILogger<PhpPlatform> logger,
                IPhpPlatformDetector detector,
                PhpPlatformInstaller phpInstaller,
                PhpComposerInstaller phpComposerInstaller,
                IExternalSdkProvider externalSdkProvider,
                TelemetryClient telemetryClient)
                : base(
                      phpScriptGeneratorOptions,
                      commonOptions,
                      phpVersionProvider,
                      phpComposerVersionProvider,
                      logger,
                      detector,
                      phpInstaller,
                      phpComposerInstaller,
                      externalSdkProvider,
                      telemetryClient)
            {
            }
        }

        private class TestPhpPlatformInstaller : PhpPlatformInstaller
        {
            private readonly bool _isVersionAlreadyInstalled;
            private readonly string _installationScript;

            public TestPhpPlatformInstaller(
                IOptions<BuildScriptGeneratorOptions> commonOptions,
                bool isVersionAlreadyInstalled,
                string installationScript)
                : base(commonOptions, NullLoggerFactory.Instance)
            {
                _isVersionAlreadyInstalled = isVersionAlreadyInstalled;
                _installationScript = installationScript;
            }

            public override bool IsVersionAlreadyInstalled(string version)
            {
                return _isVersionAlreadyInstalled;
            }

            public override string GetInstallerScriptSnippet(string version, bool skipSdkBinaryDownload = false)
            {
                return _installationScript;
            }
        }

        private class TestPhpComposerInstaller : PhpComposerInstaller
        {
            private readonly bool _isVersionAlreadyInstalled;
            private readonly string _installationScript;

            public TestPhpComposerInstaller(
                IOptions<BuildScriptGeneratorOptions> commonOptions,
                bool isVersionAlreadyInstalled,
                string installationScript)
                : base(commonOptions, NullLoggerFactory.Instance)
            {
                _isVersionAlreadyInstalled = isVersionAlreadyInstalled;
                _installationScript = installationScript;
            }

            public override bool IsVersionAlreadyInstalled(string version)
            {
                return _isVersionAlreadyInstalled;
            }

            public override string GetInstallerScriptSnippet(string version, bool skipSdkBinaryDownload = false)
            {
                return _installationScript;
            }
        }

        private class TestPhpComposerVersionProvider : IPhpComposerVersionProvider
        {
            private readonly string[] _supportedPhpComposerVersions;
            private readonly string _defaultVersion;

            public TestPhpComposerVersionProvider(string[] supportedPhpComposerVersions, string defaultVersion)
            {
                _supportedPhpComposerVersions = supportedPhpComposerVersions;
                _defaultVersion = defaultVersion;
            }

            public PlatformVersionInfo GetVersionInfo()
            {
                return PlatformVersionInfo.CreateOnDiskVersionInfo(
                    _supportedPhpComposerVersions,
                    _defaultVersion);
            }
        }
    }
}
