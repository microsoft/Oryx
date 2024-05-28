// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Oryx.Detector.DotNetCore;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.Detector.Tests.DotNetCore
{
    public class ProbeAndFindProjectFileProviderTest : ProjectFileProviderTestBase
    {
        public ProbeAndFindProjectFileProviderTest(TestTempDirTestFixture testFixture) : base(testFixture)
        {
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsWebApp_WhenSourceRepoHasOtherValidProjectTypes()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(Path.Combine(webApp1Dir, "WebApp1.csproj"), WebSdkProjectFile);
            var azureFunctionsAppDir = CreateDir(srcDir, "AzureFunctionsApp1");
            File.WriteAllText(Path.Combine(
                azureFunctionsAppDir,
                "AzureFunctionsApp1.csproj"),
                AzureFunctionsProjectFile);
            var expectedRelativePath = Path.Combine("src", "WebApp1", "WebApp1.csproj");
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var provider = GetProjectFileProvider();

            // Act
            var actual = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(expectedRelativePath, actual);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsWebApp_EvenIfMultipleAzureFunctionsProjectsExist()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(Path.Combine(webApp1Dir, "WebApp1.csproj"), WebSdkProjectFile);
            var expectedRelativePath = Path.Combine("src", "WebApp1", "WebApp1.csproj");
            var azureFunctionsApp1Dir = CreateDir(srcDir, "AzureFunctionsApp1");
            File.WriteAllText(Path.Combine(
                azureFunctionsApp1Dir,
                "AzureFunctionsApp1.csproj"),
                AzureFunctionsProjectFile);
            var azureFunctionsApp2Dir = CreateDir(srcDir, "AzureFunctionsApp2");
            File.WriteAllText(Path.Combine(
                azureFunctionsApp2Dir,
                "AzureFunctionsApp2.csproj"),
                AzureFunctionsProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var provider = GetProjectFileProvider();

            // Act
            var actual = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(expectedRelativePath, actual);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsAzureFunctionsApp_OnlyWhenNoWebAppIsFound()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var app1Dir = CreateDir(srcDir, "App1");
            File.WriteAllText(Path.Combine(app1Dir, "App1.csproj"), NonWebSdkProjectFile);
            var azureFunctionsAppDir = CreateDir(srcDir, "AzureFunctionsApp1");
            File.WriteAllText(Path.Combine(
                azureFunctionsAppDir,
                "AzureFunctionsApp1.csproj"),
                AzureFunctionsProjectFile);
            var expectedRelativePath = Path.Combine("src", "AzureFunctionsApp1", "AzureFunctionsApp1.csproj");
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var options = new DetectorOptions();
            options.AppType = null;
            var provider = GetProjectFileProvider(options);

            // Act
            var actual = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(expectedRelativePath, actual);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsOnlyAzureFunctionsApp_WhenOryxAppType_IsSetTo_Functions()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(Path.Combine(webApp1Dir, "WebApp1.csproj"), WebSdkProjectFile);

            var azureFunctionsApp1Dir = CreateDir(srcDir, "AzureFunctionsApp1");
            File.WriteAllText(Path.Combine(
                azureFunctionsApp1Dir,
                "AzureFunctionsApp1.csproj"),
                AzureFunctionsProjectFile);

            var expectedRelativePath = Path.Combine("src", "AzureFunctionsApp1", "AzureFunctionsApp1.csproj");

            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var options = new DetectorOptions();
            options.AppType = Constants.FunctionApplications;
            var provider = GetProjectFileProvider(options);

            // Act
            var actual = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(expectedRelativePath, actual);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsOnlyBlazorWasmApp_WhenOryxAppType_IsSetTo_StaticSites()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(Path.Combine(webApp1Dir, "WebApp1.csproj"), WebSdkProjectFile);

            var azureFunctionsApp1Dir = CreateDir(srcDir, "AzureFunctionsApp1");
            File.WriteAllText(Path.Combine(
                azureFunctionsApp1Dir,
                "AzureFunctionsApp1.csproj"),
                AzureFunctionsProjectFile);

            var azureBlazorWasmApp1Dir = CreateDir(srcDir, "BlazorWasmApp1");
            File.WriteAllText(Path.Combine(
                azureBlazorWasmApp1Dir,
                "BlazorWasmApp1.csproj"),
                AzureBlazorWasmClientNetStandardProjectFile);
            var expectedRelativePath = Path.Combine("src", "BlazorWasmApp1", "BlazorWasmApp1.csproj");

            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var options = new DetectorOptions();
            options.AppType = Constants.StaticSiteApplications;

            var provider = GetProjectFileProvider(options);

            // Act
            var actual = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(expectedRelativePath, actual);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsOnlyWebApp_WhenOryxAppType_IsSetTo_WebApps()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(Path.Combine(webApp1Dir, "WebApp1.csproj"), WebSdkProjectFile);

            var azureFunctionsApp1Dir = CreateDir(srcDir, "AzureFunctionsApp1");
            File.WriteAllText(Path.Combine(
                azureFunctionsApp1Dir,
                "AzureFunctionsApp1.csproj"),
                AzureFunctionsProjectFile);

            var azureBlazorWasmApp1Dir = CreateDir(srcDir, "BlazorWasmApp1");
            File.WriteAllText(Path.Combine(
                azureBlazorWasmApp1Dir,
                "BlazorWasmApp1.csproj"),
                AzureBlazorWasmClientNetStandardProjectFile);
            var expectedRelativePath = Path.Combine("src", "WebApp1", "WebApp1.csproj");

            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var options = new DetectorOptions();
            options.AppType = Constants.WebApplications;

            var provider = GetProjectFileProvider(options);

            // Act
            var actual = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(expectedRelativePath, actual);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ThrowsWhenAppTypeIsSetToWebAppsAndMultipleProjectFilesAreFound()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(Path.Combine(webApp1Dir, "WebApp1.csproj"), WebSdkProjectFile);
            var webApp2Dir = CreateDir(srcDir, "WebApp2");
            File.WriteAllText(Path.Combine(webApp2Dir, "WebApp2.csproj"), WebSdkProjectFile);

            var azureFunctionsApp1Dir = CreateDir(srcDir, "AzureFunctionsApp1");
            File.WriteAllText(Path.Combine(
                azureFunctionsApp1Dir,
                "AzureFunctionsApp1.csproj"),
                AzureFunctionsProjectFile);

            var azureBlazorWasmApp1Dir = CreateDir(srcDir, "BlazorWasmApp1");
            File.WriteAllText(Path.Combine(
                azureBlazorWasmApp1Dir,
                "BlazorWasmApp1.csproj"),
                AzureBlazorWasmClientNetStandardProjectFile);

            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var options = new DetectorOptions();
            options.AppType = Constants.WebApplications;

            var provider = GetProjectFileProvider(options);

            // Act & Assert
            var exception = Assert.Throws<InvalidProjectFileException>(
                () => provider.GetRelativePathToProjectFile(context));
            Assert.StartsWith(
                "Ambiguity in selecting a project to build. Found multiple projects:",
                exception.Message);
            Assert.DoesNotContain("AzureFunctionsApp1.csproj", exception.Message);
            Assert.DoesNotContain("BlazorWasmApp1.csproj", exception.Message);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsNull_IfNoProjectFileFound()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var provider = GetProjectFileProvider();

            // Act
            var actual = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsWebApp_WhenOryxAppType_IsNotSet_EvenIfMultipleNonWebAppProjectsExist()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(Path.Combine(webApp1Dir, "WebApp1.csproj"), WebSdkProjectFile);

            var azureFunctionsApp1Dir = CreateDir(srcDir, "AzureFunctionsApp1");
            File.WriteAllText(Path.Combine(
                azureFunctionsApp1Dir,
                "AzureFunctionsApp1.csproj"),
                AzureFunctionsProjectFile);

            var azureBlazorWasmApp1Dir = CreateDir(srcDir, "BlazorWasmApp1");
            File.WriteAllText(Path.Combine(
                azureBlazorWasmApp1Dir,
                "BlazorWasmApp1.csproj"),
                AzureBlazorWasmClientNetStandardProjectFile);
            var expectedRelativePath = Path.Combine("src", "WebApp1", "WebApp1.csproj");

            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var provider = GetProjectFileProvider();

            // Act
            var actual = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(expectedRelativePath, actual);
        }

        [Theory]
        [InlineData(DotNetCoreConstants.CSharpProjectFileExtension)]
        [InlineData(DotNetCoreConstants.FSharpProjectFileExtension)]
        public void GetRelativePathToProjectFile_ReturnsNull_IfNoWebSdkProjectFound_AllAcrossRepo(
            string projectFileExtension)
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(Path.Combine(webApp1Dir, $"WebApp1.{projectFileExtension}"), NonWebSdkProjectFile);
            var webApp2Dir = CreateDir(srcDir, "WebApp2");
            File.WriteAllText(Path.Combine(webApp2Dir, $"WebApp2.{projectFileExtension}"), NonWebSdkProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var provider = GetProjectFileProvider();

            // Act
            var actual = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Null(actual);
        }

        [Theory]
        [InlineData(DotNetCoreConstants.CSharpProjectFileExtension)]
        [InlineData(DotNetCoreConstants.FSharpProjectFileExtension)]
        public void GetRelativePathToProjectFile_Throws_IfSourceRepo_HasMultipleWebSdkProjects(
            string projectFileExtension)
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(Path.Combine(webApp1Dir, $"WebApp1.{projectFileExtension}"), WebSdkProjectFile);
            var webApp2Dir = CreateDir(srcDir, "WebApp2");
            File.WriteAllText(Path.Combine(webApp2Dir, $"WebApp2.{projectFileExtension}"), WebSdkProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var provider = GetProjectFileProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidProjectFileException>(
                () => provider.GetRelativePathToProjectFile(context));
            Assert.StartsWith(
                "Ambiguity in selecting a project to build. Found multiple projects:",
                exception.Message);
        }

        [Theory]
        [InlineData(DotNetCoreConstants.CSharpProjectFileExtension)]
        [InlineData(DotNetCoreConstants.FSharpProjectFileExtension)]
        public void GetRelativePathToProjectFile_Throws_IfSourceRepo_HasMultipleWebAppProjects_AtDifferentDirLevels(
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
            var context = GetContext(sourceRepo);
            var provider = GetProjectFileProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidProjectFileException>(
                () => provider.GetRelativePathToProjectFile(context));
            Assert.StartsWith(
                "Ambiguity in selecting a project to build. Found multiple projects:",
                exception.Message);
        }

        [Theory]
        [InlineData(DotNetCoreConstants.CSharpProjectFileExtension)]
        [InlineData(DotNetCoreConstants.FSharpProjectFileExtension)]
        public void GetRelativePathToProjectFile_ReturnsProjectFile_ByProbingAllAcrossRepo(string projectFileExtension)
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(Path.Combine(webApp1Dir, $"WebApp1.{projectFileExtension}"), NonWebSdkProjectFile);
            var webApp2Dir = CreateDir(srcDir, "WebApp2");
            var projectFile = Path.Combine(webApp2Dir, $"WebApp2.{projectFileExtension}");
            File.WriteAllText(projectFile, WebSdkProjectFile);
            var expectedRelativePath = Path.Combine("src", "WebApp2", $"WebApp2.{projectFileExtension}");
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var provider = GetProjectFileProvider();

            // Act
            var actualPath = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(expectedRelativePath, actualPath);
        }

        [Fact]
        public void GetRelativePathToProjectFile_Throws_IfSourceRepo_HasMultipleAzureFunctionsProjects()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var azureFunctionsApp1Dir = CreateDir(srcDir, "AzureFunctionsApp1");
            File.WriteAllText(Path.Combine(
                azureFunctionsApp1Dir,
                "AzureFunctionsApp1.csproj"),
                AzureFunctionsProjectFile);
            var azureFunctionsApp2Dir = CreateDir(srcDir, "AzureFunctionsApp2");
            File.WriteAllText(Path.Combine(
                azureFunctionsApp2Dir,
                "AzureFunctionsApp2.csproj"),
                AzureFunctionsProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var provider = GetProjectFileProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidProjectFileException>(
                () => provider.GetRelativePathToProjectFile(context));
            Assert.StartsWith(
                "Ambiguity in selecting a project to build. Found multiple projects:",
                exception.Message);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsNull_WhenSourceRepoHasValidProjectType_ButDeepProbingIsDisabled()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(Path.Combine(webApp1Dir, "WebApp1.csproj"), WebSdkProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var options = new DetectorOptions
            {
                DisableRecursiveLookUp = true,
            };
            var provider = GetProjectFileProvider(options);

            // Act
            var actual = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsRelativePath_WhenSourceRepoHasValidProjectTypeAtRoot_AndDeepProbingIsDisabled()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            File.WriteAllText(Path.Combine(sourceRepoDir, "WebApp1.csproj"), WebSdkProjectFile);
            var expectedPath = "WebApp1.csproj";
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var options = new DetectorOptions
            {
                DisableRecursiveLookUp = true,
            };
            var provider = GetProjectFileProvider(options);

            // Act
            var actual = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(expectedPath, actual);
        }
    }
}