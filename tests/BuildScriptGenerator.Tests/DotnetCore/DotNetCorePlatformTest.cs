// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Detector.DotNetCore;
using Microsoft.Oryx.Tests.Common;
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

        [Fact]
        public void GetInstallerScript_UsesExternalAcrProvider_IfEnabled_AndSdkIsNotAlreadyInstalled()
        {
            // Arrange
            var externalAcrSdkProvider = new TestExternalAcrSdkProvider(returnValue: true);
            var commonOptions = new BuildScriptGeneratorOptions()
            {
                EnableDynamicInstall = true,
                EnableExternalAcrSdkProvider = true,
                DebianFlavor = OsTypes.DebianBookworm,
            };
            var detector = CreateDetector(detectedVersion: "3.1");
            var platform = CreatePlatformWithProviders(
                detector,
                commonOptions: commonOptions,
                sdkAlreadyInstalled: false,
                externalAcrSdkProvider: externalAcrSdkProvider);
            var context = CreateContext();
            var detectorResult = new DotNetCorePlatformDetectorResult
            {
                Platform = DotNetCoreConstants.PlatformName,
                PlatformVersion = "3.1.2",
                SdkVersion = "3.1.2",
            };

            // Act
            var snippet = platform.GetInstallerScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(TestDotNetCorePlatformInstaller.InstallerScriptWithSkipSdkBinaryDownload, snippet);
            Assert.True(externalAcrSdkProvider.RequestSdkAsyncCalled);
        }

        [Fact]
        public void GetInstallerScript_UsesDirectAcrProvider_IfEnabled_AndSdkIsNotAlreadyInstalled()
        {
            // Arrange
            var acrSdkProvider = new TestAcrSdkProvider(returnValue: true);
            var commonOptions = new BuildScriptGeneratorOptions()
            {
                EnableDynamicInstall = true,
                EnableAcrSdkProvider = true,
                DebianFlavor = OsTypes.DebianBookworm,
            };
            var detector = CreateDetector(detectedVersion: "3.1");
            var platform = CreatePlatformWithProviders(
                detector,
                commonOptions: commonOptions,
                sdkAlreadyInstalled: false,
                acrSdkProvider: acrSdkProvider,
                supportedVersions: new Dictionary<string, string>
                {
                    { "3.1.32", "3.1.426" },
                });
            var context = CreateContext();
            var detectorResult = new DotNetCorePlatformDetectorResult
            {
                Platform = DotNetCoreConstants.PlatformName,
                PlatformVersion = "3.1.32",
                SdkVersion = "3.1.426",
            };

            // Act
            var snippet = platform.GetInstallerScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(TestDotNetCorePlatformInstaller.InstallerScriptWithSkipSdkBinaryDownload, snippet);
            Assert.True(acrSdkProvider.RequestSdkFromAcrAsyncCalled);
        }

        [Fact]
        public void GetInstallerScript_FallsBackFromExternalAcrToExternalSdk_WhenExternalAcrFails()
        {
            // Arrange
            var externalAcrSdkProvider = new TestExternalAcrSdkProvider(returnValue: false);
            var commonOptions = new BuildScriptGeneratorOptions()
            {
                EnableDynamicInstall = true,
                EnableExternalAcrSdkProvider = true,
                EnableExternalSdkProvider = true,
                DebianFlavor = OsTypes.DebianBookworm,
            };
            var detector = CreateDetector(detectedVersion: "3.1");
            var platform = CreatePlatformWithProviders(
                detector,
                commonOptions: commonOptions,
                sdkAlreadyInstalled: false,
                externalAcrSdkProvider: externalAcrSdkProvider);
            var context = CreateContext();
            var detectorResult = new DotNetCorePlatformDetectorResult
            {
                Platform = DotNetCoreConstants.PlatformName,
                PlatformVersion = "3.1.2",
                SdkVersion = "3.1.2",
            };

            // Act
            var snippet = platform.GetInstallerScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(TestDotNetCorePlatformInstaller.InstallerScriptWithSkipSdkBinaryDownload, snippet);
            Assert.True(externalAcrSdkProvider.RequestSdkAsyncCalled);
        }

        [Fact]
        public void GetInstallerScript_FallsBackToCdn_WhenAllProvidersFail()
        {
            // Arrange
            var externalAcrSdkProvider = new TestExternalAcrSdkProvider(returnValue: false);
            var acrSdkProvider = new TestAcrSdkProvider(returnValue: false);
            var externalSdkProvider = new TestExternalSdkProvider(requestBlobResult: false);
            var commonOptions = new BuildScriptGeneratorOptions()
            {
                EnableDynamicInstall = true,
                EnableExternalAcrSdkProvider = true,
                EnableExternalSdkProvider = true,
                EnableAcrSdkProvider = true,
                DebianFlavor = OsTypes.DebianBookworm,
            };
            var detector = CreateDetector(detectedVersion: "3.1");
            var platform = CreatePlatformWithProviders(
                detector,
                commonOptions: commonOptions,
                sdkAlreadyInstalled: false,
                externalAcrSdkProvider: externalAcrSdkProvider,
                acrSdkProvider: acrSdkProvider,
                externalSdkProvider: externalSdkProvider,
                supportedVersions: new Dictionary<string, string>
                {
                    { "3.1.32", "3.1.426" },
                });
            var context = CreateContext();
            var detectorResult = new DotNetCorePlatformDetectorResult
            {
                Platform = DotNetCoreConstants.PlatformName,
                PlatformVersion = "3.1.32",
                SdkVersion = "3.1.426",
            };

            // Act
            var snippet = platform.GetInstallerScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(TestDotNetCorePlatformInstaller.InstallerScript, snippet);
            Assert.True(externalAcrSdkProvider.RequestSdkAsyncCalled);
            Assert.True(acrSdkProvider.RequestSdkFromAcrAsyncCalled);
        }

        [Fact]
        public void GetInstallerScript_DirectAcrProvider_PassesRuntimeVersion()
        {
            // Arrange
            var acrSdkProvider = new TestAcrSdkProvider(returnValue: true);
            var commonOptions = new BuildScriptGeneratorOptions()
            {
                EnableDynamicInstall = true,
                EnableAcrSdkProvider = true,
                DebianFlavor = OsTypes.DebianBookworm,
            };
            var detector = CreateDetector(detectedVersion: "3.1");
            var platform = CreatePlatformWithProviders(
                detector,
                commonOptions: commonOptions,
                sdkAlreadyInstalled: false,
                acrSdkProvider: acrSdkProvider,
                supportedVersions: new Dictionary<string, string>
                {
                    { "3.1.32", "3.1.426" },
                });
            var context = CreateContext();
            var detectorResult = new DotNetCorePlatformDetectorResult
            {
                Platform = DotNetCoreConstants.PlatformName,
                PlatformVersion = "3.1.32",
                SdkVersion = "3.1.426",
            };

            // Act
            var snippet = platform.GetInstallerScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(snippet);
            Assert.True(acrSdkProvider.RequestSdkFromAcrAsyncCalled);
            Assert.Equal(DotNetCoreConstants.PlatformName, acrSdkProvider.LastRequestedPlatformName);
            Assert.Equal("3.1.426", acrSdkProvider.LastRequestedVersion);
            Assert.Equal(OsTypes.DebianBookworm, acrSdkProvider.LastRequestedDebianFlavor);
            Assert.Equal("3.1.32", acrSdkProvider.LastRequestedRuntimeVersion);
        }

        [Fact]
        public void GetInstallerScript_FallsBackFromExternalAcrToDirectAcr_WhenBothExternalProvidersFail()
        {
            // Arrange
            var externalAcrSdkProvider = new TestExternalAcrSdkProvider(returnValue: false);
            var acrSdkProvider = new TestAcrSdkProvider(returnValue: true);
            var externalSdkProvider = new TestExternalSdkProvider(requestBlobResult: false);
            var commonOptions = new BuildScriptGeneratorOptions()
            {
                EnableDynamicInstall = true,
                EnableExternalAcrSdkProvider = true,
                EnableExternalSdkProvider = true,
                EnableAcrSdkProvider = true,
                DebianFlavor = OsTypes.DebianBookworm,
            };
            var detector = CreateDetector(detectedVersion: "3.1");
            var platform = CreatePlatformWithProviders(
                detector,
                commonOptions: commonOptions,
                sdkAlreadyInstalled: false,
                externalAcrSdkProvider: externalAcrSdkProvider,
                acrSdkProvider: acrSdkProvider,
                externalSdkProvider: externalSdkProvider,
                supportedVersions: new Dictionary<string, string>
                {
                    { "3.1.32", "3.1.426" },
                });
            var context = CreateContext();
            var detectorResult = new DotNetCorePlatformDetectorResult
            {
                Platform = DotNetCoreConstants.PlatformName,
                PlatformVersion = "3.1.32",
                SdkVersion = "3.1.426",
            };

            // Act
            var snippet = platform.GetInstallerScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(TestDotNetCorePlatformInstaller.InstallerScriptWithSkipSdkBinaryDownload, snippet);
            Assert.True(externalAcrSdkProvider.RequestSdkAsyncCalled);
            Assert.True(acrSdkProvider.RequestSdkFromAcrAsyncCalled);
        }

        [Fact]
        public void ResolveVersions_UsesExternalAcrSdkVersion_WhenEnabled()
        {
            // Arrange
            var commonOptions = new BuildScriptGeneratorOptions()
            {
                EnableDynamicInstall = true,
                EnableExternalAcrSdkProvider = true,
                DebianFlavor = OsTypes.DebianBookworm,
            };
            var detector = CreateDetector(detectedVersion: "3.1");
            var externalAcrVersionProvider = new TestDotNetCoreExternalAcrVersionProvider(
                Options.Create(commonOptions), NullLoggerFactory.Instance, new DefaultStandardOutputWriter(),
                sdkVersion: "3.1.415");
            var platform = CreatePlatformWithExternalAcrVersionProvider(
                detector,
                commonOptions: commonOptions,
                externalAcrVersionProvider: externalAcrVersionProvider,
                supportedVersions: new Dictionary<string, string>
                {
                    { "3.1.2", "3.1.302" },
                });
            var context = CreateContext();
            var detectorResult = new DotNetCorePlatformDetectorResult
            {
                Platform = DotNetCoreConstants.PlatformName,
                PlatformVersion = "3.1",
            };

            // Act
            platform.ResolveVersions(context, detectorResult);

            // Assert - ExternalACR dictates SDK version, overriding normal map lookup.
            // PlatformVersion is only processed by hierarchical rules (not fully resolved
            // against the version map), so it stays as the detected value.
            Assert.Equal("3.1.415", detectorResult.SdkVersion);
            Assert.Equal("3.1", detectorResult.PlatformVersion);
        }

        [Fact]
        public void ResolveVersions_FallsBackToVersionMap_WhenExternalAcrReturnsNull()
        {
            // Arrange
            var commonOptions = new BuildScriptGeneratorOptions()
            {
                EnableDynamicInstall = true,
                EnableExternalAcrSdkProvider = true,
                DebianFlavor = OsTypes.DebianBookworm,
            };
            var detector = CreateDetector(detectedVersion: "3.1");
            var externalAcrVersionProvider = new TestDotNetCoreExternalAcrVersionProvider(
                Options.Create(commonOptions), NullLoggerFactory.Instance, new DefaultStandardOutputWriter(),
                sdkVersion: null);
            var platform = CreatePlatformWithExternalAcrVersionProvider(
                detector,
                commonOptions: commonOptions,
                externalAcrVersionProvider: externalAcrVersionProvider,
                supportedVersions: new Dictionary<string, string>
                {
                    { "3.1.2", "3.1.302" },
                });
            var context = CreateContext();
            var detectorResult = new DotNetCorePlatformDetectorResult
            {
                Platform = DotNetCoreConstants.PlatformName,
                PlatformVersion = "3.1",
            };

            // Act
            platform.ResolveVersions(context, detectorResult);

            // Assert - Falls back to normal version map.
            // PlatformVersion is fully resolved (hierarchical rules + version map).
            Assert.Equal("3.1.302", detectorResult.SdkVersion);
            Assert.Equal("3.1.2", detectorResult.PlatformVersion);
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
            var externalSdkProvider = new TestExternalSdkProvider();
            var commonOptions = new BuildScriptGeneratorOptions();
            var dotNetCoreScriptGeneratorOptions = new DotNetCoreScriptGeneratorOptions();
            dotNetCoreScriptGeneratorOptions.DefaultRuntimeVersion = envVarDefaultVersion;
            var installer = new DotNetCorePlatformInstaller(
                Options.Create(commonOptions),
                NullLoggerFactory.Instance);
            var globalJsonSdkResolver = new GlobalJsonSdkResolver(NullLogger<GlobalJsonSdkResolver>.Instance);
            return new TestDotNetCorePlatform(
                versionProvider,
                new DotNetCoreExternalAcrVersionProvider(Options.Create(new BuildScriptGeneratorOptions()), NullLoggerFactory.Instance, new DefaultStandardOutputWriter()),
                detector,
                Options.Create(commonOptions),
                Options.Create(dotNetCoreScriptGeneratorOptions),
                installer,
                globalJsonSdkResolver,
                externalSdkProvider,
                new TestExternalAcrSdkProvider(),
                new TestAcrSdkProvider(),
                TelemetryClientHelper.GetTelemetryClient(),
                new DefaultStandardOutputWriter());
        }

        private DotNetCorePlatform CreatePlatformWithProviders(
            IDotNetCorePlatformDetector detector,
            BuildScriptGeneratorOptions commonOptions = null,
            bool sdkAlreadyInstalled = true,
            Dictionary<string, string> supportedVersions = null,
            IExternalAcrSdkProvider externalAcrSdkProvider = null,
            IAcrSdkProvider acrSdkProvider = null,
            IExternalSdkProvider externalSdkProvider = null)
        {
            commonOptions = commonOptions ?? new BuildScriptGeneratorOptions();
            var defaultVersion = DotNetCoreRunTimeVersions.NetCoreApp31;
            supportedVersions = supportedVersions ?? new Dictionary<string, string>
            {
                { defaultVersion, defaultVersion },
            };
            var versionProvider = new TestDotNetCoreVersionProvider(
                supportedVersions,
                defaultVersion);
            externalSdkProvider = externalSdkProvider ?? new TestExternalSdkProvider();
            externalAcrSdkProvider = externalAcrSdkProvider ?? new TestExternalAcrSdkProvider();
            acrSdkProvider = acrSdkProvider ?? new TestAcrSdkProvider();
            var dotNetCoreScriptGeneratorOptions = new DotNetCoreScriptGeneratorOptions();
            var installer = new TestDotNetCorePlatformInstaller(
                Options.Create(commonOptions),
                sdkAlreadyInstalled,
                NullLoggerFactory.Instance);
            var globalJsonSdkResolver = new GlobalJsonSdkResolver(NullLogger<GlobalJsonSdkResolver>.Instance);
            return new TestDotNetCorePlatform(
                versionProvider,
                new DotNetCoreExternalAcrVersionProvider(Options.Create(new BuildScriptGeneratorOptions()), NullLoggerFactory.Instance, new DefaultStandardOutputWriter()),
                detector,
                Options.Create(commonOptions),
                Options.Create(dotNetCoreScriptGeneratorOptions),
                installer,
                globalJsonSdkResolver,
                externalSdkProvider,
                externalAcrSdkProvider,
                acrSdkProvider,
                TelemetryClientHelper.GetTelemetryClient(),
                new DefaultStandardOutputWriter());
        }

        private class TestDotNetCorePlatform : DotNetCorePlatform
        {
            public TestDotNetCorePlatform(
                IDotNetCoreVersionProvider versionProvider,
                DotNetCoreExternalAcrVersionProvider externalAcrVersionProvider,
                IDotNetCorePlatformDetector detector,
                IOptions<BuildScriptGeneratorOptions> cliOptions,
                IOptions<DotNetCoreScriptGeneratorOptions> dotNetCoreScriptGeneratorOptions,
                DotNetCorePlatformInstaller platformInstaller,
                GlobalJsonSdkResolver globalJsonSdkResolver,
                IExternalSdkProvider externalSdkProvider,
                IExternalAcrSdkProvider externalAcrSdkProvider,
                IAcrSdkProvider acrSdkProvider,
                TelemetryClient telemetryClient,
                IStandardOutputWriter outputWriter)
                : base(
                      versionProvider,
                      externalAcrVersionProvider,
                      NullLogger<DotNetCorePlatform>.Instance,
                      detector,
                      cliOptions,
                      dotNetCoreScriptGeneratorOptions,
                      platformInstaller,
                      globalJsonSdkResolver,
                      externalSdkProvider,
                      externalAcrSdkProvider,
                      acrSdkProvider,
                      telemetryClient,
                      outputWriter)
            {
            }
        }

        private class TestDotNetCorePlatformInstaller : DotNetCorePlatformInstaller
        {
            public static string InstallerScript = "installer-script-snippet";
            public static string InstallerScriptWithSkipSdkBinaryDownload = "installer-script-snippet-with-skip-sdk-binary-download";
            private readonly bool _sdkIsAlreadyInstalled;

            public TestDotNetCorePlatformInstaller(
                IOptions<BuildScriptGeneratorOptions> cliOptions,
                bool sdkIsAlreadyInstalled,
                ILoggerFactory loggerFactory)
                : base(cliOptions, loggerFactory)
            {
                _sdkIsAlreadyInstalled = sdkIsAlreadyInstalled;
            }

            public override bool IsVersionAlreadyInstalled(string version)
            {
                return _sdkIsAlreadyInstalled;
            }

            public override string GetInstallerScriptSnippet(string version, bool skipSdkBinaryDownload = false)
            {
                if (skipSdkBinaryDownload)
                {
                    return InstallerScriptWithSkipSdkBinaryDownload;
                }

                return InstallerScript;
            }
        }

        private class TestDotNetCoreExternalAcrVersionProvider : DotNetCoreExternalAcrVersionProvider
        {
            private readonly string _sdkVersion;

            public TestDotNetCoreExternalAcrVersionProvider(
                IOptions<BuildScriptGeneratorOptions> options,
                ILoggerFactory loggerFactory,
                IStandardOutputWriter outputWriter,
                string sdkVersion)
                : base(options, loggerFactory, outputWriter)
            {
                _sdkVersion = sdkVersion;
            }

            public override string GetSdkVersion()
            {
                return _sdkVersion;
            }
        }

        private DotNetCorePlatform CreatePlatformWithExternalAcrVersionProvider(
            IDotNetCorePlatformDetector detector,
            BuildScriptGeneratorOptions commonOptions = null,
            TestDotNetCoreExternalAcrVersionProvider externalAcrVersionProvider = null,
            Dictionary<string, string> supportedVersions = null)
        {
            commonOptions = commonOptions ?? new BuildScriptGeneratorOptions();
            var defaultVersion = DotNetCoreRunTimeVersions.NetCoreApp31;
            supportedVersions = supportedVersions ?? new Dictionary<string, string>
            {
                { defaultVersion, defaultVersion },
            };
            var versionProvider = new TestDotNetCoreVersionProvider(
                supportedVersions,
                defaultVersion);
            externalAcrVersionProvider = externalAcrVersionProvider ?? new TestDotNetCoreExternalAcrVersionProvider(
                Options.Create(commonOptions), NullLoggerFactory.Instance, new DefaultStandardOutputWriter(), sdkVersion: null);
            var dotNetCoreScriptGeneratorOptions = new DotNetCoreScriptGeneratorOptions();
            var installer = new DotNetCorePlatformInstaller(
                Options.Create(commonOptions),
                NullLoggerFactory.Instance);
            var globalJsonSdkResolver = new GlobalJsonSdkResolver(NullLogger<GlobalJsonSdkResolver>.Instance);
            return new TestDotNetCorePlatform(
                versionProvider,
                externalAcrVersionProvider,
                detector,
                Options.Create(commonOptions),
                Options.Create(dotNetCoreScriptGeneratorOptions),
                installer,
                globalJsonSdkResolver,
                new TestExternalSdkProvider(),
                new TestExternalAcrSdkProvider(),
                new TestAcrSdkProvider(),
                TelemetryClientHelper.GetTelemetryClient(),
                new DefaultStandardOutputWriter());
        }
    }
}
