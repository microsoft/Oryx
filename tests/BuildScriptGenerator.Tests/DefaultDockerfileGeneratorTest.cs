// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class DefaultDockerfileGeneratorTest : IClassFixture<TestTempDirTestFixture>
    {
        private const string _buildImageFormat = "mcr.microsoft.com/oryx/build:{0}";
        private const string _argRuntimeFormat = "ARG RUNTIME={0}:{1}";

        private readonly string _tempDirRoot;

        public DefaultDockerfileGeneratorTest(TestTempDirTestFixture testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }

        [Fact]
        public void GenerateDockerfile_Throws_IfNoPlatformIsCompatible()
        {
            // Arrange
            var generator = CreateDefaultDockerfileGenerator(platforms: new IProgrammingPlatform[] { });
            var ctx = CreateDockerfileContext(null, null);

            // Act & Assert
            var exception = Assert.Throws<UnsupportedPlatformException>(
                () => generator.GenerateDockerfile(ctx));
        }

        [Theory]
        [InlineData("dotnet", "2.0", "latest")]
        [InlineData("dotnet", "2.1", "slim")]
        [InlineData("dotnet", "3.0", "latest")]
        [InlineData("nodejs", "6", "latest")]
        [InlineData("nodejs", "8", "slim")]
        [InlineData("nodejs", "10", "slim")]
        [InlineData("nodejs", "12", "slim")]
        [InlineData("php", "5.6", "latest")]
        [InlineData("php", "7.3", "latest")]
        [InlineData("python", "2.7", "latest")]
        [InlineData("python", "3.7", "slim")]
        [InlineData("python", "3.8", "slim")]
        public void GenerateDockerfile_GeneratesBuildTagAndRuntime_ForProvidedPlatformAndVersion(
            string platformName,
            string platformVersion,
            string expectedBuildTag)
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: platformName,
                detectedPlatformVersion: platformVersion);
            var platform = new TestProgrammingPlatform(
                platformName,
                new[] { platformVersion },
                detector: detector);
            var generator = CreateDefaultDockerfileGenerator(platform);
            var ctx = CreateDockerfileContext(platformName, platformVersion);

            // Act
            var dockerfile = generator.GenerateDockerfile(ctx);

            // Assert
            Assert.NotNull(dockerfile);
            Assert.NotEqual(string.Empty, dockerfile);
            Assert.Contains(string.Format(_buildImageFormat, expectedBuildTag), dockerfile);
            Assert.Contains(string.Format(_argRuntimeFormat,
                ConvertToRuntimeName(platformName),
                platformVersion),
                dockerfile);
            Assert.False(detector.DetectInvoked);
        }

        [Theory]
        [InlineData("dotnet", "2.0", "latest")]
        [InlineData("dotnet", "2.1", "slim")]
        [InlineData("dotnet", "3.0", "latest")]
        [InlineData("nodejs", "6", "latest")]
        [InlineData("nodejs", "8", "slim")]
        [InlineData("nodejs", "10", "slim")]
        [InlineData("nodejs", "12", "slim")]
        [InlineData("php", "5.6", "latest")]
        [InlineData("php", "7.3", "latest")]
        [InlineData("python", "2.7", "latest")]
        [InlineData("python", "3.7", "slim")]
        [InlineData("python", "3.8", "slim")]
        public void GenerateDockerfile_GeneratesBuildTagAndRuntime_ForProvidedPlatform(
            string platformName,
            string detectedPlatformVersion,
            string expectedBuildTag)
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: platformName,
                detectedPlatformVersion: detectedPlatformVersion);
            var platform = new TestProgrammingPlatform(
                platformName,
                new[] { detectedPlatformVersion },
                detector: detector);
            var generator = CreateDefaultDockerfileGenerator(platform);
            var ctx = CreateDockerfileContext(platformName, null);

            // Act
            var dockerfile = generator.GenerateDockerfile(ctx);

            // Assert
            Assert.NotNull(dockerfile);
            Assert.NotEqual(string.Empty, dockerfile);
            Assert.Contains(string.Format(_buildImageFormat, expectedBuildTag), dockerfile);
            Assert.Contains(string.Format(_argRuntimeFormat,
                ConvertToRuntimeName(platformName),
                detectedPlatformVersion),
                dockerfile);
            Assert.True(detector.DetectInvoked);
        }

        [Theory]
        [InlineData("dotnet", "2.0", "latest")]
        [InlineData("dotnet", "2.1", "slim")]
        [InlineData("dotnet", "3.0", "latest")]
        [InlineData("nodejs", "6", "latest")]
        [InlineData("nodejs", "8", "slim")]
        [InlineData("nodejs", "10", "slim")]
        [InlineData("nodejs", "12", "slim")]
        [InlineData("php", "5.6", "latest")]
        [InlineData("php", "7.3", "latest")]
        [InlineData("python", "2.7", "latest")]
        [InlineData("python", "3.7", "slim")]
        [InlineData("python", "3.8", "slim")]
        public void GenerateDockerfile_GeneratesBuildTagAndRuntime_ForNoProvidedPlatform(
            string detectedPlatformName,
            string detectedPlatformVersion,
            string expectedBuildTag)
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: detectedPlatformName,
                detectedPlatformVersion: detectedPlatformVersion);
            var platform = new TestProgrammingPlatform(
                detectedPlatformName,
                new[] { detectedPlatformVersion },
                detector: detector);
            var generator = CreateDefaultDockerfileGenerator(platform);
            var ctx = CreateDockerfileContext(null, null);

            // Act
            var dockerfile = generator.GenerateDockerfile(ctx);

            // Assert
            Assert.NotNull(dockerfile);
            Assert.NotEqual(string.Empty, dockerfile);
            Assert.Contains(string.Format(_buildImageFormat, expectedBuildTag), dockerfile);
            Assert.Contains(string.Format(_argRuntimeFormat,
                ConvertToRuntimeName(detectedPlatformName),
                detectedPlatformVersion),
                dockerfile);
            Assert.True(detector.DetectInvoked);
        }

        [Theory]
        [InlineData("nodejs", "8", "dotnet", "2.1", "slim")]
        [InlineData("nodejs", "8", "dotnet", "3.0", "latest")]
        [InlineData("nodejs", "12", "dotnet", "2.1", "slim")]
        [InlineData("nodejs", "12", "dotnet", "3.0", "latest")]
        [InlineData("nodejs", "8", "python", "3.7", "slim")]
        [InlineData("nodejs", "8", "python", "2.7", "latest")]
        [InlineData("python", "3.7", "dotnet", "2.1", "slim")]
        [InlineData("python", "3.7", "dotnet", "3.0", "latest")]
        [InlineData("dotnet", "2.1", "php", "5.6", "latest")]
        public void GenerateDockerfile_GeneratesBuildTagAndRuntime_ForMultiPlatformBuild(
            string platformName,
            string platformVersion,
            string runtimePlatformName,
            string runtimePlatformVersion,
            string expectedBuildTag)
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: platformName,
                detectedPlatformVersion: platformVersion);
            var platform = new TestProgrammingPlatform(
                platformName,
                new[] { platformVersion },
                detector: detector);

            var runtimeDetector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: runtimePlatformName,
                detectedPlatformVersion: runtimePlatformVersion);
            var runtimePlatform = new TestProgrammingPlatform(
                runtimePlatformName,
                new[] { runtimePlatformVersion },
                detector: runtimeDetector);

            var generator = CreateDefaultDockerfileGenerator(new[] { platform, runtimePlatform });
            var ctx = CreateDockerfileContext(null, null, true);

            // Act
            var dockerfile = generator.GenerateDockerfile(ctx);

            // Assert
            Assert.NotNull(dockerfile);
            Assert.NotEqual(string.Empty, dockerfile);
            Assert.Contains(string.Format(_buildImageFormat, expectedBuildTag), dockerfile);
            Assert.Contains(string.Format(_argRuntimeFormat,
                ConvertToRuntimeName(runtimePlatformName),
                runtimePlatformVersion),
                dockerfile);
        }

        private DockerfileContext CreateDockerfileContext(
            string platformName,
            string platformVersion,
            bool enableMultiePlatformBuild = false)
        {
            return new DockerfileContext()
            {
                Platform = platformName,
                PlatformVersion = platformVersion,
                DisableMultiPlatformBuild = !enableMultiePlatformBuild,
            };
        }

        private DefaultDockerfileGenerator CreateDefaultDockerfileGenerator(IProgrammingPlatform platform)
        {
            return CreateDefaultDockerfileGenerator(new[] { platform });
        }

        private DefaultDockerfileGenerator CreateDefaultDockerfileGenerator(IProgrammingPlatform[] platforms)
        {
            return new DefaultDockerfileGenerator(
                new DefaultCompatiblePlatformDetector(platforms, NullLogger<DefaultCompatiblePlatformDetector>.Instance),
                NullLogger<DefaultDockerfileGenerator>.Instance);
        }

        private TestPlatformDetectorUsingPlatformName CreateTestLanguageDetector(string name, string version)
        {
            return new TestPlatformDetectorUsingPlatformName(name, version);
        }

        private string ConvertToRuntimeName(string platformName)
        {
            if (string.Equals(platformName, DotNetCoreConstants.PlatformName, StringComparison.OrdinalIgnoreCase))
            {
                platformName = "dotnetcore";
            }

            if (string.Equals(platformName, NodeConstants.PlatformName, StringComparison.OrdinalIgnoreCase))
            {
                platformName = "node";
            }

            return platformName;
        }
    }
}
