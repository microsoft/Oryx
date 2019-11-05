// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Tests.Common;
using Moq;
using Xunit;

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
            var context = CreateContext(sourceRepo.Object);
            var detector = CreateDotNetCoreLanguageDetector(
                supportedVersions: GetAllSupportedRuntimeVersions(),
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
            var detector = CreateDotNetCoreLanguageDetector(
                supportedVersions: GetAllSupportedRuntimeVersions(),
                projectFile);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("netcoreapp1.0", DotNetCoreRunTimeVersions.NetCoreApp10)]
        [InlineData("netcoreapp1.1", DotNetCoreRunTimeVersions.NetCoreApp11)]
        [InlineData("netcoreapp2.0", DotNetCoreRunTimeVersions.NetCoreApp20)]
        [InlineData("netcoreapp2.1", DotNetCoreRunTimeVersions.NetCoreApp21)]
        [InlineData("netcoreapp2.2", DotNetCoreRunTimeVersions.NetCoreApp22)]
        [InlineData("netcoreapp3.0", DotNetCoreRunTimeVersions.NetCoreApp30)]
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
            var context = CreateContext(sourceRepo.Object);
            var detector = CreateDotNetCoreLanguageDetector(
                supportedVersions: GetAllSupportedRuntimeVersions(),
                projectFile);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DotNetCoreConstants.LanguageName, result.Language);
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
            var context = CreateContext(sourceRepo.Object);
            var detector = CreateDotNetCoreLanguageDetector(
                supportedVersions: GetAllSupportedRuntimeVersions(),
                projectFile);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DotNetCoreConstants.LanguageName, result.Language);
            Assert.Equal(DotNetCoreRunTimeVersions.NetCoreApp21, result.LanguageVersion);
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
            var context = CreateContext(sourceRepo.Object);
            var detector = CreateDotNetCoreLanguageDetector(
                supportedVersions: GetAllSupportedRuntimeVersions(),
                projectFile);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
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
            var context = CreateContext(sourceRepo.Object);
            var detector = CreateDotNetCoreLanguageDetector(
                supportedVersions: new[] { "2.2" },
                projectFile);

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(
                () => detector.Detect(context));
            Assert.Equal(
                $"Platform 'dotnet' version '{DotNetCoreRunTimeVersions.NetCoreApp21}' is unsupported. " +
                "Supported versions: 2.2",
                exception.Message);
        }

        private BuildScriptGeneratorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private DotNetCoreLanguageDetector CreateDotNetCoreLanguageDetector(
            string[] supportedVersions,
            string projectFile)
        {
            return CreateDotNetCoreLanguageDetector(
                supportedVersions,
                projectFile,
                new TestEnvironment());
        }

        private DotNetCoreLanguageDetector CreateDotNetCoreLanguageDetector(
            string[] supportedVersions,
            string projectFile,
            IEnvironment environment)
        {
            var optionsSetup = new DotNetCoreScriptGeneratorOptionsSetup(environment);
            var options = new DotNetCoreScriptGeneratorOptions();
            optionsSetup.Configure(options);

            return new DotNetCoreLanguageDetector(
                new TestVersionProvider(supportedVersions),
                Options.Create(options),
                new TestProjectFileProvider(projectFile),
                NullLogger<DotNetCoreLanguageDetector>.Instance,
                new DefaultStandardOutputWriter());
        }

        private string[] GetAllSupportedRuntimeVersions()
        {
            return new[]
            {
                DotNetCoreRunTimeVersions.NetCoreApp10,
                DotNetCoreRunTimeVersions.NetCoreApp11,
                DotNetCoreRunTimeVersions.NetCoreApp20,
                DotNetCoreRunTimeVersions.NetCoreApp21,
                DotNetCoreRunTimeVersions.NetCoreApp22,
                DotNetCoreRunTimeVersions.NetCoreApp30,
            };
        }

        private class TestProjectFileProvider : DefaultProjectFileProvider
        {
            private readonly string _projectFilePath;

            public TestProjectFileProvider(string projectFilePath)
                : base(projectFileProviders: null)
            {
                _projectFilePath = projectFilePath;
            }

            public override string GetRelativePathToProjectFile(RepositoryContext context)
            {
                return _projectFilePath;
            }
        }
    }
}
