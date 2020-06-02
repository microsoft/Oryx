// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Moq;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.DotNetCore
{
    public class DotNetCorePlatformTest
    {
        [Fact]
        public void Detect_ThrowsUnsupportedException_ForUnknownNetCoreAppVersion()
        {
            // Arrange
            var projectFile = "test.csproj";
            var projectFileContent = SampleProjectFileContents.ProjectFileWithTargetFrameworkPlaceHolder.Replace(
                "#TargetFramework#",
                "netcoreapp0.0");
            var sourceRepo = new Mock<ISourceRepo>();
            sourceRepo
                .Setup(repo => repo.EnumerateFiles(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new[] { projectFile });
            sourceRepo
                .Setup(repo => repo.ReadFile(It.IsAny<string>()))
                .Returns(projectFileContent);
            var context = CreateContext(sourceRepo.Object);
            var platform = CreatePlatform(projectFile: projectFile, defaultVersion: "2.2.7");

            var exception = Assert.Throws<UnsupportedVersionException>(
                () => platform.Detect(context));
            Assert.Equal(
                $"Platform 'dotnet' version '0.0' is unsupported. " +
                "Supported versions: 2.2.7",
                exception.Message);
        }

        [Fact]
        public void Detect_ThrowsUnsupportedException_WhenNoVersionFoundReturnsMaximumSatisfyingVersion()
        {
            // Arrange
            var projectFile = "test.csproj";
            var sourceRepo = new Mock<ISourceRepo>();
            sourceRepo
                .Setup(repo => repo.EnumerateFiles(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new[] { projectFile });
            sourceRepo
                .Setup(repo => repo.ReadFile(It.IsAny<string>()))
                .Returns(SampleProjectFileContents.ProjectFileWithTargetFrameworkPlaceHolder.Replace(
                    "#TargetFramework#",
                    "netcoreapp2.1"));
            var context = CreateContext(sourceRepo.Object);
            var platform = CreatePlatform(projectFile, defaultVersion: "2.2.7");

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(
                () => platform.Detect(context));
            Assert.Equal(
                $"Platform 'dotnet' version '2.1' is unsupported. " +
                "Supported versions: 2.2.7",
                exception.Message);
        }

        [Theory]
        [InlineData("netcoreapp1.0", "1.0.14")]
        [InlineData("netcoreapp1.1", "1.1.15")]
        [InlineData("netcoreapp2.0", "2.0.9")]
        [InlineData("netcoreapp2.1", "2.1.15")]
        [InlineData("netcoreapp2.2", "2.2.8")]
        [InlineData("netcoreapp3.0", "3.0.2")]
        [InlineData("netcoreapp3.1", "3.1.2")]
        [InlineData("netcoreapp5.0", "5.0.0-rc.1.14955.1")]
        public void Detect_ReturnsExpectedMaximumSatisfyingPlatformVersion_ForTargetFrameworkVersions(
            string netCoreAppVersion,
            string expectedSdkVersion)
        {
            // Arrange
            var projectFile = "test.csproj";
            var projectFileContent = SampleProjectFileContents.ProjectFileWithTargetFrameworkPlaceHolder.Replace(
                "#TargetFramework#",
                netCoreAppVersion);
            var sourceRepo = new Mock<ISourceRepo>();
            sourceRepo
                .Setup(repo => repo.EnumerateFiles(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new[] { projectFile });
            sourceRepo
                .Setup(repo => repo.ReadFile(It.IsAny<string>()))
                .Returns(projectFileContent);
            var context = CreateContext(sourceRepo.Object);
            var platform = CreatePlatform(
                projectFile: projectFile,
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

        [Fact]
        public void Detect_ReturnsExpectedPlatformVersion_WhenProjectFileHasMultiplePropertyGroups()
        {
            // Arrange
            var projectFile = "test.csproj";
            var sourceRepo = new Mock<ISourceRepo>();
            sourceRepo
                .Setup(repo => repo.EnumerateFiles(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new[] { projectFile });
            sourceRepo
                .Setup(repo => repo.ReadFile(It.IsAny<string>()))
                .Returns(SampleProjectFileContents.ProjectFileWithMultipleProperties);
            var context = CreateContext(sourceRepo.Object);
            var platform = CreatePlatform(
                projectFile: projectFile,
                supportedVersions: new Dictionary<string, string> { { "2.1.14", "2.1.803" } });

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DotNetCoreConstants.PlatformName, result.Platform);
            Assert.Equal("2.1.14", result.PlatformVersion);
        }

        private BuildScriptGeneratorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private DotNetCorePlatform CreatePlatform(
            string projectFile = null,
            Dictionary<string, string> supportedVersions = null,
            string defaultVersion = null)
        {
            var projectFileProvider = new TestProjectFileProvider(projectFile);
            var detector = new DotNetCorePlatformDetector(
                projectFileProvider,
                NullLogger<DotNetCorePlatformDetector>.Instance);
            defaultVersion = defaultVersion ?? DotNetCoreRunTimeVersions.NetCoreApp31;
            supportedVersions = supportedVersions ?? new Dictionary<string, string>
            {
                { defaultVersion, defaultVersion },
            };
            var versionProvider = new TestDotNetCoreVersionProvider(
                supportedVersions,
                defaultVersion);
            var commonOptions = new BuildScriptGeneratorOptions();
            var dotNetCoreScriptGeneratorOptions = new DotNetCoreScriptGeneratorOptions();
            var installer = new DotNetCorePlatformInstaller(
                Options.Create(commonOptions),
                versionProvider,
                NullLoggerFactory.Instance);
            var globalJsonSdkResolver = new GlobalJsonSdkResolver(NullLogger<GlobalJsonSdkResolver>.Instance);
            return new TestDotNetCorePlatform(
                versionProvider,
                projectFileProvider,
                detector,
                Options.Create(commonOptions),
                Options.Create(dotNetCoreScriptGeneratorOptions),
                installer,
                globalJsonSdkResolver);
        }

        private class TestDotNetCorePlatform : DotNetCorePlatform
        {
            public TestDotNetCorePlatform(
                IDotNetCoreVersionProvider versionProvider,
                DefaultProjectFileProvider projectFileProvider,
                DotNetCorePlatformDetector detector,
                IOptions<BuildScriptGeneratorOptions> cliOptions,
                IOptions<DotNetCoreScriptGeneratorOptions> dotNetCoreScriptGeneratorOptions,
                DotNetCorePlatformInstaller platformInstaller,
                GlobalJsonSdkResolver globalJsonSdkResolver)
                : base(
                      versionProvider,
                      projectFileProvider,
                      NullLogger<DotNetCorePlatform>.Instance,
                      detector,
                      cliOptions,
                      dotNetCoreScriptGeneratorOptions,
                      platformInstaller,
                      globalJsonSdkResolver)
            {
            }
        }
    }
}
