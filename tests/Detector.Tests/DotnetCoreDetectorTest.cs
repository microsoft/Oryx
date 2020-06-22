// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Oryx.Detector.DotNetCore;
using Moq;
using Xunit;

namespace Microsoft.Oryx.Detector.Tests.DotNetCore
{
    public class DotNetCoreDetectorTest
    {
        private const string ProjectFileWithNoTargetFramework = @"
        <Project Sdk=""Microsoft.NET.Sdk.Web"">
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
          </PropertyGroup>
        </Project>";

        private const string ProjectFileWithMultipleProperties = @"
        <Project Sdk=""Microsoft.NET.Sdk.Web"">
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
          </PropertyGroup>
          <PropertyGroup>
            <TargetFramework>netcoreapp2.1</TargetFramework>
            <LangVersion>7.3</LangVersion>
          </PropertyGroup>
        </Project>";

        private const string ProjectFileWithTargetFrameworkPlaceHolder = @"
        <Project Sdk=""Microsoft.NET.Sdk.Web"">
          <PropertyGroup>
            <TargetFramework>#TargetFramework#</TargetFramework>
            <LangVersion>7.3</LangVersion>
            <IsPackable>false</IsPackable>
            <AssemblyName>Microsoft.Oryx.BuildScriptGenerator.Tests</AssemblyName>
            <RootNamespace>Microsoft.Oryx.BuildScriptGenerator.Tests</RootNamespace>
          </PropertyGroup>
        </Project>";

        private const string GlobalJsonWithSdkVersionPlaceholder = @"
        {
            ""sdk"": {
                ""version"": ""#version#""
            }
        }";

        [Fact]
        public void Detect_ReturnsNull_IfRepoDoesNotContain_ProjectFile()
        {
            // Arrange
            var sourceRepo = new Mock<ISourceRepo>();
            var context = CreateContext(sourceRepo.Object);
            var detector = CreateDotNetCorePlatformDetector(
                projectFile: null);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsNull_IfProjectFile_DoesNotHaveTargetFrameworkSpecified()
        {
            // Arrange
            var projectFile = "test.csproj";
            var sourceRepo = new Mock<ISourceRepo>();
            sourceRepo
                .Setup(repo => repo.EnumerateFiles(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new[] { projectFile });
            sourceRepo
                .Setup(repo => repo.ReadFile(It.IsAny<string>()))
                .Returns(ProjectFileWithNoTargetFramework);
            var context = CreateContext(sourceRepo.Object);
            var detector = CreateDotNetCorePlatformDetector(
                projectFile);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
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
                .Returns(ProjectFileWithMultipleProperties);
            var context = CreateContext(sourceRepo.Object);
            var detector = CreateDotNetCorePlatformDetector(
                projectFile);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DotNetCoreConstants.PlatformName, result.Platform);
            Assert.Equal("2.1", result.PlatformVersion);
        }

        [Fact]
        public void Detect_DotNetCoreAppVersionFromTargetFramework()
        {
            // Arrange
            var projectFile = "test.csproj";
            var projectFileContent = ProjectFileWithTargetFrameworkPlaceHolder.Replace(
                "#TargetFramework#",
                "netcoreapp3.1");
            var sourceRepo = new Mock<ISourceRepo>();
            sourceRepo
                .Setup(repo => repo.EnumerateFiles(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new[] { projectFile });
            sourceRepo
                .Setup(repo => repo.ReadFile(It.IsAny<string>()))
                .Returns(projectFileContent);
            var context = CreateContext(sourceRepo.Object);
            var detector = CreateDotNetCorePlatformDetector(
                projectFile);

            var result = detector.Detect(context);

            Assert.NotNull(result);
            Assert.Equal(DotNetCoreConstants.PlatformName, result.Platform);
            Assert.Equal("3.1", result.PlatformVersion);
        }

        private DetectorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new DetectorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private DotNetCoreDetector CreateDotNetCorePlatformDetector(
            string projectFile)
        {
            return new DotNetCoreDetector(
                new TestProjectFileProvider(projectFile),
                NullLogger<DotNetCoreDetector>.Instance);
        }

        private class TestProjectFileProvider : DefaultProjectFileProvider
        {
            private readonly string _projectFilePath;

            public TestProjectFileProvider(string projectFilePath)
                : base(projectFileProviders: null)
            {
                _projectFilePath = projectFilePath;
            }

            public override string GetRelativePathToProjectFile(DetectorContext context)
            {
                return _projectFilePath;
            }
        }

    }
}
