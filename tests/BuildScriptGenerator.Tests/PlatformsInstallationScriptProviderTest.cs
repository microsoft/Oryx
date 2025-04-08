// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class PlatformsInstallationScriptProviderTest
    {
        [Fact]
        public void DoesNotDetectPlatforms_IfDetectionResultsAlreadyProvided()
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
                installationScriptContent: "install-content",
                detector: detector);
            var envScriptProvider = CreateEnvironmentSetupScriptProvider(new[] { platform });
            var context = CreateScriptGeneratorContext();
            var detectionResults = new[]
            {
                new PlatformDetectorResult
                {
                    Platform = "test",
                    PlatformVersion = "1.0.0",
                }
            };

            // Act
            var setupScript = envScriptProvider.GetBashScriptSnippet(context, detectionResults);

            // Assert
            Assert.False(detector.DetectInvoked);
            Assert.Contains("install-content", setupScript);
            Assert.DoesNotContain("script-content", setupScript);
        }

        [Fact]
        public void ContainsInstallationScriptContent_FromSinglePlatform()
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
                installationScriptContent: "install-content",
                detector: detector);
            var envScriptProvider = CreateEnvironmentSetupScriptProvider(new[] { platform });
            var context = CreateScriptGeneratorContext();

            // Act
            var setupScript = envScriptProvider.GetBashScriptSnippet(context);

            // Assert
            Assert.Contains("install-content", setupScript);
            Assert.True(detector.DetectInvoked);
            Assert.DoesNotContain("script-content", setupScript);
        }

        [Fact]
        public void ContainsInstallationScriptContent_FromMultiplePlatforms()
        {
            // Arrange
            var detector1 = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "test1",
                detectedPlatformVersion: "1.0.0");
            var platform1 = new TestProgrammingPlatform(
                "test1",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script1-content",
                installationScriptContent: "install1-content",
                detector: detector1);
            var detector2 = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "test2",
                detectedPlatformVersion: "1.0.0");
            var platform2 = new TestProgrammingPlatform(
                "test2",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script2-content",
                installationScriptContent: "install2-content",
                detector: detector2);
            var envScriptProvider = CreateEnvironmentSetupScriptProvider(new[] { platform1, platform2 });
            var context = CreateScriptGeneratorContext();

            // Act
            var setupScript = envScriptProvider.GetBashScriptSnippet(context);

            // Assert
            Assert.Contains("install1-content", setupScript);
            Assert.Contains("install2-content", setupScript);
            Assert.DoesNotContain("script1-content", setupScript);
            Assert.DoesNotContain("script2-content", setupScript);
            Assert.True(detector1.DetectInvoked);
            Assert.True(detector2.DetectInvoked);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void DoesNotFailIfPlatformDoesNotReturnInstallationScriptSnippet(
            string installationScriptContent)
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
                installationScriptContent: installationScriptContent,
                detector: detector);
            var envScriptProvider = CreateEnvironmentSetupScriptProvider(new[] { platform });
            var context = CreateScriptGeneratorContext();

            // Act
            var setupScript = envScriptProvider.GetBashScriptSnippet(context);

            // Assert
            Assert.True(detector.DetectInvoked);
            Assert.DoesNotContain("script-content", setupScript);
        }

        private PlatformsInstallationScriptProvider CreateEnvironmentSetupScriptProvider(
            IEnumerable<IProgrammingPlatform> platforms)
        {
            var platformDetector = new DefaultPlatformsInformationProvider(
                platforms,
                new DefaultStandardOutputWriter(),
                Options.Create(new BuildScriptGeneratorOptions()));
            return new PlatformsInstallationScriptProvider(
                platforms,
                platformDetector,
                new DefaultStandardOutputWriter());
        }

        private static BuildScriptGeneratorContext CreateScriptGeneratorContext()
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = new MemorySourceRepo(),
            };
        }
    }
}
