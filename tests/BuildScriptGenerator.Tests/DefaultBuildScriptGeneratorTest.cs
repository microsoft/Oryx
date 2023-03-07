// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Resources;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class DefaultBuildScriptGeneratorTest : IClassFixture<TestTempDirTestFixture>
    {
        private const string TestPlatformName = "test";

        private readonly string _tempDirRoot;

        public DefaultBuildScriptGeneratorTest(TestTempDirTestFixture testFixure)
        {
            _tempDirRoot = testFixure.RootDirPath;
        }

        [Fact]
        public void TryGenerateScript_ReturnsTrue_IfNoPlatformIsProvided_AndCanDetectPlatform()
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "test",
                detectedPlatformVersion: "1.0.0");
            var platform = new TestProgrammingPlatform(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content",
                detector: detector);
            var commonOptions = new BuildScriptGeneratorOptions();
            var generator = CreateDefaultScriptGenerator(platform, commonOptions);
            var context = CreateScriptGeneratorContext();

            // Act
            generator.GenerateBashScript(context, out var generatedScript);

            // Assert
            Assert.Contains("script-content", generatedScript);
            Assert.True(detector.DetectInvoked);
        }

        [Fact]
        public void TryGenerateScript_OnlyProcessProvidedPlatform_IfMultiPlatformIsDisabled()
        {
            // Arrange
            var detector1 = new TestPlatformDetectorSimpleMatch(shouldMatch: true, "main", "1.0.0");
            var platform1 = new TestProgrammingPlatform(
                "main",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content",
                detector: detector1);
            var detector2 = new TestPlatformDetectorSimpleMatch(shouldMatch: true, "anotherPlatform", "1.0.0");
            var platform2 = new TestProgrammingPlatform(
                "anotherPlatform",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "some code",
                detector: detector2);
            var commonOptions = new BuildScriptGeneratorOptions
            {
                PlatformName = "main",
                PlatformVersion = "1.0.0",
                EnableMultiPlatformBuild = false,
            };
            var generator = CreateDefaultScriptGenerator(new[] { platform1, platform2 }, commonOptions);
            var context = CreateScriptGeneratorContext();

            // Act
            generator.GenerateBashScript(context, out var generatedScript);

            // Assert
            Assert.Contains("script-content", generatedScript);
            Assert.DoesNotContain("some code", generatedScript);
        }

        [Fact]
        public void TryGenerateScript_ReturnsTrue_IfPlatformIsProvidedButNoVersion_AndCanDetectVersion()
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "test",
                detectedPlatformVersion: "1.0.0");
            var platform = new TestProgrammingPlatform(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content",
                detector: detector);
            var commonOptions = new BuildScriptGeneratorOptions
            {
                PlatformName = "test",
                PlatformVersion = null, // version not provided by user
            };
            var generator = CreateDefaultScriptGenerator(platform, commonOptions);
            var context = CreateScriptGeneratorContext();

            // Act
            generator.GenerateBashScript(context, out var generatedScript);

            // Assert
            Assert.Contains("script-content", generatedScript);
            Assert.True(detector.DetectInvoked);
        }

        [Fact]
        public void TryGenerateScript_Throws_IfNoPlatformIsProvided_AndCannotDetectPlatform()
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: null,
                detectedPlatformVersion: null);
            var platform = new TestProgrammingPlatform("test", new[] { "1.0.0" }, detector: detector);
            var commonOptions = new BuildScriptGeneratorOptions
            {
                EnableMultiPlatformBuild = true,
            };
            var generator = CreateDefaultScriptGenerator(platform, commonOptions);
            var context = CreateScriptGeneratorContext();

            // Act & Assert
            var exception = Assert.Throws<UnsupportedPlatformException>(
                () => generator.GenerateBashScript(context, out var generatedScript));
            Assert.Equal(Labels.UnableToDetectPlatformMessage, exception.Message);
            Assert.True(detector.DetectInvoked);
        }

        [Fact]
        public void TryGenerateScript_Throws_IfPlatformIsProvidedButNoVersion_AndCannotDetectVersion()
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "test",
                detectedPlatformVersion: null);
            var platform = new TestProgrammingPlatform("test", new[] { "1.0.0" }, detector: detector);
            var commonOptions = new BuildScriptGeneratorOptions
            {
                PlatformName = "test",
            };
            var generator = CreateDefaultScriptGenerator(platform, commonOptions);
            var context = CreateScriptGeneratorContext();

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(
                () => generator.GenerateBashScript(context, out var generatedScript));
            Assert.Equal("Couldn't detect a version for the platform 'test' in the repo.", exception.Message);
            Assert.True(detector.DetectInvoked);
        }

        [Fact]
        public void TryGenerateScript_Throws_IfPlatformIsProvided_AndCannotDetectPlatform()
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: null,
                detectedPlatformVersion: null);
            var platform = new TestProgrammingPlatform("test1", new[] { "1.0.0" }, detector: detector);
            var commonOptions = new BuildScriptGeneratorOptions
            {
                PlatformName = "test2",
            };
            var generator = CreateDefaultScriptGenerator(platform, commonOptions);
            var context = CreateScriptGeneratorContext();

            // Act & Assert
            var exception = Assert.Throws<UnsupportedPlatformException>(
                () => generator.GenerateBashScript(context, out var generatedScript));
            Assert.Equal("'test2' platform is not supported. Supported platforms are: test1", exception.Message);
        }

        [Fact]
        public void TryGenerateScript_Throws_IfPlatformIsProvidedButDisabled()
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "test",
                detectedPlatformVersion: "1.0.0");
            var platform = new TestProgrammingPlatform("test", new[] { "1.0.0" }, detector: detector, enabled: false);
            var commonOptions = new BuildScriptGeneratorOptions
            {
                PlatformName = "test",
                PlatformVersion = "1.0.0",
            };
            var generator = CreateDefaultScriptGenerator(platform, commonOptions);
            var context = CreateScriptGeneratorContext();

            // Act & Assert
            var exception = Assert.Throws<UnsupportedPlatformException>(
                () => generator.GenerateBashScript(context, out var generatedScript));
        }

        [Fact]
        public void TryGenerateScript_Throws_IfSuppliedPlatformIsUnsupported()
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "test",
                detectedPlatformVersion: "1.0.0");
            var platform = new TestProgrammingPlatform(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content",
                detector: detector);
            var commonOptions = new BuildScriptGeneratorOptions
            {
                PlatformName = "unsupported",
                PlatformVersion = "1.0.0",
            };
            var generator = CreateDefaultScriptGenerator(platform, commonOptions);
            var context = CreateScriptGeneratorContext();

            // Act & Assert
            var exception = Assert.Throws<UnsupportedPlatformException>(
                () => generator.GenerateBashScript(context, out var generatedScript));
            Assert.Equal(
                "'unsupported' platform is not supported. Supported platforms are: test",
                exception.Message);
            Assert.True(detector.DetectInvoked);
        }

        [Fact]
        public void TryGenerateScript_ReturnsFalse_IfGeneratorTryGenerateScript_IsFalse()
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "test",
                detectedPlatformVersion: "1.0.0");
            var platform = new TestProgrammingPlatform(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: false,
                scriptContent: null,
                detector: detector);
            var commonOptions = new BuildScriptGeneratorOptions();
            var generator = CreateDefaultScriptGenerator(platform, commonOptions);
            var context = CreateScriptGeneratorContext();

            // Act & Assert
            var exception = Assert.Throws<UnsupportedPlatformException>(
                () => generator.GenerateBashScript(context, out var generatedScript));
            Assert.Equal(Labels.UnableToDetectPlatformMessage, exception.Message);
            Assert.True(detector.DetectInvoked);
        }

        [Fact]
        public void TryGenerateScript_CallsDetector_IfMultiPlatformIsOff_AndNoLangProvided()
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "test",
                detectedPlatformVersion: "1.0.0");
            var platform = new TestProgrammingPlatform(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content",
                detector: detector);
            var commonOptions = new BuildScriptGeneratorOptions
            {
                EnableMultiPlatformBuild = false,
            };
            var generator = CreateDefaultScriptGenerator(platform, commonOptions);
            var context = CreateScriptGeneratorContext();

            // Act & Assert
            generator.GenerateBashScript(context, out var generatedScript);
            Assert.True(detector.DetectInvoked);
        }

        [Fact]
        public void TryGenerateScript_DoesntCallDetector_IfMultiPlatformIsOff_AndLangProvided()
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "test",
                detectedPlatformVersion: "1.0.0");
            var platform = new TestProgrammingPlatform(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content",
                detector: detector);

            var detector2 = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "test2",
                detectedPlatformVersion: "1.0.0");
            var platform2 = new TestProgrammingPlatform(
                "test2",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content",
                detector: detector2);

            var commonOptions = new BuildScriptGeneratorOptions
            {
                PlatformName = "test",
                PlatformVersion = "1.0.0",
            };
            var generator = CreateDefaultScriptGenerator(new[] { platform, platform2 }, commonOptions);
            var context = CreateScriptGeneratorContext();

            // Act & Assert
            generator.GenerateBashScript(context, out var generatedScript);
            Assert.True(detector.DetectInvoked);
            Assert.True(detector2.DetectInvoked);
        }

        [Fact]
        public void TryGenerateScript_CallsDetector_IfMultiPlatformIsOn_AndLangProvided()
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "test",
                detectedPlatformVersion: "1.0.0");
            var platform = new TestProgrammingPlatform(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content",
                detector: detector);

            var detector2 = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "test2",
                detectedPlatformVersion: "1.0.0");
            var platform2 = new TestProgrammingPlatform(
                "test2",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content",
                detector: detector2);

            var commonOptions = new BuildScriptGeneratorOptions
            {
                PlatformName = "test",
                PlatformVersion = "1.0.0",
                EnableMultiPlatformBuild = true,
            };
            var generator = CreateDefaultScriptGenerator(new[] { platform, platform2 }, commonOptions);
            var context = CreateScriptGeneratorContext();

            // Act & Assert
            generator.GenerateBashScript(context, out var generatedScript);
            Assert.True(detector.DetectInvoked);
            Assert.True(detector2.DetectInvoked);
        }

        [Fact]
        public void GeneratesScript_UsingTheFirstPlatform_WhichCanGenerateScript()
        {
            // Arrange
            var detector1 = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "lang1",
                detectedPlatformVersion: "1.0.0");
            var platform1 = new TestProgrammingPlatform(
                "lang1",
                new[] { "1.0.0" },
                canGenerateScript: false,
                scriptContent: null,
                installationScriptContent: "lang1-installationscript",
                detector: detector1);
            var detector2 = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "lang2",
                detectedPlatformVersion: "1.0.0");
            var platform2 = new TestProgrammingPlatform(
                "lang2",
                new[] { "1.0.0" },
                canGenerateScript: true,
                installationScriptContent: "lang2-installationscript",
                scriptContent: "script-content",
                detector: detector2);

            var commonOptions = new BuildScriptGeneratorOptions
            {
                PlatformName = "lang2",
                PlatformVersion = "1.0.0",
            };
            var generator = CreateDefaultScriptGenerator(new[] { platform1, platform2 }, commonOptions);
            var context = CreateScriptGeneratorContext();

            // Act
            generator.GenerateBashScript(context, out var generatedScript);

            // Assert
            Assert.Contains("script-content", generatedScript);
            Assert.True(detector1.DetectInvoked);
            Assert.True(detector2.DetectInvoked);
            Assert.Contains("lang1-installationscript", generatedScript);
            Assert.Contains("lang2-installationscript", generatedScript);
        }

        [Fact]
        public void GeneratesScript_AddsSnippetsForMultiplePlatforms()
        {
            // Arrange
            var platform1 = new TestProgrammingPlatform(
                platformName: "lang1",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "ABCDEFG",
                detector: new TestPlatformDetectorSimpleMatch(
                    shouldMatch: true,
                    platformName: "lang1",
                    platformVersion: "1.0.0"));
            var platform2 = new TestProgrammingPlatform(
                platformName: "lang2",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "123456",
                detector: new TestPlatformDetectorSimpleMatch(
                    shouldMatch: true,
                    platformName: "lang2",
                    platformVersion: "1.0.0"));

            var commonOptions = new BuildScriptGeneratorOptions
            {
                PlatformName = "lang1",
                PlatformVersion = "1.0.0",
                EnableMultiPlatformBuild = true,
            };
            var generator = CreateDefaultScriptGenerator(new[] { platform1, platform2 }, commonOptions);
            var context = CreateScriptGeneratorContext();

            // Act
            generator.GenerateBashScript(context, out var generatedScript);
            var expectedPlatformNameManifestEntry = "echo \"PlatformName=\\\"lang1,lang2\\\"\" >> \"$MANIFEST_DIR/$MANIFEST_FILE\"";
            var buggyPlatformNameManifestEntry = "echo \"PlatformName=\\\",lang1lang2\\\"\" >> \"$MANIFEST_DIR/$MANIFEST_FILE\"";
            // Assert
            Assert.Contains("ABCDEFG", generatedScript);
            Assert.Contains("123456", generatedScript);
            Assert.Contains(expectedPlatformNameManifestEntry, generatedScript, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(buggyPlatformNameManifestEntry, generatedScript, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GeneratesScript_AddsSnippetsForOnePlatform_OtherIsDisabled()
        {
            // Arrange
            var platform1 = new TestProgrammingPlatform(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "ABCDEFG",
                detector: new TestPlatformDetectorSimpleMatch(shouldMatch: true, "test", "1.0.0"));
            var platform2 = new TestProgrammingPlatform(
                "test2",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "123456",
                detector: new TestPlatformDetectorSimpleMatch(shouldMatch: true, "test2", "1.0.0"),
                enabled: false);

            var commonOptions = new BuildScriptGeneratorOptions
            {
                PlatformName = "test",
                PlatformVersion = "1.0.0",
            };
            var generator = CreateDefaultScriptGenerator(new[] { platform1, platform2 }, commonOptions);
            var context = CreateScriptGeneratorContext();

            // Act
            generator.GenerateBashScript(context, out var generatedScript);

            // Assert
            Assert.Contains("ABCDEFG", generatedScript);
            Assert.DoesNotContain("123456", generatedScript);
        }

        [Fact]
        public void GetCompatiblePlatforms_ReturnsOnlyPlatforms_ParticipatingIn_MultiPlatformBuilds()
        {
            // Arrange
            var platform1 = new TestProgrammingPlatform(
                platformName: "lang1",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "ABCDEFG",
                detector: new TestPlatformDetectorSimpleMatch(
                    shouldMatch: true,
                    platformName: "lang1",
                    platformVersion: "1.0.0"));
            var platform2 = new TestProgrammingPlatform(
                platformName: "lang2",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "123456",
                detector: new TestPlatformDetectorSimpleMatch(
                    shouldMatch: true,
                    platformName: "lang2",
                    platformVersion: "1.0.0"),
                platformIsEnabledForMultiPlatformBuild: false); // This platform explicitly opts out

            var commonOptions = new BuildScriptGeneratorOptions
            {
                PlatformName = "lang1",
                PlatformVersion = "1.0.0",
                EnableMultiPlatformBuild = true,
            };
            var detector = CreateDefaultCompatibleDetector(new[] { platform1, platform2 }, commonOptions);
            var context = CreateScriptGeneratorContext();

            // Act
            var compatiblePlatforms = detector.GetCompatiblePlatforms(context);

            // Assert
            Assert.NotNull(compatiblePlatforms);
            Assert.Equal(2, compatiblePlatforms.Count);
        }

        [Fact]
        public void Checkers_AreAppliedCorrectly_WhenCheckersAreEnabled()
        {
            // Arrange
            var repoWarning = new CheckerMessage("some repo warning");
            IChecker[] checkers = { new TestChecker(() => new[] { repoWarning }) };

            var platformVersion = "1.0.0";
            var detector = new TestPlatformDetectorSimpleMatch(true, TestPlatformName, platformVersion);
            var platform = new TestProgrammingPlatform(
                TestPlatformName, new[] { platformVersion }, true, "script-content", detector: detector);

            var commonOptions = new BuildScriptGeneratorOptions
            {
                PlatformName = TestPlatformName,
                PlatformVersion = platformVersion,
                EnableCheckers = true,
            };
            var generator = CreateDefaultScriptGenerator(new[] { platform }, commonOptions, checkers);
            var context = CreateScriptGeneratorContext();

            var messages = new List<ICheckerMessage>();

            // Act
            // Return value of TryGenerateBashScript is irrelevant - messages should be added even if build fails
            generator.GenerateBashScript(context, out var generatedScript, messages);

            // Assert
            Assert.Single(messages);
            Assert.Equal(repoWarning, messages.First());
        }

        [Fact]
        public void Checkers_DontFailTheBuild_WhenTheyThrow()
        {
            // Arrange
            bool checkerRan = false;
            IChecker[] checkers = { new TestChecker(() =>
            {
                checkerRan = true;
                throw new Exception("checker failed");
            }) };

            var platformVersion = "1.0.0";
            var detector = new TestPlatformDetectorSimpleMatch(true, TestPlatformName, platformVersion);
            var scriptContent = "script-content";
            var platform = new TestProgrammingPlatform(
                TestPlatformName, new[] { platformVersion }, true, scriptContent, detector: detector);

            var commonOptions = new BuildScriptGeneratorOptions
            {
                PlatformName = TestPlatformName,
                PlatformVersion = platformVersion,
                EnableCheckers = true,
            };
            var generator = CreateDefaultScriptGenerator(new[] { platform }, commonOptions, checkers);
            var context = CreateScriptGeneratorContext();

            var messages = new List<ICheckerMessage>();

            // Act
            generator.GenerateBashScript(context, out var generatedScript, messages);

            // Assert
            Assert.True(checkerRan);
        }

        private DefaultBuildScriptGenerator CreateDefaultScriptGenerator(
            IProgrammingPlatform platform,
            BuildScriptGeneratorOptions commonOptions)
        {
            return CreateDefaultScriptGenerator(new[] { platform }, commonOptions, checkers: null);
        }

        private DefaultCompatiblePlatformDetector CreateDefaultCompatibleDetector(
            IProgrammingPlatform[] platforms,
            BuildScriptGeneratorOptions commonOptions)
        {
            commonOptions = commonOptions ?? new BuildScriptGeneratorOptions();
            commonOptions.SourceDir = "/app";
            commonOptions.DestinationDir = "/output";

            var configuration = new TestConfiguration();
            configuration[$"{commonOptions.PlatformName}_version"] = commonOptions.PlatformVersion;
            return new DefaultCompatiblePlatformDetector(
                    platforms,
                    NullLogger<DefaultCompatiblePlatformDetector>.Instance,
                    Options.Create(commonOptions));
        }

        private DefaultBuildScriptGenerator CreateDefaultScriptGenerator(
            IProgrammingPlatform[] platforms,
            BuildScriptGeneratorOptions commonOptions,
            IEnumerable<IChecker> checkers = null)
        {
            commonOptions = commonOptions ?? new BuildScriptGeneratorOptions();
            commonOptions.SourceDir = "/app";
            commonOptions.DestinationDir = "/output";
            var defaultPlatformDetector = new DefaultPlatformsInformationProvider(
                platforms,
                new DefaultStandardOutputWriter());
            var envScriptProvider = new BuildScriptGenerator.PlatformsInstallationScriptProvider(
                platforms,
                defaultPlatformDetector,
                new DefaultStandardOutputWriter());
            return new DefaultBuildScriptGenerator(
                defaultPlatformDetector,
                envScriptProvider,
                Options.Create(commonOptions),
                new DefaultCompatiblePlatformDetector(
                    platforms,
                    NullLogger<DefaultCompatiblePlatformDetector>.Instance,
                    Options.Create(commonOptions)),
                checkers,
                NullLogger<DefaultBuildScriptGenerator>.Instance,
                new DefaultStandardOutputWriter(),
                TelemetryClientHelper.GetTelemetryClient());
        }

        private static BuildScriptGeneratorContext CreateScriptGeneratorContext()
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = new TestSourceRepo(),
            };
        }

        [Checker(TestPlatformName)]
        private class TestChecker : IChecker
        {
            private readonly Func<IEnumerable<ICheckerMessage>> _sourceRepoMessageProvider;
            private readonly Func<IEnumerable<ICheckerMessage>> _toolVersionMessageProvider;

            public TestChecker(
                Func<IEnumerable<ICheckerMessage>> repoMessageProvider = null,
                Func<IEnumerable<ICheckerMessage>> toolMessageProvider = null)
            {
                _sourceRepoMessageProvider = repoMessageProvider ?? (() => Enumerable.Empty<ICheckerMessage>());
                _toolVersionMessageProvider = toolMessageProvider ?? (() => Enumerable.Empty<ICheckerMessage>());
            }

            public IEnumerable<ICheckerMessage> CheckSourceRepo(ISourceRepo repo) =>
                _sourceRepoMessageProvider();

            public IEnumerable<ICheckerMessage> CheckToolVersions(IDictionary<string, string> tools) =>
                _toolVersionMessageProvider();
        }

        private class TestSourceRepo : ISourceRepo
        {
            public string RootPath => string.Empty;

            public bool FileExists(params string[] paths)
            {
                throw new NotImplementedException();
            }

            public bool DirExists(params string[] paths)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<string> EnumerateFiles(string searchPattern, bool searchSubDirectories)
            {
                throw new NotImplementedException();
            }

            public string ReadFile(params string[] paths)
            {
                throw new NotImplementedException();
            }

            public string[] ReadAllLines(params string[] paths)
            {
                throw new NotImplementedException();
            }

            public string GetGitCommitId() => null;
        }
    }
}