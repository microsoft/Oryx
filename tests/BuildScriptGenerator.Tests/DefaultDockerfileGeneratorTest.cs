// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class DefaultDockerfileGeneratorTest : IClassFixture<TestTempDirTestFixture>
    {
        private const string _buildImageFormat = "mcr.microsoft.com/oryx/{0}:{1}";
        private const string _argRuntimeFormat = "ARG RUNTIME={0}:{1}";

        private const string _buildImageName = "cli";
        private const string _buildImageTag = "stable";

        private readonly string _tempDirRoot;

        public DefaultDockerfileGeneratorTest(TestTempDirTestFixture testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }

        [Fact]
        public void GenerateDockerfile_Throws_IfNoPlatformIsCompatible()
        {
            // Arrange
            var commonOptions = new BuildScriptGeneratorOptions();
            var generator = CreateDefaultDockerfileGenerator(platforms: new IProgrammingPlatform[] { }, commonOptions);
            var ctx = CreateDockerfileContext();

            // Act & Assert
            var exception = Assert.Throws<UnsupportedPlatformException>(
                () => generator.GenerateDockerfile(ctx));
        }

        /// <summary>
        /// Tests that the platform name and version provided will be used directly in the generated Dockerfile.
        /// This scenario ensures that outdated images, or images that haven't generated the new list of
        /// supported runtime versions, will respect the provided runtime version.
        /// </summary>
        /// <param name="platformName">The name of the platform for the build and run.</param>
        /// <param name="detectedPlatformVersion">The platform version that is "detected".</param>
        /// <param name="commandArgPlatformVersion">The version provided to the "oryx dockerfile" command.</param>
        /// <param name="expectedRuntimeImageTag">The expected runtime tag of the Dockerfile produced.</param>
        [Theory]
        [InlineData("dotnet", "2.0", "2.0", "2.0")]
        [InlineData("dotnet", "2.1", "2.1", "2.1")]
        [InlineData("dotnet", "3.0", "3.0",  "3.0")]
        [InlineData("nodejs", "6", "6", "6")]
        [InlineData("nodejs", "8", "8", "8")]
        [InlineData("nodejs", "10", "10", "10")]
        [InlineData("nodejs", "12", "12", "12")]
        [InlineData("php", "5.6", "5.6", "5.6")]
        [InlineData("php", "7.3", "7.3", "7.3")]
        [InlineData("python", "2.7", "2.7", "2.7")]
        [InlineData("python", "3.7", "3.7", "3.7")]
        [InlineData("python", "3.8", "3.8", "3.8")]
        public void GenerateDockerfile_GeneratesBuildTagAndRuntime_ForProvidedPlatformAndVersion(
            string platformName,
            string detectedPlatformVersion,
            string commandArgPlatformVersion,
            string expectedRuntimeImageTag)
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: platformName,
                detectedPlatformVersion: detectedPlatformVersion);
            var platform = new TestProgrammingPlatform(
                platformName,
                new string[] {},
                detector: detector);
            var commonOptions = new BuildScriptGeneratorOptions
            {
                PlatformName = platformName,
                PlatformVersion = commandArgPlatformVersion,
                RuntimePlatformName = platformName,
                RuntimePlatformVersion = commandArgPlatformVersion,
            };
            var generator = CreateDefaultDockerfileGenerator(platform, commonOptions);
            var ctx = CreateDockerfileContext();

            // Act
            var dockerfile = generator.GenerateDockerfile(ctx);

            // Assert
            Assert.NotNull(dockerfile);
            Assert.NotEqual(string.Empty, dockerfile);
            Assert.Contains(string.Format(_buildImageFormat, _buildImageName, _buildImageTag), dockerfile);
            Assert.Contains(string.Format(_argRuntimeFormat,
                ConvertToRuntimeName(platformName),
                expectedRuntimeImageTag),
                dockerfile);
            Assert.True(detector.DetectInvoked);
        }

        /// <summary>
        /// Tests that the correct runtime version will be used for the detected platform version for the
        /// given platform name. If the max satisfying supported version found for the detected version isn't
        /// available in the runtime (or within the semver spec), we'll default to the latest version.
        /// </summary>
        /// <param name="platformName">The name of the platform for the build and run.</param>
        /// <param name="detectedPlatformVersion">The platform version that is "detected".</param>
        /// <param name="expectedRuntimeImageTag">The expected runtime tag of the Dockerfile produced.</param>
        [Theory]
        [InlineData("dotnet", "2.0", "2.0")]
        [InlineData("dotnet", "2.1", "2.1")]
        [InlineData("dotnet", "3.0", "3.0")]
        [InlineData("nodejs", "6", "6")]
        [InlineData("nodejs", "8", "8")]
        [InlineData("nodejs", "10", "10")]
        [InlineData("nodejs", "12", "12")]
        [InlineData("nodejs", "~12", "12")] // Test semver spec
        [InlineData("nodejs", "~8", "8")] // Test semver spec 
        [InlineData("nodejs", "<13", "12")] // Test semver
        [InlineData("php", "5.6", "5.6")]
        [InlineData("php", "7.3", "7.3")]
        [InlineData("python", "2.7", "2.7")]
        [InlineData("python", "3.7", "3.11")] // 3.7.x not currently a runtime, use latest
        [InlineData("python", "3.8.1", "3.8")]
        public void GenerateDockerfile_GeneratesBuildTagAndRuntime_ForProvidedPlatform(
            string platformName,
            string detectedPlatformVersion,
            string expectedRuntimeImageTag)
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: platformName,
                detectedPlatformVersion: detectedPlatformVersion);
            var platform = new TestProgrammingPlatform(
                platformName,
                new string[] {},
                detector: detector);
            var commonOptions = new BuildScriptGeneratorOptions
            {
                PlatformName = platformName,
            };
            var generator = CreateDefaultDockerfileGenerator(platform, commonOptions);
            var ctx = CreateDockerfileContext();

            // Act
            var dockerfile = generator.GenerateDockerfile(ctx);

            // Assert
            Assert.NotNull(dockerfile);
            Assert.NotEqual(string.Empty, dockerfile);
            Assert.Contains(string.Format(_buildImageFormat, _buildImageName, _buildImageTag), dockerfile);
            Assert.Contains(string.Format(_argRuntimeFormat,
                ConvertToRuntimeName(platformName),
                expectedRuntimeImageTag),
                dockerfile);
            Assert.True(detector.DetectInvoked);
        }

        /// <summary>
        /// Tests that the correct runtime version will be used for the detected platform version for the
        /// given platform name. If the max satisfying supported version found for the detected version isn't
        /// available in the runtime (or within the semver spec), we'll default to the latest version.
        /// </summary>
        /// <param name="platformName">The name of the platform for the build and run.</param>
        /// <param name="detectedPlatformVersion">The platform version that is "detected".</param>
        /// <param name="expectedRuntimeImageTag">The expected runtime tag of the Dockerfile produced.</param>
        [Theory]
        [InlineData("dotnet", "5.0.17", "5.0")]
        [InlineData("dotnet", "2.1", "2.1")]
        [InlineData("dotnet", "3.0", "3.0")]
        [InlineData("nodejs", "6", "6")]
        [InlineData("nodejs", "8", "8")]
        [InlineData("nodejs", "10", "10")]
        [InlineData("nodejs", "12", "12")]
        [InlineData("nodejs", "~12", "12")] // Test semver spec
        [InlineData("nodejs", "~8", "8")] // Test semver spec 
        [InlineData("nodejs", "<13", "12")] // Test semver
        [InlineData("php", "5.6", "5.6")]
        [InlineData("php", "7.3", "7.3")]
        [InlineData("python", "2.7", "2.7")]
        [InlineData("python", "3.7", "3.11")] // 3.7.x not currently a runtime, use latest
        [InlineData("python", "3.8.1", "3.8")]
        public void GenerateDockerfile_GeneratesBuildTagAndRuntime_ForNoProvidedPlatform(
            string platformName,
            string detectedPlatformVersion,
            string expectedRuntimeImageTag)
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: platformName,
                detectedPlatformVersion: detectedPlatformVersion);
            var platform = new TestProgrammingPlatform(
                platformName,
                new string[] {},
                detector: detector);
            var commonOptions = new BuildScriptGeneratorOptions();
            var generator = CreateDefaultDockerfileGenerator(platform, commonOptions);
            var ctx = CreateDockerfileContext();

            // Act
            var dockerfile = generator.GenerateDockerfile(ctx);

            // Assert
            Assert.NotNull(dockerfile);
            Assert.NotEqual(string.Empty, dockerfile);
            Assert.Contains(string.Format(_buildImageFormat, _buildImageName, _buildImageTag), dockerfile);
            Assert.Contains(string.Format(_argRuntimeFormat,
                ConvertToRuntimeName(platformName),
                expectedRuntimeImageTag),
                dockerfile);
            Assert.True(detector.DetectInvoked);
        }

        [Theory]
        [InlineData("nodejs", "8", "dotnet", "2.1")]
        [InlineData("nodejs", "8", "dotnet", "3.0")]
        [InlineData("nodejs", "12", "dotnet", "2.1")]
        [InlineData("nodejs", "12", "dotnet", "3.0")]
        [InlineData("nodejs", "8", "python", "3.7")]
        [InlineData("nodejs", "8", "python", "2.7")]
        [InlineData("python", "3.7", "dotnet", "2.1")]
        [InlineData("python", "3.7", "dotnet", "3.0")]
        [InlineData("dotnet", "2.1", "php", "5.6")]
        public void GenerateDockerfile_GeneratesBuildTagAndRuntime_ForMultiPlatformBuild(
            string platformName,
            string platformVersion,
            string runtimePlatformName,
            string runtimePlatformVersion)
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
            var commonOptions = new BuildScriptGeneratorOptions
            {
                EnableMultiPlatformBuild = true,
                PlatformName = platformName,
                PlatformVersion = platformVersion,
                RuntimePlatformName = runtimePlatformName,
                RuntimePlatformVersion = runtimePlatformVersion,
            };
            var generator = CreateDefaultDockerfileGenerator(new[] { platform, runtimePlatform }, commonOptions);
            var ctx = CreateDockerfileContext();

            // Act
            var dockerfile = generator.GenerateDockerfile(ctx);

            // Assert
            Assert.NotNull(dockerfile);
            Assert.NotEqual(string.Empty, dockerfile);
            Assert.Contains(string.Format(_buildImageFormat, _buildImageName, _buildImageTag), dockerfile);
            Assert.Contains(string.Format(_argRuntimeFormat,
                ConvertToRuntimeName(runtimePlatformName),
                runtimePlatformVersion),
                dockerfile);
        }

        /// <summary>
        /// Tests that the platform name and version provided will be used directly in the generated Dockerfile
        /// and uses the provided build image and tag in the Dockerfile.
        /// </summary>
        /// <param name="platformName">The name of the platform for the build and run.</param>
        /// <param name="detectedPlatformVersion">The platform version that is "detected".</param>
        /// <param name="commandArgPlatformVersion">The version provided to the "oryx dockerfile" command.</param>
        /// <param name="expectedRuntimeImageTag">The expected runtime tag of the Dockerfile produced.</param>
        [Theory]
        [InlineData("dotnet", "2.0", "2.0", "2.0")]
        public void GenerateDockerfile_GeneratesBuildTagAndRuntime_ForProvidedPlatformAndVersion_AndProvidedBuildImage(
            string platformName,
            string detectedPlatformVersion,
            string commandArgPlatformVersion,
            string expectedRuntimeImageTag)
        {
            // Arrange
            var buildImageName = "build";
            var buildImageTag = "latest";
            var buildImage = $"{buildImageName}:{buildImageTag}";
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: platformName,
                detectedPlatformVersion: detectedPlatformVersion);
            var platform = new TestProgrammingPlatform(
                platformName,
                new string[] { },
                detector: detector);
            var commonOptions = new BuildScriptGeneratorOptions
            {
                BuildImage = buildImage,
                PlatformName = platformName,
                PlatformVersion = commandArgPlatformVersion,
                RuntimePlatformName = platformName,
                RuntimePlatformVersion = commandArgPlatformVersion,
            };
            var generator = CreateDefaultDockerfileGenerator(platform, commonOptions);
            var ctx = CreateDockerfileContext();

            // Act
            var dockerfile = generator.GenerateDockerfile(ctx);

            // Assert
            Assert.NotNull(dockerfile);
            Assert.NotEqual(string.Empty, dockerfile);
            Assert.Contains(string.Format(_buildImageFormat, buildImageName, buildImageTag), dockerfile);
            Assert.Contains(string.Format(_argRuntimeFormat,
                ConvertToRuntimeName(platformName),
                expectedRuntimeImageTag),
                dockerfile);
            Assert.True(detector.DetectInvoked);
        }

        /// <summary>
        /// Tests that the platform name and version provided will be used directly in the generated Dockerfile
        /// and uses the provided bind port as a part of the 'oryx create-script' call in the Dockerfile.
        /// </summary>
        /// <param name="platformName">The name of the platform for the build and run.</param>
        /// <param name="detectedPlatformVersion">The platform version that is "detected".</param>
        /// <param name="commandArgPlatformVersion">The version provided to the "oryx dockerfile" command.</param>
        /// <param name="expectedRuntimeImageTag">The expected runtime tag of the Dockerfile produced.</param>
        [Theory]
        [InlineData("dotnet", "2.0", "2.0", "2.0")]
        public void GenerateDockerfile_GeneratesBuildTagAndRuntime_ForProvidedPlatformAndVersion_AndProvidedBindPort(
            string platformName,
            string detectedPlatformVersion,
            string commandArgPlatformVersion,
            string expectedRuntimeImageTag)
        {
            // Arrange
            var bindPort = "8080";
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: platformName,
                detectedPlatformVersion: detectedPlatformVersion);
            var platform = new TestProgrammingPlatform(
                platformName,
                new string[] { },
                detector: detector);
            var commonOptions = new BuildScriptGeneratorOptions
            {
                BindPort = bindPort,
                PlatformName = platformName,
                PlatformVersion = commandArgPlatformVersion,
                RuntimePlatformName = platformName,
                RuntimePlatformVersion = commandArgPlatformVersion,
            };
            var generator = CreateDefaultDockerfileGenerator(platform, commonOptions);
            var ctx = CreateDockerfileContext();

            // Act
            var dockerfile = generator.GenerateDockerfile(ctx);

            // Assert
            Assert.NotNull(dockerfile);
            Assert.NotEqual(string.Empty, dockerfile);
            Assert.Contains(string.Format(_buildImageFormat, _buildImageName, _buildImageTag), dockerfile);
            Assert.Contains(string.Format(_argRuntimeFormat,
                ConvertToRuntimeName(platformName),
                expectedRuntimeImageTag),
                dockerfile);
            Assert.Contains($"oryx create-script -bindPort {bindPort}", dockerfile);
            Assert.True(detector.DetectInvoked);
        }

        [Theory]
        [InlineData("golang", "1.17")]
        [InlineData("java", "11.0.14")]
        public void GenerateDockerfile_FailsForUnsupportedPlatform(
            string platformName,
            string detectedPlatformVersion)
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: platformName,
                detectedPlatformVersion: detectedPlatformVersion);
            var platform = new TestProgrammingPlatform(
                platformName,
                new string[] { },
                detector: detector);
            var commonOptions = new BuildScriptGeneratorOptions();
            var generator = CreateDefaultDockerfileGenerator(platform, commonOptions);
            var ctx = CreateDockerfileContext();

            // Act & Assert
            var exception = Assert.Throws<InvalidDockerfileImageException>(() => generator.GenerateDockerfile(ctx));
            Assert.Contains("--runtime-platform argument is empty", exception.Message);
        }

        [Theory]
        [InlineData("golang", "1.17")]
        [InlineData("java", "11.0.14")]
        public void GenerateDockerfile_FailsForUnsupportedPlatformVersion(
            string platformName,
            string detectedPlatformVersion)
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: platformName,
                detectedPlatformVersion: detectedPlatformVersion);
            var platform = new TestProgrammingPlatform(
                platformName,
                new string[] { },
                detector: detector);
            var commonOptions = new BuildScriptGeneratorOptions
            {
                RuntimePlatformName = platformName,
            };
            var generator = CreateDefaultDockerfileGenerator(platform, commonOptions);
            var ctx = CreateDockerfileContext();

            // Act & Assert
            var exception = Assert.Throws<InvalidDockerfileImageException>(() => generator.GenerateDockerfile(ctx));
            Assert.Contains("--runtime-platform-version argument is empty", exception.Message);
        }

        private DockerfileContext CreateDockerfileContext()
        {
            return new DockerfileContext();
        }

        private DefaultDockerfileGenerator CreateDefaultDockerfileGenerator(
            IProgrammingPlatform platform,
            BuildScriptGeneratorOptions commonOptions)
        {
            return CreateDefaultDockerfileGenerator(new[] { platform }, commonOptions);
        }

        private DefaultDockerfileGenerator CreateDefaultDockerfileGenerator(
            IProgrammingPlatform[] platforms,
            BuildScriptGeneratorOptions commonOptions)
        {
            commonOptions = commonOptions ?? new BuildScriptGeneratorOptions();
            return new DefaultDockerfileGenerator(
                new DefaultCompatiblePlatformDetector(
                    platforms,
                    NullLogger<DefaultCompatiblePlatformDetector>.Instance,
                    Options.Create(commonOptions)),
                NullLogger<DefaultDockerfileGenerator>.Instance,
                Options.Create(commonOptions),
                new ApplicationInsights.TelemetryClient(new ApplicationInsights.Extensibility.TelemetryConfiguration()
                {
                    ConnectionString = "test"
                }));
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
