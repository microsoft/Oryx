// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Xml.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotnetCore;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.DotNetCore
{
    public class DefaultAspNetCoreWebAppProjectFileProviderTest : IClassFixture<TestTempDirTestFixture>
    {
        private const string ProjectFileWithNoMicrosoftAspNetCorePackageReferences = @"
        <Project Sdk=""Microsoft.NET.Sdk.Web"">
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""xunit"" Version=""2.3.1"" />
          </ItemGroup>
        </Project>";

        private const string ProjectFileWithMicrosoftAspNetCorePackageReference = @"
        <Project Sdk=""Microsoft.NET.Sdk.Web"">
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.AspNetCore"" Version=""2.1.0"" />
          </ItemGroup>
        </Project>";

        private const string ProjectFileWithMicrosoftAspNetCoreAllPackageReference = @"
        <Project Sdk=""Microsoft.NET.Sdk.Web"">
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.AspNetCore.All"" Version=""2.1.0"" />
          </ItemGroup>
        </Project>";

        private const string ProjectFileWithMicrosoftAspNetCoreAppPackageReference = @"
        <Project Sdk=""Microsoft.NET.Sdk.Web"">
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.AspNetCore.App"" Version=""2.1.0"" />
          </ItemGroup>
        </Project>";

        private const string ProjectFileWithMultipleItemGroups = @"
        <Project Sdk=""Microsoft.NET.Sdk.Web"">
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""xunit"" Version=""2.3.1"" />
          </ItemGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.AspNetCore.App"" Version=""2.1.0"" />
          </ItemGroup>
        </Project>";

        private readonly string _tempDirRoot;

        public DefaultAspNetCoreWebAppProjectFileProviderTest(TestTempDirTestFixture testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }

        [Fact]
        public void IsAspNetCoreWebApplicationProject_ReturnsFalse_WhenNoAspNetCoreRelatedPackagesArePresent()
        {
            // Arrange
            var xdoc = XDocument.Load(new StringReader(ProjectFileWithNoMicrosoftAspNetCorePackageReferences));

            // Act
            var actual = DefaultAspNetCoreWebAppProjectFileProvider.IsAspNetCoreWebApplicationProject(xdoc);

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void IsAspNetCoreWebApplicationProject_ReturnsTrue_ForAspNetCorePackageReference()
        {
            // Arrange
            var xdoc = XDocument.Load(new StringReader(ProjectFileWithMicrosoftAspNetCorePackageReference));

            // Act
            var actual = DefaultAspNetCoreWebAppProjectFileProvider.IsAspNetCoreWebApplicationProject(xdoc);

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void IsAspNetCoreWebApplicationProject_ReturnsTrue_ForAspNetCoreAllPackageReference()
        {
            // Arrange
            var xdoc = XDocument.Load(new StringReader(ProjectFileWithMicrosoftAspNetCoreAllPackageReference));

            // Act
            var actual = DefaultAspNetCoreWebAppProjectFileProvider.IsAspNetCoreWebApplicationProject(xdoc);

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void IsAspNetCoreWebApplicationProject_ReturnsTrue_ForAspNetCoreAppPackageReference()
        {
            // Arrange
            var xdoc = XDocument.Load(new StringReader(ProjectFileWithMicrosoftAspNetCoreAppPackageReference));

            // Act
            var actual = DefaultAspNetCoreWebAppProjectFileProvider.IsAspNetCoreWebApplicationProject(xdoc);

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void IsAspNetCoreWebApplicationProject_ReturnsTrue_ForProjectFileWithMultipleItemGroups()
        {
            // Arrange
            var xdoc = XDocument.Load(new StringReader(ProjectFileWithMultipleItemGroups));

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

        [Fact]
        public void GetProjectFile_ReturnsNull_IfNoAspNetCoreWebApplicationReferencesFound_ForRootProject()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedFile = Path.Combine(sourceRepoDir, "test.csproj");
            File.WriteAllText(
                expectedFile,
                ProjectFileWithNoMicrosoftAspNetCorePackageReferences);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var provider = CreateProjectFileProvider();

            // Act
            var actual = provider.GetProjectFile(sourceRepo);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetProjectFile_ReturnsNull_IfPathInProjectEnvVariableValue_DoesNotExist()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(
                Path.Combine(webApp1Dir, "WebApp1.csproj"),
                ProjectFileWithMicrosoftAspNetCoreAppPackageReference);
            var webApp2Dir = CreateDir(srcDir, "WebApp2");
            File.WriteAllText(
                Path.Combine(webApp2Dir, "WebApp2.csproj"),
                ProjectFileWithMicrosoftAspNetCoreAppPackageReference);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var relativeProjectPath = Path.Combine("src", "WebApp2", "WebApp2-doesnotexist.csproj");
            var options = new DotnetCoreScriptGeneratorOptions();
            options.Project = relativeProjectPath;
            var provider = CreateProjectFileProvider(options);

            // Act
            var actualFile = provider.GetProjectFile(sourceRepo);

            // Assert
            Assert.Null(actualFile);
        }

        [Fact]
        public void GetProjectFile_ReturnsNull_IfNoAspNetCoreWebApplicationReferencesFound_AllAcrossRepo()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(
                Path.Combine(webApp1Dir, "WebApp1.csproj"),
                ProjectFileWithNoMicrosoftAspNetCorePackageReferences);
            var webApp2Dir = CreateDir(srcDir, "WebApp2");
            File.WriteAllText(
                Path.Combine(webApp2Dir, "WebApp2.csproj"),
                ProjectFileWithNoMicrosoftAspNetCorePackageReferences);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var provider = CreateProjectFileProvider();

            // Act
            var actual = provider.GetProjectFile(sourceRepo);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetProjectFile_ReturnsProjectFile_PresentAtRoot_IfPresent()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedFile = Path.Combine(sourceRepoDir, "test.csproj");
            File.WriteAllText(
                expectedFile,
                ProjectFileWithMicrosoftAspNetCoreAppPackageReference);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var provider = CreateProjectFileProvider();

            // Act
            var actual = provider.GetProjectFile(sourceRepo);

            // Assert
            Assert.Equal(expectedFile, actual);
        }

        [Fact]
        public void GetProjectFile_Throws_IfSourceRepo_HasMultipleWebAppProjects()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(
                Path.Combine(webApp1Dir, "WebApp1.csproj"),
                ProjectFileWithMicrosoftAspNetCoreAppPackageReference);
            var webApp2Dir = CreateDir(srcDir, "WebApp2");
            File.WriteAllText(
                Path.Combine(webApp2Dir, "WebApp2.csproj"),
                ProjectFileWithMicrosoftAspNetCoreAppPackageReference);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var provider = CreateProjectFileProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidUsageException>(() => provider.GetProjectFile(sourceRepo));
            Assert.StartsWith(
                "Ambiguity in selecting an ASP.NET Core web application to build. Found multiple applications:",
                exception.Message);
        }

        [Fact]
        public void GetProjectFile_Throws_IfSourceRepo_HasMultipleWebAppProjects_AcrossDifferentFolderLevels()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(
                Path.Combine(webApp1Dir, "WebApp1.csproj"),
                ProjectFileWithMicrosoftAspNetCoreAppPackageReference);
            var fooDir = CreateDir(srcDir, "foo");
            var webApp2Dir = CreateDir(fooDir, "WebApp2");
            File.WriteAllText(
                Path.Combine(webApp2Dir, "WebApp2.csproj"),
                ProjectFileWithMicrosoftAspNetCoreAppPackageReference);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var provider = CreateProjectFileProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidUsageException>(() => provider.GetProjectFile(sourceRepo));
            Assert.StartsWith(
                "Ambiguity in selecting an ASP.NET Core web application to build. Found multiple applications:",
                exception.Message);
        }

        [Fact]
        public void GetProjectFile_DoesNotThrow_IfSourceRepo_HasMultipleWebAppProjects_AndProjectEnvVariableValue()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(
                Path.Combine(webApp1Dir, "WebApp1.csproj"),
                ProjectFileWithMicrosoftAspNetCoreAppPackageReference);
            var webApp2Dir = CreateDir(srcDir, "WebApp2");
            var expectedFile = Path.Combine(webApp2Dir, "WebApp2.csproj");
            File.WriteAllText(
                expectedFile,
                ProjectFileWithMicrosoftAspNetCoreAppPackageReference);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var relativeProjectPath = Path.Combine("src", "WebApp2", "WebApp2.csproj");
            var options = new DotnetCoreScriptGeneratorOptions();
            options.Project = relativeProjectPath;
            var provider = CreateProjectFileProvider(options);

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
