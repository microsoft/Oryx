// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Detector;
using Moq;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class DefaultPlatformDetectorTest
    {
        [Fact]
        public void RunsDetectionOnAllEnabledPlatforms()
        {
            // Arrange
            var platform1 = new Mock<IProgrammingPlatform>();
            platform1
                .Setup(p => p.Detect(It.IsAny<RepositoryContext>()))
                .Returns(new PlatformDetectorResult { Platform = "platform1", PlatformVersion = "1.0.0" });
            platform1
                .Setup(p => p.IsEnabled(It.IsAny<RepositoryContext>()))
                .Returns(true);
            var platform2 = new Mock<IProgrammingPlatform>();
            platform2
                .Setup(p => p.Detect(It.IsAny<RepositoryContext>()))
                .Returns(new PlatformDetectorResult { Platform = "platform2", PlatformVersion = "1.0.0" });
            platform2
                .Setup(p => p.IsEnabled(It.IsAny<RepositoryContext>()))
                .Returns(true);
            var detector = CreatePlatformDetector(new[] { platform1.Object, platform2.Object });
            var context = CreateScriptGeneratorContext();

            // Act
            var actualResults = detector.GetPlatformsInfo(context);

            // Assert
            Assert.NotNull(actualResults);
            Assert.Equal(2, actualResults.Count());
            var actualDetectedResults = actualResults.Select(pi => pi.DetectorResult);
            Assert.Equal("platform1", actualDetectedResults.ElementAt(0).Platform);
            Assert.Equal("1.0.0", actualDetectedResults.ElementAt(0).PlatformVersion);
            Assert.Equal("platform2", actualDetectedResults.ElementAt(1).Platform);
            Assert.Equal("1.0.0", actualDetectedResults.ElementAt(1).PlatformVersion);
        }

        [Fact]
        public void RunsDetectionOnEnabledPlatformsOnly()
        {
            // Arrange
            var platform1 = new Mock<IProgrammingPlatform>();
            platform1
                .Setup(p => p.Detect(It.IsAny<RepositoryContext>()))
                .Returns(new PlatformDetectorResult { Platform = "platform1", PlatformVersion = "1.0.0" });
            platform1
                .Setup(p => p.IsEnabled(It.IsAny<RepositoryContext>()))
                .Returns(true);
            var platform2 = new Mock<IProgrammingPlatform>();
            platform2
                .Setup(p => p.Detect(It.IsAny<RepositoryContext>()))
                .Returns(new PlatformDetectorResult { Platform = "platform2", PlatformVersion = "1.0.0" });
            platform2
                .Setup(p => p.IsEnabled(It.IsAny<RepositoryContext>()))
                .Returns(false);
            var detector = CreatePlatformDetector(new[] { platform1.Object, platform2.Object });
            var context = CreateScriptGeneratorContext();

            // Act
            var actualResults = detector.GetPlatformsInfo(context);

            // Assert
            Assert.NotNull(actualResults);
            var actualResult = Assert.Single(actualResults);
            Assert.Equal("platform1", actualResult.DetectorResult.Platform);
            Assert.Equal("1.0.0", actualResult.DetectorResult.PlatformVersion);
        }

        [Fact]
        public void DoesNoFailIfPlatformDetectorReturnsNull()
        {
            // Arrange
            var platform1 = new Mock<IProgrammingPlatform>();
            platform1
                .Setup(p => p.Detect(It.IsAny<RepositoryContext>()))
                .Returns(value: null);
            platform1
                .Setup(p => p.IsEnabled(It.IsAny<RepositoryContext>()))
                .Returns(true);
            var platform2 = new Mock<IProgrammingPlatform>();
            platform2
                .Setup(p => p.Detect(It.IsAny<RepositoryContext>()))
                .Returns(new PlatformDetectorResult { Platform = "platform2", PlatformVersion = "1.0.0" });
            platform2
                .Setup(p => p.IsEnabled(It.IsAny<RepositoryContext>()))
                .Returns(true);
            var detector = CreatePlatformDetector(new[] { platform1.Object, platform2.Object });
            var context = CreateScriptGeneratorContext();

            // Act
            var actualResults = detector.GetPlatformsInfo(context);

            // Assert
            Assert.NotNull(actualResults);
            var actualResult = Assert.Single(actualResults);
            Assert.Equal("platform2", actualResult.DetectorResult.Platform);
            Assert.Equal("1.0.0", actualResult.DetectorResult.PlatformVersion);
        }

        private DefaultPlatformsInformationProvider CreatePlatformDetector(
            IEnumerable<IProgrammingPlatform> platforms)
        {
            return CreatePlatformDetector(platforms, new BuildScriptGeneratorOptions());
        }

        private DefaultPlatformsInformationProvider CreatePlatformDetector(
            IEnumerable<IProgrammingPlatform> platforms,
            BuildScriptGeneratorOptions options)
        {
            return new DefaultPlatformsInformationProvider(
                platforms,
                new DefaultStandardOutputWriter(),
                NullLogger<DefaultPlatformsInformationProvider>.Instance,
                Options.Create(options));
        }

        [Fact]
        public void SkipDetection_OnlyDetectsSpecifiedPlatform()
        {
            // Arrange
            var platform1 = new Mock<IProgrammingPlatform>();
            platform1.Setup(p => p.Name).Returns("nodejs");
            platform1
                .Setup(p => p.Detect(It.IsAny<RepositoryContext>()))
                .Returns(new PlatformDetectorResult { Platform = "nodejs", PlatformVersion = "18.0.0" });
            platform1
                .Setup(p => p.IsEnabled(It.IsAny<RepositoryContext>()))
                .Returns(true);

            var platform2 = new Mock<IProgrammingPlatform>();
            platform2.Setup(p => p.Name).Returns("python");
            platform2
                .Setup(p => p.Detect(It.IsAny<RepositoryContext>()))
                .Returns(new PlatformDetectorResult { Platform = "python", PlatformVersion = "3.10.0" });
            platform2
                .Setup(p => p.IsEnabled(It.IsAny<RepositoryContext>()))
                .Returns(true);

            var platform3 = new Mock<IProgrammingPlatform>();
            platform3.Setup(p => p.Name).Returns("dotnet");
            platform3
                .Setup(p => p.Detect(It.IsAny<RepositoryContext>()))
                .Returns(new PlatformDetectorResult { Platform = "dotnet", PlatformVersion = "6.0.0" });
            platform3
                .Setup(p => p.IsEnabled(It.IsAny<RepositoryContext>()))
                .Returns(true);

            var options = new BuildScriptGeneratorOptions
            {
                SkipDetection = true,
                PlatformName = "nodejs",
            };

            var detector = CreatePlatformDetector(
                new[] { platform1.Object, platform2.Object, platform3.Object },
                options);
            var context = CreateScriptGeneratorContext();

            // Act
            var actualResults = detector.GetPlatformsInfo(context);

            // Assert
            Assert.NotNull(actualResults);
            var actualResult = Assert.Single(actualResults);
            Assert.Equal("nodejs", actualResult.DetectorResult.Platform);
            Assert.Equal("18.0.0", actualResult.DetectorResult.PlatformVersion);

            // Verify that Detect was NOT called on the other platforms
            platform2.Verify(p => p.Detect(It.IsAny<RepositoryContext>()), Times.Never);
            platform3.Verify(p => p.Detect(It.IsAny<RepositoryContext>()), Times.Never);
        }

        [Fact]
        public void SkipDetection_False_DetectsAllPlatforms()
        {
            // Arrange
            var platform1 = new Mock<IProgrammingPlatform>();
            platform1.Setup(p => p.Name).Returns("nodejs");
            platform1
                .Setup(p => p.Detect(It.IsAny<RepositoryContext>()))
                .Returns(new PlatformDetectorResult { Platform = "nodejs", PlatformVersion = "18.0.0" });
            platform1
                .Setup(p => p.IsEnabled(It.IsAny<RepositoryContext>()))
                .Returns(true);

            var platform2 = new Mock<IProgrammingPlatform>();
            platform2.Setup(p => p.Name).Returns("python");
            platform2
                .Setup(p => p.Detect(It.IsAny<RepositoryContext>()))
                .Returns(new PlatformDetectorResult { Platform = "python", PlatformVersion = "3.10.0" });
            platform2
                .Setup(p => p.IsEnabled(It.IsAny<RepositoryContext>()))
                .Returns(true);

            var options = new BuildScriptGeneratorOptions
            {
                SkipDetection = false,
                PlatformName = "nodejs",
            };

            var detector = CreatePlatformDetector(
                new[] { platform1.Object, platform2.Object },
                options);
            var context = CreateScriptGeneratorContext();

            // Act
            var actualResults = detector.GetPlatformsInfo(context);

            // Assert — both platforms should be detected
            Assert.NotNull(actualResults);
            Assert.Equal(2, actualResults.Count());
            platform1.Verify(p => p.Detect(It.IsAny<RepositoryContext>()), Times.Once);
            platform2.Verify(p => p.Detect(It.IsAny<RepositoryContext>()), Times.Once);
        }

        [Fact]
        public void SkipDetection_StillRespectsDisabledPlatform()
        {
            // Arrange — the specified platform is disabled
            var platform1 = new Mock<IProgrammingPlatform>();
            platform1.Setup(p => p.Name).Returns("nodejs");
            platform1
                .Setup(p => p.Detect(It.IsAny<RepositoryContext>()))
                .Returns(new PlatformDetectorResult { Platform = "nodejs", PlatformVersion = "18.0.0" });
            platform1
                .Setup(p => p.IsEnabled(It.IsAny<RepositoryContext>()))
                .Returns(false);

            var options = new BuildScriptGeneratorOptions
            {
                SkipDetection = true,
                PlatformName = "nodejs",
            };

            var detector = CreatePlatformDetector(new[] { platform1.Object }, options);
            var context = CreateScriptGeneratorContext();

            // Act
            var actualResults = detector.GetPlatformsInfo(context);

            // Assert — platform is disabled, so no results even with skip-detection
            Assert.NotNull(actualResults);
            Assert.Empty(actualResults);
            platform1.Verify(p => p.Detect(It.IsAny<RepositoryContext>()), Times.Never);
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