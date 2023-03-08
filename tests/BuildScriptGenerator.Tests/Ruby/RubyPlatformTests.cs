// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Golang;
using Microsoft.Oryx.BuildScriptGenerator.Ruby;
using Microsoft.Oryx.Detector.Ruby;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Ruby
{
    public class RubyPlatformTest
    {
        [Fact]
        public void Detect_ReturnsVersionFromOptions_EvenIfDetectorReturnsVersion()
        {
            // Arrange
            var expectedVersion = "2.7.1";
            var detectedVersion = "2.6.6";
            var defaultVersion = "2.7.1";
            var rubyScriptGeneratorOptions = new RubyScriptGeneratorOptions
            {
                RubyVersion = expectedVersion
            };
            var platform = CreateRubyPlatform(
                supportedRubyVersions: new[] { expectedVersion },
                defaultVersion: defaultVersion,
                detectedVersion: detectedVersion,
                rubyScriptGeneratorOptions: rubyScriptGeneratorOptions);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(RubyConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedRubyVersion_ReturnedByDetector()
        {
            // Arrange
            var detectedVersion = "0";
            var supportedVersion = "2.7.1";
            var platform = CreateRubyPlatform(
                supportedRubyVersions: new[] { supportedVersion },
                defaultVersion: supportedVersion,
                detectedVersion: detectedVersion);
            var context = CreateContext();

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => platform.Detect(context));
            Assert.Equal(
                $"Platform 'ruby' version '{detectedVersion}' is unsupported. " +
                $"Supported versions: {supportedVersion}",
                exception.Message);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedRubyVersion_IsSetInOptions()
        {
            // Arrange
            var versionFromOptions = "0";
            var supportedVersion = "2.7.1";
            var rubyScriptGeneratorOptions = new RubyScriptGeneratorOptions
            {
                RubyVersion = versionFromOptions
            };
            var platform = CreateRubyPlatform(
                supportedRubyVersions: new[] { supportedVersion },
                defaultVersion: supportedVersion,
                detectedVersion: supportedVersion,
                rubyScriptGeneratorOptions: rubyScriptGeneratorOptions);
            var context = CreateContext();

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => platform.Detect(context));
            Assert.Equal(
                $"Platform 'ruby' version '{versionFromOptions}' is unsupported. " +
                $"Supported versions: {supportedVersion}",
                exception.Message);
        }

        [Fact]
        public void Detect_ReturnsDefaultVersion_IfNoVersionFoundReturnedByDetector_OrOptions()
        {
            // Arrange
            var expectedVersion = "2.7.1";
            var platform = CreateRubyPlatform(
                supportedRubyVersions: new[] { expectedVersion },
                defaultVersion: expectedVersion,
                detectedVersion: null);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(RubyConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void GeneratedBuildSnippet_ContainsJekyllCommand_WhenConfigYmlFileExists_AndGemfileDoesNotExist_ForStaticWebApp()
        {
            // Arrange
            var commonOptions = new BuildScriptGeneratorOptions();
            commonOptions.AppType = Constants.StaticSiteApplications;
            var rubyPlatform = CreateRubyPlatform(
                commonOptions: commonOptions,
                isRubyVersionAlreadyInstalled: false);
            var repo = new MemorySourceRepo();
            repo.AddFile(string.Empty, RubyConstants.ConfigYmlFileName);
            var context = CreateContext(repo);
            var detectorResult = new RubyPlatformDetectorResult
            {
                Platform = RubyConstants.PlatformName,
                PlatformVersion = "2.6.6",
                GemfileExists = false,
                ConfigYmlFileExists = true,
            };

            // Act
            var buildScriptSnippet = rubyPlatform.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains("gem install jekyll", buildScriptSnippet.BashBuildScriptSnippet);
            Assert.Contains("jekyll build", buildScriptSnippet.BashBuildScriptSnippet);
            Assert.DoesNotContain("bundle install", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_ContainsJekyllCommand_WhenConfigYmlFileExists_AndGemfileExists_ForStaticWebApp()
        {
            // Arrange
            var commonOptions = new BuildScriptGeneratorOptions();
            commonOptions.AppType = Constants.StaticSiteApplications;
            var rubyPlatform = CreateRubyPlatform(
                commonOptions: commonOptions,
                isRubyVersionAlreadyInstalled: false);
            var repo = new MemorySourceRepo();
            repo.AddFile(string.Empty, RubyConstants.GemFileName);
            repo.AddFile(string.Empty, RubyConstants.ConfigYmlFileName);
            var context = CreateContext(repo);
            var detectorResult = new RubyPlatformDetectorResult
            {
                Platform = RubyConstants.PlatformName,
                PlatformVersion = "2.6.6",
                GemfileExists = true,
                ConfigYmlFileExists = true,
            };

            // Act
            var buildScriptSnippet = rubyPlatform.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.DoesNotContain("gem install jekyll", buildScriptSnippet.BashBuildScriptSnippet);
            Assert.Contains("jekyll build", buildScriptSnippet.BashBuildScriptSnippet);
            Assert.Contains("bundle install", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_DoesNotContainsJekyllCommand_WhenOnlyGemFileExists_ForStaticWebApp()
        {
            // Arrange
            var commonOptions = new BuildScriptGeneratorOptions();
            commonOptions.AppType = Constants.StaticSiteApplications;
            var rubyPlatform = CreateRubyPlatform(
                commonOptions: commonOptions,
                isRubyVersionAlreadyInstalled: false);
            var repo = new MemorySourceRepo();
            repo.AddFile(string.Empty, RubyConstants.GemFileName);
            var context = CreateContext(repo);
            var detectorResult = new RubyPlatformDetectorResult
            {
                Platform = RubyConstants.PlatformName,
                PlatformVersion = "2.6.6",
                GemfileExists = true,

            };

            // Act
            var buildScriptSnippet = rubyPlatform.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.DoesNotContain("gem install jekyll", buildScriptSnippet.BashBuildScriptSnippet);
            Assert.DoesNotContain("jekyll build", buildScriptSnippet.BashBuildScriptSnippet);
            Assert.Contains("bundle install", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_CustomBuildCommandWillExecute_WhenOtherCommandsAlsoExist()
        {
            // Arrange
            var commonOptions = new BuildScriptGeneratorOptions();
            commonOptions.AppType = Constants.StaticSiteApplications;
            var rubyScriptGeneratorOptions = new RubyScriptGeneratorOptions
            {
                CustomBuildCommand = "custom build command"
            };
            var rubyPlatform = CreateRubyPlatform(
                rubyScriptGeneratorOptions: rubyScriptGeneratorOptions,
                commonOptions: commonOptions,
                isRubyVersionAlreadyInstalled: false);
            var repo = new MemorySourceRepo();
            repo.AddFile(string.Empty, RubyConstants.GemFileName);
            var context = CreateContext(repo);
            var detectorResult = new RubyPlatformDetectorResult
            {
                Platform = RubyConstants.PlatformName,
                PlatformVersion = "2.6.6",
                GemfileExists = true,

            };

            // Act
            var buildScriptSnippet = rubyPlatform.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.DoesNotContain("gem install jekyll", buildScriptSnippet.BashBuildScriptSnippet);
            Assert.DoesNotContain("jekyll build", buildScriptSnippet.BashBuildScriptSnippet);
            Assert.DoesNotContain("bundle install", buildScriptSnippet.BashBuildScriptSnippet);
            Assert.Contains("custom build command", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void HasRubyInstallScript_IfDynamicInstallIsEnabled_AndRubyVersionIsNotAlreadyInstalled()
        {
            // Arrange
            var expectedScript = "test-script";
            var commonOptions = new BuildScriptGeneratorOptions();
            commonOptions.EnableDynamicInstall = true;
            var rubyPlatform = CreateRubyPlatform(
                commonOptions: commonOptions,
                isRubyVersionAlreadyInstalled: false,
                rubyInstallationScript: expectedScript);
            var repo = new MemorySourceRepo();
            repo.AddFile("{}", RubyConstants.GemFileName);
            var context = CreateContext(repo);
            var detectedResult = new RubyPlatformDetectorResult
            {
                Platform = RubyConstants.PlatformName,
                PlatformVersion = "2.7.1",
            };

            // Act
            var actualScriptSnippet = rubyPlatform.GetInstallerScriptSnippet(context, detectedResult);

            // Assert
            Assert.NotNull(actualScriptSnippet);
            Assert.Contains(expectedScript, actualScriptSnippet);
        }

        [Fact]
        public void HasNoRubyInstallScript_IfDynamicInstallIsEnabled_AndRubyVersionIsAlreadyInstalled()
        {
            // Arrange
            var installationScript = "test-script";
            var commonOptions = new BuildScriptGeneratorOptions();
            commonOptions.EnableDynamicInstall = true;
            var rubyPlatform = CreateRubyPlatform(
                commonOptions: commonOptions,
                isRubyVersionAlreadyInstalled: true,
                rubyInstallationScript: installationScript);
            var repo = new MemorySourceRepo();
            repo.AddFile("{}", RubyConstants.GemFileName);
            var context = CreateContext(repo);
            var detectedResult = new RubyPlatformDetectorResult
            {
                Platform = RubyConstants.PlatformName,
                PlatformVersion = "2.7.1",
            };

            // Act
            var actualScriptSnippet = rubyPlatform.GetInstallerScriptSnippet(context, detectedResult);

            // Assert
            Assert.Null(actualScriptSnippet);
        }

        [Fact]
        public void DoesNotHaveRubyInstallScript_IfDynamicInstallNotEnabled_AndRubyVersionIsNotAlreadyInstalled()
        {
            // Arrange
            var installationScript = "test-script";
            var commonOptions = new BuildScriptGeneratorOptions();
            commonOptions.EnableDynamicInstall = false;
            var rubyPlatform = CreateRubyPlatform(
                commonOptions: commonOptions,
                isRubyVersionAlreadyInstalled: false,
                rubyInstallationScript: installationScript);
            var repo = new MemorySourceRepo();
            repo.AddFile("{}", RubyConstants.GemFileName);
            var context = CreateContext(repo);
            var detectedResult = new RubyPlatformDetectorResult
            {
                Platform = RubyConstants.PlatformName,
                PlatformVersion = "2.7.1",
            };

            // Act
            var actualScriptSnippet = rubyPlatform.GetInstallerScriptSnippet(context, detectedResult);

            // Assert
            Assert.Null(actualScriptSnippet);
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
            var options = new RubyScriptGeneratorOptions();
            options.DefaultVersion = envVarDefaultVersion;
            var platform = CreateRubyPlatform(
                detectedVersion: detectedVersion,
                defaultVersion: detectedDefaultVersion,
                rubyScriptGeneratorOptions: options,
                supportedRubyVersions: new[] { detectedVersion, detectedDefaultVersion, envVarDefaultVersion });

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(RubyConstants.PlatformName, result.Platform);
            Assert.Equal(expectedSdkVersion, result.PlatformVersion);
        }

        private RubyPlatform CreateRubyPlatform(
            string[] supportedRubyVersions = null,
            string defaultVersion = null,
            string detectedVersion = null,
            BuildScriptGeneratorOptions commonOptions = null,
            RubyScriptGeneratorOptions rubyScriptGeneratorOptions = null,
            bool? isRubyVersionAlreadyInstalled = null,
            string rubyInstallationScript = null)
        {
            commonOptions = commonOptions ?? new BuildScriptGeneratorOptions();
            rubyScriptGeneratorOptions = rubyScriptGeneratorOptions ?? new RubyScriptGeneratorOptions();
            isRubyVersionAlreadyInstalled = isRubyVersionAlreadyInstalled ?? true;
            rubyInstallationScript = rubyInstallationScript ?? "default-ruby-installation-script";
            var versionProvider = new TestRubyVersionProvider(supportedRubyVersions, defaultVersion);
            var detector = new TestRubyPlatformDetector(detectedVersion: detectedVersion);
            var rubyInstaller = new TestRubyPlatformInstaller(
                Options.Create(commonOptions),
                isRubyVersionAlreadyInstalled.Value,
                rubyInstallationScript);
            return new TestRubyPlatform(
                Options.Create(rubyScriptGeneratorOptions),
                Options.Create(commonOptions),
                versionProvider,
                NullLogger<TestRubyPlatform>.Instance,
                detector,
                rubyInstaller, 
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

        private class TestRubyPlatform : RubyPlatform
        {
            public TestRubyPlatform(
                IOptions<RubyScriptGeneratorOptions> rubyScriptGeneratorOptions,
                IOptions<BuildScriptGeneratorOptions> commonOptions,
                IRubyVersionProvider rubyVersionProvider,
                ILogger<RubyPlatform> logger,
                IRubyPlatformDetector detector,
                RubyPlatformInstaller rubyInstaller,
                TelemetryClient telemetryClient)
                : base(
                      rubyScriptGeneratorOptions,
                      commonOptions,
                      rubyVersionProvider,
                      logger,
                      detector,
                      rubyInstaller,
                      telemetryClient)
            {
            }
        }

        private class TestRubyPlatformInstaller : RubyPlatformInstaller
        {
            private readonly bool _isVersionAlreadyInstalled;
            private readonly string _installerScript;

            public TestRubyPlatformInstaller(
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