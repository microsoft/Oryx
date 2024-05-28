// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Golang;
using Microsoft.Oryx.Detector.Golang;
using Microsoft.Oryx.Tests.Common;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Golang
{
    public class GolangPlatformTests : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRoot;

        public GolangPlatformTests(TestTempDirTestFixture testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }
        
        [Fact]
        public void Detect_Throws_WhenUnsupportedGoVersion_ReturnedByDetector()
        {
            // Arrange
            var detectedVersion = "0";
            var supportedVersion = "1.17";
            var platform = CreateGolangPlatform(
                supportedGolangVersions: new[] { supportedVersion },
                defaultVersion: supportedVersion,
                detectedVersion: detectedVersion);
            var context = CreateContext();

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => platform.Detect(context));
            Assert.Equal(
                $"Platform 'golang' version '{detectedVersion}' is unsupported. " +
                $"Supported versions: {supportedVersion}",
                exception.Message);
        }

        [Fact]
        public void Detect_ReturnsDefaultVersion_IfNoVersionFoundReturnedByDetector_OrOptions()
        {
            // Arrange
            var supportedVersion = "1.17";
            var platform = CreateGolangPlatform(
                supportedGolangVersions: new[] { supportedVersion },
                defaultVersion: supportedVersion,
                detectedVersion: null);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(GolangConstants.PlatformName, result.Platform);
            Assert.Equal(supportedVersion, result.PlatformVersion);
        }

        [Theory]
        [InlineData(null, "1.17", null, "1.17")]
        [InlineData(null, "1.17", "1.16", "1.17")]
        [InlineData(null, null, "1.16", "1.16")]
        [InlineData("1.18", "1.17", "1.16", "1.18")]
        public void Detect_ReturnsExpectedVersion_BasedOnHierarchy(
            string detectedVersion,
            string envVarDefaultVersion,
            string detectedDefaultVersion,
            string expectedSdkVersion)
        {
            // Arrange
            var context = CreateContext();
            var options = new GolangScriptGeneratorOptions();
            options.DefaultVersion = envVarDefaultVersion;
            var platform = CreateGolangPlatform(
                detectedVersion: detectedVersion,
                defaultVersion: detectedDefaultVersion,
                golangScriptGeneratorOptions: options,
                supportedGolangVersions: new[] { detectedVersion, detectedDefaultVersion, envVarDefaultVersion });

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(GolangConstants.PlatformName, result.Platform);
            Assert.Equal(expectedSdkVersion, result.PlatformVersion);
        }

        [Fact]
        public void GeneratedSnippet_HasInstallationScript()
        {
            // Arrange
            var expectedScript = "test-script";
            var commonOptions = new BuildScriptGeneratorOptions();
            commonOptions.EnableDynamicInstall = true;
            var golangPlatform = CreateGolangPlatform(
                commonOptions: commonOptions,
                isGolangVersionAlreadyInstalled: false,
                golangInstallationScript: expectedScript);
            var repo = new MemorySourceRepo();
            repo.AddFile("{}", GolangConstants.GoModFileName);
            var context = CreateContext(repo);
            var detectedResult = new GolangPlatformDetectorResult
            {
                Platform = GolangConstants.PlatformName,
                PlatformVersion = "1.17",
            };

            // Act
            var actualScriptSnippet = golangPlatform.GetInstallerScriptSnippet(context, detectedResult);

            // Assert
            Assert.NotNull(actualScriptSnippet);
            Assert.Contains(expectedScript, actualScriptSnippet);
        }

        private GolangPlatform CreateGolangPlatform(
            string[] supportedGolangVersions = null,
            string defaultVersion = null,
            string detectedVersion = null,
            BuildScriptGeneratorOptions commonOptions = null,
            GolangScriptGeneratorOptions golangScriptGeneratorOptions = null,
            bool? isGolangVersionAlreadyInstalled = null,
            string golangInstallationScript = null)
        {
            commonOptions = commonOptions ?? new BuildScriptGeneratorOptions();
            golangScriptGeneratorOptions = golangScriptGeneratorOptions ?? new GolangScriptGeneratorOptions();
            isGolangVersionAlreadyInstalled = isGolangVersionAlreadyInstalled ?? true;
            golangInstallationScript = golangInstallationScript ?? "default-golang-installation-script";
            var versionProvider = new TestGolangVersionProvider(supportedGolangVersions, defaultVersion);
            var detector = new TestGolangPlatformDetector(detectedVersion: detectedVersion);
            var golangInstaller = new TestGolangPlatformInstaller(
                Options.Create(commonOptions),
                isGolangVersionAlreadyInstalled.Value,
                golangInstallationScript);
            return new TestGolangPlatform(
                Options.Create(golangScriptGeneratorOptions),
                Options.Create(commonOptions),
                versionProvider,
                NullLogger<TestGolangPlatform>.Instance,
                detector,
                golangInstaller, 
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

        private class TestGolangPlatform : GolangPlatform
        {
            public TestGolangPlatform(
                IOptions<GolangScriptGeneratorOptions> golangScriptGeneratorOptions,
                IOptions<BuildScriptGeneratorOptions> commonOptions,
                IGolangVersionProvider goVersionProvider,
                ILogger<GolangPlatform> logger,
                IGolangPlatformDetector detector,
                GolangPlatformInstaller golangInstaller,
                TelemetryClient telemetryClient)
                : base(
                      golangScriptGeneratorOptions,
                      commonOptions,
                      goVersionProvider,
                      logger,
                      detector,
                      golangInstaller,
                      telemetryClient)
            {
            }
        }

        private class TestGolangPlatformInstaller : GolangPlatformInstaller
        {
            private readonly bool _isVersionAlreadyInstalled;
            private readonly string _installerScript;

            public TestGolangPlatformInstaller(
                IOptions<BuildScriptGeneratorOptions> commonOptions,
                bool isVersionAlreadyInstalled,
                string installerScript)
                : base(commonOptions, NullLoggerFactory.Instance)
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
