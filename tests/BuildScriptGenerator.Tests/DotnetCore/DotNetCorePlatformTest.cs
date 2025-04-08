// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Detector.DotNetCore;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.DotNetCore
{
    public class DotNetCorePlatformTest
    {
        [Fact]
        public void Detect_ThrowsUnsupportedException_ForUnknownNetCoreAppVersion()
        {
            // Arrange
            var detector = CreateDetector(detectedVersion: "0.0");
            var context = CreateContext();
            var platform = CreatePlatform(
                detector,
                defaultVersion: "2.1.1",
                supportedVersions: new Dictionary<string, string> { { "2.1.1", "3.1.3" } });

            // Act
            var exception = Assert.Throws<UnsupportedVersionException>(() => platform.Detect(context));

            // Assert
            Assert.Equal(
                $"Platform '{DotNetCoreConstants.PlatformName}' version '0.0' is unsupported. " +
                "Supported versions: 2.1.1",
                exception.Message);
        }

        [Theory]
        [InlineData("1.0", "1.0.14")]
        [InlineData("1.1", "1.1.15")]
        [InlineData("2.0", "2.0.9")]
        [InlineData("2.1", "2.1.15")]
        [InlineData("2.2", "2.2.8")]
        [InlineData("3.0", "3.0.2")]
        [InlineData("3.1", "3.1.2")]
        [InlineData("5.0", "5.0.0-rc.1.14955.1")]
        public void Detect_ReturnsExpectedMaximumSatisfyingPlatformVersion_ForTargetFrameworkVersions(
            string netCoreAppVersion,
            string expectedSdkVersion)
        {
            // Arrange
            var detector = CreateDetector(detectedVersion: netCoreAppVersion);
            var context = CreateContext();
            var platform = CreatePlatform(
                detector,
                defaultVersion: "1.5.0",
                supportedVersions: new Dictionary<string, string>
                {
                    { "1.5.0", "1.5.0" },
                    { "1.0.14", "1.0.14" },
                    { "1.1.15", "1.1.15" },
                    { "2.0.9", "2.0.9" },
                    { "2.1.15", "2.1.15" },
                    { "2.2.8", "2.2.8" },
                    { "3.0.2", "3.0.2" },
                    { "3.1.2", "3.1.2" },
                    { "5.0.0-rc.1.14955.1", "5.0.0-rc.1.14955.1"},
                });

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DotNetCoreConstants.PlatformName, result.Platform);
            Assert.Equal(expectedSdkVersion, result.PlatformVersion);
        }

        [Theory]
        [InlineData(null, "3.1", null, "3.1.2")]
        [InlineData(null, "3.1", "2.2.8", "3.1.2")]
        [InlineData(null, null, "2.2.8", "2.2.8")]
        [InlineData("3.0.2", "3.1", "2.2.8", "3.0.2")]
        public void Detect_ReturnsExpectedVersion_BasedOnHierarchy(
            string detectedVersion,
            string envVarDefaultVersion, 
            string detectedDefaultVersion,
            string expectedSdkVersion)
        {
            // Arrange
            var detector = CreateDetector(detectedVersion: detectedVersion);
            var context = CreateContext();
            var platform = CreatePlatform(
                detector,
                defaultVersion: detectedDefaultVersion,
                envVarDefaultVersion: envVarDefaultVersion,
                supportedVersions: new Dictionary<string, string>
                {
                    { "1.5.0", "1.5.0" },
                    { "1.0.14", "1.0.14" },
                    { "1.1.15", "1.1.15" },
                    { "2.0.9", "2.0.9" },
                    { "2.1.15", "2.1.15" },
                    { "2.2.8", "2.2.8" },
                    { "3.0.2", "3.0.2" },
                    { "3.1.2", "3.1.2" },
                    { "5.0.0-rc.1.14955.1", "5.0.0-rc.1.14955.1"},
                });

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DotNetCoreConstants.PlatformName, result.Platform);
            Assert.Equal(expectedSdkVersion, result.PlatformVersion);
        }

        private BuildScriptGeneratorContext CreateContext(ISourceRepo sourceRepo = null)
        {
            sourceRepo = sourceRepo ?? new MemorySourceRepo();

            return new BuildScriptGeneratorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private IDotNetCorePlatformDetector CreateDetector(
            string detectedVersion = null,
            string detectedProjectFile = null)
        {
            return new TestDotNetCorePlatformDetector(detectedVersion, detectedProjectFile);
        }

        private DotNetCorePlatform CreatePlatform(
            IDotNetCorePlatformDetector detector,
            Dictionary<string, string> supportedVersions = null,
            string defaultVersion = null,
            string envVarDefaultVersion = null)
        {
            defaultVersion = defaultVersion ?? DotNetCoreRunTimeVersions.NetCoreApp31;
            supportedVersions = supportedVersions ?? new Dictionary<string, string>
            {
                { defaultVersion, defaultVersion },
            };
            var versionProvider = new TestDotNetCoreVersionProvider(
                supportedVersions,
                defaultVersion);
            var externalSdkProvider = new ExternalSdkProvider(NullLogger<ExternalSdkProvider>.Instance);
            var commonOptions = new BuildScriptGeneratorOptions();
            var dotNetCoreScriptGeneratorOptions = new DotNetCoreScriptGeneratorOptions();
            dotNetCoreScriptGeneratorOptions.DefaultRuntimeVersion = envVarDefaultVersion;
            var installer = new DotNetCorePlatformInstaller(
                Options.Create(commonOptions),
                NullLoggerFactory.Instance);
            var globalJsonSdkResolver = new GlobalJsonSdkResolver(NullLogger<GlobalJsonSdkResolver>.Instance);
            return new TestDotNetCorePlatform(
                versionProvider,
                detector,
                Options.Create(commonOptions),
                Options.Create(dotNetCoreScriptGeneratorOptions),
                installer,
                globalJsonSdkResolver,
                externalSdkProvider,
                TelemetryClientHelper.GetTelemetryClient());
        }

        private class TestDotNetCorePlatform : DotNetCorePlatform
        {
            public TestDotNetCorePlatform(
                IDotNetCoreVersionProvider versionProvider,
                IDotNetCorePlatformDetector detector,
                IOptions<BuildScriptGeneratorOptions> cliOptions,
                IOptions<DotNetCoreScriptGeneratorOptions> dotNetCoreScriptGeneratorOptions,
                DotNetCorePlatformInstaller platformInstaller,
                GlobalJsonSdkResolver globalJsonSdkResolver,
                IExternalSdkProvider externalSdkProvider,
                TelemetryClient telemetryClient)
                : base(
                      versionProvider,
                      NullLogger<DotNetCorePlatform>.Instance,
                      detector,
                      cliOptions,
                      dotNetCoreScriptGeneratorOptions,
                      platformInstaller,
                      globalJsonSdkResolver,
                      externalSdkProvider,
                      telemetryClient)
            {
            }
        }
    }
}
