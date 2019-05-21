// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Xml.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.DotNetCore
{
    public class DefaultAspNetCoreWebAppProjectFileProviderTest : IClassFixture<TestTempDirTestFixture>
    {
        private const string NonWebSdkProjectFile = @"
        <Project Sdk=""Microsoft.NET.Sdk.Razor"">
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""xunit"" Version=""2.3.1"" />
          </ItemGroup>
        </Project>";

        private const string WebSdkProjectFile = @"
        <Project Sdk=""Microsoft.NET.Sdk.Web"">
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.AspNetCore"" Version=""2.1.0"" />
          </ItemGroup>
        </Project>";

        private const string WebSdkProjectFileWithVersion = @"
        <Project Sdk=""Microsoft.NET.Sdk.Web/1.0.0"">
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.AspNetCore"" Version=""2.1.0"" />
          </ItemGroup>
        </Project>";

        private const string NonWebSdkProjectFileWithVersion = @"
        <Project Sdk=""Microsoft.NET.Sdk/1.0.0"">
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.AspNetCore"" Version=""2.1.0"" />
          </ItemGroup>
        </Project>";

        private const string WebSdkProjectFileWithSdkInfoAsElement = @"
        <Project>
          <Sdk Name=""Microsoft.NET.Sdk.Web"" Version=""1.0.0"" />
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.AspNetCore"" Version=""2.1.0"" />
          </ItemGroup>
        </Project>";

        private const string NonWebSdkProjectFileWithSdkInfoAsElement = @"
        <Project>
          <Sdk Name=""Microsoft.NET.Sdk"" Version=""1.0.0"" />
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.AspNetCore"" Version=""2.1.0"" />
          </ItemGroup>
        </Project>";

        private const string NoSdkInformationProjectFile = @"
        <Project>
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.AspNetCore"" Version=""2.1.0"" />
          </ItemGroup>
        </Project>";

        private const string WebSdkProjectFileWithMultipleSdkInfoAsElement = @"
        <Project Sdk=""Microsoft.NET.Sdk/1.0.0"">
          <Sdk Name=""Microsoft.NET.Sdk.Web"" Version=""1.0.0"" />
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.AspNetCore"" Version=""2.1.0"" />
          </ItemGroup>
        </Project>";

        private readonly string _tempDirRoot;

        public DefaultAspNetCoreWebAppProjectFileProviderTest(TestTempDirTestFixture testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }

        [Fact]
        public void IsAspNetCoreWebApplicationProject_ReturnsFalse_WhenProject_IsNotWebSdkProject()
        {
            // Arrange
            var xdoc = XDocument.Load(new StringReader(NonWebSdkProjectFile));

            // Act
            var actual = DefaultAspNetCoreWebAppProjectFileProvider.IsAspNetCoreWebApplicationProject(xdoc);

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void IsAspNetCoreWebApplicationProject_ReturnsTrue_WhenProject_IsWebSdkProject()
        {
            // Arrange
            var xdoc = XDocument.Load(new StringReader(WebSdkProjectFile));

            // Act
            var actual = DefaultAspNetCoreWebAppProjectFileProvider.IsAspNetCoreWebApplicationProject(xdoc);

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void GetProjectFile_ReturnsNull_IfNoProjectFileFound()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var provider = CreateProjectFileProvider();

            // Act
            var actual = provider.GetProjectFile(sourceRepo);

            // Assert
            Assert.Null(actual);
        }

        [Theory]
        [InlineData(DotnetCoreConstants.CSharpProjectFileExtension)]
        [InlineData(DotnetCoreConstants.FSharpProjectFileExtension)]
        public void GetProjectFile_ReturnsProjectFile_PresentAtRoot_IfPresent(string projectFileExtension)
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedFile = Path.Combine(sourceRepoDir, $"WebApp1.{projectFileExtension}");
            File.WriteAllText(expectedFile, WebSdkProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var provider = CreateProjectFileProvider();

            // Act
            var actual = provider.GetProjectFile(sourceRepo);

            // Assert
            Assert.Equal(expectedFile, actual);
        }

        [Fact]
        public void GetProjectFile_ReturnsProjectFile_IfSdkHasBothNameAndVersion()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedFile = Path.Combine(sourceRepoDir, "WebApp1.csproj");
            File.WriteAllText(expectedFile, WebSdkProjectFileWithVersion);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var provider = CreateProjectFileProvider();

            // Act
            var actual = provider.GetProjectFile(sourceRepo);

            // Assert
            Assert.Equal(expectedFile, actual);
        }

        [Fact]
        public void GetProjectFile_ReturnsNull_IfSdkHasBothNameAndVersion_AndIsNotWebSdk()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedFile = Path.Combine(sourceRepoDir, "WebApp1.csproj");
            File.WriteAllText(expectedFile, NonWebSdkProjectFileWithVersion);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var provider = CreateProjectFileProvider();

            // Act
            var actual = provider.GetProjectFile(sourceRepo);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetProjectFile_ReturnsProjectFile_IfSdkIsPresentAsElement()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedFile = Path.Combine(sourceRepoDir, "WebApp1.csproj");
            File.WriteAllText(expectedFile, WebSdkProjectFileWithSdkInfoAsElement);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var provider = CreateProjectFileProvider();

            // Act
            var actual = provider.GetProjectFile(sourceRepo);

            // Assert
            Assert.Equal(expectedFile, actual);
        }

        [Fact]
        public void GetProjectFile_ReturnsNull_IfSdkIsPresentAsElement_AndIsNotWebSdk()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedFile = Path.Combine(sourceRepoDir, "WebApp1.csproj");
            File.WriteAllText(expectedFile, NonWebSdkProjectFileWithSdkInfoAsElement);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var provider = CreateProjectFileProvider();

            // Act
            var actual = provider.GetProjectFile(sourceRepo);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetProjectFile_ReturnsNull_WhenNoInformationAboutSdkIsPresentInProjectFile()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedFile = Path.Combine(sourceRepoDir, "WebApp1.csproj");
            File.WriteAllText(expectedFile, NoSdkInformationProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var provider = CreateProjectFileProvider();

            // Act
            var actual = provider.GetProjectFile(sourceRepo);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetProjectFile_ReturnsProjectFile_IfMultiplePlacesHaveSdkInfo()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedFile = Path.Combine(sourceRepoDir, "WebApp1.csproj");
            File.WriteAllText(expectedFile, WebSdkProjectFileWithMultipleSdkInfoAsElement);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var provider = CreateProjectFileProvider();

            // Act
            var actual = provider.GetProjectFile(sourceRepo);

            // Assert
            Assert.Equal(expectedFile, actual);
        }

        [Theory]
        [InlineData(DotnetCoreConstants.CSharpProjectFileExtension)]
        [InlineData(DotnetCoreConstants.FSharpProjectFileExtension)]
        public void GetProjectFile_ReturnsNull_IfRootProject_IsNotWebSdkProject(string projectFileExtension)
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedFile = Path.Combine(sourceRepoDir, $"WebApp1.{projectFileExtension}");
            File.WriteAllText(expectedFile, NonWebSdkProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var provider = CreateProjectFileProvider();

            // Act
            var actual = provider.GetProjectFile(sourceRepo);

            // Assert
            Assert.Null(actual);
        }

        // This is a scenario where our probing of a project could be incorrect. In this case a user can explicitly
        // specify the project file to use and we use it without checking further.
        [Theory]
        [InlineData(DotnetCoreConstants.CSharpProjectFileExtension)]
        [InlineData(DotnetCoreConstants.FSharpProjectFileExtension)]
        public void GetProjectFile_ReturnsFile_IfProjectEnvVariableIsSet_AndProjectFileIsNotAspNetCoreApp(
            string projectFileExtension)
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            var expectedFile = Path.Combine(webApp1Dir, $"WebApp1.{projectFileExtension}");
            File.WriteAllText(expectedFile, NonWebSdkProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var relativeProjectPath = Path.Combine("src", "WebApp1", $"WebApp1.{projectFileExtension}");
            var options = new DotnetCoreScriptGeneratorOptions();
            options.Project = relativeProjectPath;
            var provider = CreateProjectFileProvider(options);

            // Act
            var actualFile = provider.GetProjectFile(sourceRepo);

            // Assert
            Assert.Equal(expectedFile, actualFile);
        }

        [Fact]
        public void GetProjectFile_Throws_IfPathInProjectEnvVariableValue_DoesNotExist()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(Path.Combine(webApp1Dir, "WebApp1.csproj"), WebSdkProjectFile);
            var webApp2Dir = CreateDir(srcDir, "WebApp2");
            File.WriteAllText(Path.Combine(webApp2Dir, "WebApp2.csproj"), WebSdkProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var relativeProjectPath = Path.Combine("src", "WebApp2", "WebApp2-doesnotexist.csproj");
            var options = new DotnetCoreScriptGeneratorOptions();
            options.Project = relativeProjectPath;
            var provider = CreateProjectFileProvider(options);

            // Act & Assert
            var exception = Assert.Throws<InvalidUsageException>(() => provider.GetProjectFile(sourceRepo));
            Assert.Contains("Could not find the project file ", exception.Message);
        }

        [Theory]
        [InlineData(DotnetCoreConstants.CSharpProjectFileExtension)]
        [InlineData(DotnetCoreConstants.FSharpProjectFileExtension)]
        public void GetProjectFile_ReturnsNull_IfNoWebSdkProjectFound_AllAcrossRepo(string projectFileExtension)
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(Path.Combine(webApp1Dir, $"WebApp1.{projectFileExtension}"), NonWebSdkProjectFile);
            var webApp2Dir = CreateDir(srcDir, "WebApp2");
            File.WriteAllText(Path.Combine(webApp2Dir, $"WebApp2.{projectFileExtension}"), NonWebSdkProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var provider = CreateProjectFileProvider();

            // Act
            var actual = provider.GetProjectFile(sourceRepo);

            // Assert
            Assert.Null(actual);
        }

        [Theory]
        [InlineData(DotnetCoreConstants.CSharpProjectFileExtension)]
        [InlineData(DotnetCoreConstants.FSharpProjectFileExtension)]
        public void GetProjectFile_Throws_IfSourceRepo_HasMultipleWebSdkProjects(string projectFileExtension)
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(Path.Combine(webApp1Dir, $"WebApp1.{projectFileExtension}"), WebSdkProjectFile);
            var webApp2Dir = CreateDir(srcDir, "WebApp2");
            File.WriteAllText(Path.Combine(webApp2Dir, $"WebApp2.{projectFileExtension}"), WebSdkProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var provider = CreateProjectFileProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidUsageException>(() => provider.GetProjectFile(sourceRepo));
            Assert.StartsWith(
                "Ambiguity in selecting an ASP.NET Core web application to build. Found multiple applications:",
                exception.Message);
        }

        [Theory]
        [InlineData(DotnetCoreConstants.CSharpProjectFileExtension)]
        [InlineData(DotnetCoreConstants.FSharpProjectFileExtension)]
        public void GetProjectFile_Throws_IfSourceRepo_HasMultipleWebAppProjects_AcrossDifferentFolderLevels(
            string projectFileExtension)
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(Path.Combine(webApp1Dir, $"WebApp1.{projectFileExtension}"), WebSdkProjectFile);
            var fooDir = CreateDir(srcDir, "foo");
            var webApp2Dir = CreateDir(fooDir, "WebApp2");
            File.WriteAllText(Path.Combine(webApp2Dir, $"WebApp2.{projectFileExtension}"), WebSdkProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var provider = CreateProjectFileProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidUsageException>(() => provider.GetProjectFile(sourceRepo));
            Assert.StartsWith(
                "Ambiguity in selecting an ASP.NET Core web application to build. Found multiple applications:",
                exception.Message);
        }

        [Theory]
        [InlineData(DotnetCoreConstants.CSharpProjectFileExtension)]
        [InlineData(DotnetCoreConstants.FSharpProjectFileExtension)]
        public void GetProjectFile_DoesNotThrow_IfSourceRepo_HasMultipleWebAppProjects_AndProjectEnvVariableValue(
            string projectFileExtension)
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(Path.Combine(webApp1Dir, $"WebApp1.{projectFileExtension}"), webApp1Dir);
            var webApp2Dir = CreateDir(srcDir, "WebApp2");
            var expectedFile = Path.Combine(webApp2Dir, $"WebApp2.{projectFileExtension}");
            File.WriteAllText(expectedFile, webApp1Dir);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var relativeProjectPath = Path.Combine("src", "WebApp2", $"WebApp2.{projectFileExtension}");
            var options = new DotnetCoreScriptGeneratorOptions();
            options.Project = relativeProjectPath;
            var provider = CreateProjectFileProvider(options);

            // Act
            var actualFile = provider.GetProjectFile(sourceRepo);

            // Assert
            Assert.Equal(expectedFile, actualFile);
        }

        [Theory]
        [InlineData(DotnetCoreConstants.CSharpProjectFileExtension)]
        [InlineData(DotnetCoreConstants.FSharpProjectFileExtension)]
        public void GetProjectFile_ReturnsProjectFile_ByProbingAllAcrossRepo(string projectFileExtension)
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(Path.Combine(webApp1Dir, $"WebApp1.{projectFileExtension}"), NonWebSdkProjectFile);
            var webApp2Dir = CreateDir(srcDir, "WebApp2");
            var expectedFile = Path.Combine(webApp2Dir, $"WebApp2.{projectFileExtension}");
            File.WriteAllText(expectedFile, WebSdkProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var provider = CreateProjectFileProvider();

            // Act
            var actualFile = provider.GetProjectFile(sourceRepo);

            // Assert
            Assert.Equal(expectedFile, actualFile);
        }

        private string CreateSourceRepoDir()
        {
            return Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString("N"))).FullName;
        }

        private string CreateDir(string parentDir, string newDirName)
        {
            return Directory.CreateDirectory(Path.Combine(parentDir, newDirName)).FullName;
        }

        private DefaultAspNetCoreWebAppProjectFileProvider CreateProjectFileProvider()
        {
            return CreateProjectFileProvider(new DotnetCoreScriptGeneratorOptions());
        }

        private DefaultAspNetCoreWebAppProjectFileProvider CreateProjectFileProvider(
            DotnetCoreScriptGeneratorOptions options)
        {
            return new DefaultAspNetCoreWebAppProjectFileProvider(
                Options.Create(options),
                NullLogger<DefaultAspNetCoreWebAppProjectFileProvider>.Instance);
        }

        private LocalSourceRepo CreateSourceRepo(string sourceDir)
        {
            return new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
        }
    }
}
