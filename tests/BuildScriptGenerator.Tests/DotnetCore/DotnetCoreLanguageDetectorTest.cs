// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
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
                supportedVersions: new Dictionary<string, string>
                {
                    {"2.1.14", "2.1.100" },
                    {"2.2.7", "2.2.100" },
                },
                defaultVersion: "2.2",
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
                supportedVersions: new Dictionary<string, string>
                {
                    {"2.1.14", "2.1.100" },
                    {"2.2.7", "2.2.100" },
                },
                defaultVersion: "2.2",
                projectFile);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("netcoreapp1.0", "1.0.14")]
        [InlineData("netcoreapp1.1", "1.1.15")]
        [InlineData("netcoreapp2.0", "2.0.9")]
        [InlineData("netcoreapp2.1", "2.1.15")]
        [InlineData("netcoreapp2.2", "2.2.8")]
        [InlineData("netcoreapp3.0", "3.0.2")]
        [InlineData("netcoreapp3.1", "3.1.2")]
        public void Detect_ReturnsExpectedMaximumSatisfyingLanguageVersion_ForTargetFrameworkVersions(
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
                supportedVersions: new Dictionary<string, string>
                {
                    {"1.0.14", "1.1.100" },
                    {"1.1.15", "1.1.100" },
                    {"2.0.9", "2.1.100" },
                    {"2.1.14", "2.1.100" },
                    {"2.1.15", "2.1.101" },
                    {"2.2.7", "2.2.100" },
                    {"2.2.8", "2.2.101" },
                    {"3.0.1", "3.0.100" },
                    {"3.0.2", "3.0.101" },
                    {"3.1.1", "3.1.100" },
                    {"3.1.2", "3.1.101" },
                },
                defaultVersion: "3.1",
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
                supportedVersions: new Dictionary<string, string>
                {
                    {"2.1.14", "2.1.100" },
                    {"2.2.7", "2.2.100" },
                },
                defaultVersion: "2.2",
                projectFile);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DotNetCoreConstants.LanguageName, result.Language);
            Assert.Equal("2.1.14", result.LanguageVersion);
        }

        [Fact]
        public void Detect_ThrowsUnsupportedException_ForUnknownNetCoreAppVersion()
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
                supportedVersions: new Dictionary<string, string>
                {
                    {"2.2.7", "2.2.100" },
                },
                defaultVersion: "2.2",
                projectFile);

            var exception = Assert.Throws<UnsupportedVersionException>(
                () => detector.Detect(context));
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
                .Returns(ProjectFileWithTargetFrameworkPlaceHolder.Replace("#TargetFramework#", "netcoreapp2.1"));
            var context = CreateContext(sourceRepo.Object);
            var detector = CreateDotNetCoreLanguageDetector(
                supportedVersions: new Dictionary<string, string>
                {
                    {"2.2.7", "2.2.100" },
                },
                defaultVersion: "2.2",
                projectFile);

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(
                () => detector.Detect(context));
            Assert.Equal(
                $"Platform 'dotnet' version '2.1' is unsupported. " +
                "Supported versions: 2.2.7",
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
            Dictionary<string, string> supportedVersions,
            string defaultVersion,
            string projectFile)
        {
            return CreateDotNetCoreLanguageDetector(
                supportedVersions,
                defaultVersion,
                projectFile,
                new DotNetCoreScriptGeneratorOptions());
        }

        private DotNetCoreLanguageDetector CreateDotNetCoreLanguageDetector(
            Dictionary<string, string> supportedVersions,
            string defaultVersion,
            string projectFile,
            DotNetCoreScriptGeneratorOptions options)
        {
            options = options ?? new DotNetCoreScriptGeneratorOptions();
            
            return new DotNetCoreLanguageDetector(
                new TestDotNetCoreVersionProvider(supportedVersions, defaultVersion),
                Options.Create(options),
                new TestProjectFileProvider(projectFile),
                NullLogger<DotNetCoreLanguageDetector>.Instance,
                new DefaultStandardOutputWriter());
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

        private class TestDotNetCoreVersionProvider : IDotNetCoreVersionProvider
        {
            private readonly Dictionary<string, string> _supportedVersions;
            private readonly string _defaultVersion;

            public TestDotNetCoreVersionProvider(Dictionary<string, string> supportedVersions, string defaultVersion)
            {
                _supportedVersions = supportedVersions;
                _defaultVersion = defaultVersion;
            }

            public string GetDefaultRuntimeVersion()
            {
                return _defaultVersion;
            }

            public Dictionary<string, string> GetSupportedVersions()
            {
                return _supportedVersions;
            }
        }
    }
}
