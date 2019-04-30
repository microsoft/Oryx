// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Moq;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.DotNetCore
{
    public class DotNetCoreLanguageDetectorTest
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
            var detector = CreateDotnetCoreLanguageDetector(
                supportedVersions: new[]
                {
                    DotNetCoreVersions.DotNetCore11Version
                },
                projectFile: null);

            // Act
            var result = detector.Detect(sourceRepo.Object);

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
            var detector = CreateDotnetCoreLanguageDetector(
                supportedVersions: new[]
                {
                    DotNetCoreVersions.DotNetCore11Version
                },
                projectFile);

            // Act
            var result = detector.Detect(sourceRepo.Object);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("netcoreapp1.0", DotNetCoreVersions.DotNetCore11Version)]
        [InlineData("netcoreapp1.1", DotNetCoreVersions.DotNetCore11Version)]
        [InlineData("netcoreapp2.0", DotNetCoreVersions.DotNetCore21Version)]
        [InlineData("netcoreapp2.1", DotNetCoreVersions.DotNetCore21Version)]
        [InlineData("netcoreapp2.2", DotNetCoreVersions.DotNetCore22Version)]
        [InlineData("netcoreapp3.0", DotNetCoreVersions.DotNetCore30Version)]
        public void Detect_ReturnsExpectedLanguageVersion_ForTargetFrameworkVersions(
            string netCoreAppVersion,
            string expectedSdkVersion)
        {
            // Arrange
            var projectFile = "test.csproj";
            var projectFileContent = ProjectFileWithTargetFrameworkPlaceHolder.Replace(
                "#TargetFramework#",
                netCoreAppVersion);
            var sourceRepo = new Mock<ISourceRepo>();
            sourceRepo
                .Setup(repo => repo.EnumerateFiles(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new[] { projectFile });
            sourceRepo
                .Setup(repo => repo.ReadFile(It.IsAny<string>()))
                .Returns(projectFileContent);
            var detector = CreateDotnetCoreLanguageDetector(
                supportedVersions: new[]
                {
                    DotNetCoreVersions.DotNetCore11Version,
                    DotNetCoreVersions.DotNetCore21Version,
                    DotNetCoreVersions.DotNetCore22Version,
                    DotNetCoreVersions.DotNetCore30Version,
                },
                projectFile);

            // Act
            var result = detector.Detect(sourceRepo.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DotnetCoreConstants.LanguageName, result.Language);
            Assert.Equal(expectedSdkVersion, result.LanguageVersion);
        }

        [Fact]
        public void Detect_ReturnsExpectedLanguageVersion_WhenProjectFileHasMultiplePropertyGroups()
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
            var detector = CreateDotnetCoreLanguageDetector(
                supportedVersions: new[]
                {
                    DotNetCoreVersions.DotNetCore11Version,
                    DotNetCoreVersions.DotNetCore21Version,
                    DotNetCoreVersions.DotNetCore22Version,
                    DotNetCoreVersions.DotNetCore30Version,
                },
                projectFile);

            // Act
            var result = detector.Detect(sourceRepo.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DotnetCoreConstants.LanguageName, result.Language);
            Assert.Equal(DotNetCoreVersions.DotNetCore21Version, result.LanguageVersion);
        }

        [Fact]
        public void Detect_ReturnsNull_ForUnknownNetCoreAppVersion()
        {
            // Arrange
            var projectFile = "test.csproj";
            var projectFileContent = ProjectFileWithTargetFrameworkPlaceHolder.Replace(
                "#TargetFramework#",
                "netcoreapp0.0");
            var sourceRepo = new Mock<ISourceRepo>();
            sourceRepo
                .Setup(repo => repo.EnumerateFiles(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new[] { projectFile });
            sourceRepo
                .Setup(repo => repo.ReadFile(It.IsAny<string>()))
                .Returns(projectFileContent);
            var detector = CreateDotnetCoreLanguageDetector(
                supportedVersions: new[]
                {
                    DotNetCoreVersions.DotNetCore11Version,
                    DotNetCoreVersions.DotNetCore21Version,
                    DotNetCoreVersions.DotNetCore22Version,
                    DotNetCoreVersions.DotNetCore30Version,
                },
                projectFile);

            // Act
            var result = detector.Detect(sourceRepo.Object);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsSdkVersion_SpecifiedInGlobalJsonFile()
        {
            // Arrange
            var projectFile = "test.csproj";
            var globalJsonFileContent = GlobalJsonWithSdkVersionPlaceholder.Replace(
                "#version#",
                DotNetCoreVersions.DotNetCore21Version);
            var projectFileContent = ProjectFileWithTargetFrameworkPlaceHolder.Replace(
                "#TargetFramework#",
                "netcoreapp2.1");
            var sourceRepo = new Mock<ISourceRepo>();
            sourceRepo
                .Setup(repo => repo.EnumerateFiles(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new[] { projectFile, "global.json" });
            sourceRepo
                .Setup(repo => repo.FileExists("global.json"))
                .Returns(true);
            sourceRepo
                .Setup(repo => repo.ReadFile("test.csproj"))
                .Returns(projectFileContent);
            sourceRepo
                .Setup(repo => repo.ReadFile("global.json"))
                .Returns(globalJsonFileContent);
            var detector = CreateDotnetCoreLanguageDetector(
                supportedVersions: new[]
                {
                    DotNetCoreVersions.DotNetCore11Version,
                    DotNetCoreVersions.DotNetCore21Version,
                    DotNetCoreVersions.DotNetCore22Version,
                    DotNetCoreVersions.DotNetCore30Version,
                },
                projectFile);

            // Act
            var result = detector.Detect(sourceRepo.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DotnetCoreConstants.LanguageName, result.Language);
            Assert.Equal(DotNetCoreVersions.DotNetCore21Version, result.LanguageVersion);
        }

        [Fact]
        public void Detect_ReturnsSdkVersion_BasedOnTargetFramework_IfSpecifiedInGlobalJsonDoesNotHaveSdkVersion()
        {
            // Arrange
            var projectFile = "test.csproj";
            var projectFileContent = ProjectFileWithTargetFrameworkPlaceHolder.Replace(
                "#TargetFramework#",
                "netcoreapp2.1");
            var sourceRepo = new Mock<ISourceRepo>();
            sourceRepo
                .Setup(repo => repo.EnumerateFiles(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new[] { projectFile, "global.json" });
            sourceRepo
                .Setup(repo => repo.FileExists("global.json"))
                .Returns(true);
            sourceRepo
                .Setup(repo => repo.ReadFile("test.csproj"))
                .Returns(projectFileContent);
            sourceRepo
                .Setup(repo => repo.ReadFile("global.json"))
                .Returns("{}");
            var detector = CreateDotnetCoreLanguageDetector(
                supportedVersions: new[]
                {
                    DotNetCoreVersions.DotNetCore11Version,
                    DotNetCoreVersions.DotNetCore21Version,
                    DotNetCoreVersions.DotNetCore22Version,
                    DotNetCoreVersions.DotNetCore30Version
                },
                projectFile);

            // Act
            var result = detector.Detect(sourceRepo.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DotnetCoreConstants.LanguageName, result.Language);
            Assert.Equal(DotNetCoreVersions.DotNetCore21Version, result.LanguageVersion);
        }

        [Fact]
        public void Detect_ReturnsMaximumSatisfyingVersion()
        {
            // Arrange
            var projectFile = "test.csproj";
            var sourceRepo = new Mock<ISourceRepo>();
            sourceRepo
                .Setup(repo => repo.EnumerateFiles(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new[] { projectFile });
            sourceRepo
                .Setup(repo => repo.ReadFile(It.IsAny<string>()))
                .Returns(ProjectFileWithTargetFrameworkPlaceHolder.Replace("#TargetFramework#", "netcoreapp2.1"));
            var detector = CreateDotnetCoreLanguageDetector(
                supportedVersions: new[] { "2.1.1", "2.1.300" },
                projectFile);

            // Act
            var result = detector.Detect(sourceRepo.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DotnetCoreConstants.LanguageName, result.Language);
            Assert.Equal("2.1.300", result.LanguageVersion);
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
                .Returns(ProjectFileWithTargetFrameworkPlaceHolder.Replace("#TargetFramework#", "netcoreapp2.1"));
            var detector = CreateDotnetCoreLanguageDetector(
                supportedVersions: new[] { "2.2" },
                projectFile);

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => detector.Detect(sourceRepo.Object));
            Assert.Equal(
                "Target .NET Core version '2.1' is unsupported. Supported versions are: 2.2",
                exception.Message);
        }

        private DotnetCoreLanguageDetector CreateDotnetCoreLanguageDetector(
            string[] supportedVersions,
            string projectFile)
        {
            return CreateDotnetCoreLanguageDetector(
                supportedVersions,
                projectFile,
                new TestEnvironment());
        }

        private DotnetCoreLanguageDetector CreateDotnetCoreLanguageDetector(
            string[] supportedVersions,
            string projectFile,
            IEnvironment environment)
        {
            var optionsSetup = new DotnetCoreScriptGeneratorOptionsSetup(environment);
            var options = new DotnetCoreScriptGeneratorOptions();
            optionsSetup.Configure(options);

            return new DotnetCoreLanguageDetector(
                new TestVersionProvider(supportedVersions),
                Options.Create(options),
                new TestAspNetCoreWebAppProjectFileProvider(projectFile),
                NullLogger<DotnetCoreLanguageDetector>.Instance);
        }

        private class TestAspNetCoreWebAppProjectFileProvider : IAspNetCoreWebAppProjectFileProvider
        {
            private readonly string _projectFilePath;

            public TestAspNetCoreWebAppProjectFileProvider(string projectFilePath)
            {
                _projectFilePath = projectFilePath;
            }

            public string GetProjectFile(ISourceRepo sourceRepo)
            {
                return _projectFilePath;
            }
        }
    }
}
