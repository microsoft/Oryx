// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.DotNetCore
{
    public class RootDirectoryProjectFileProviderTest : ProjectFileProviderTestBase
    {
        public RootDirectoryProjectFileProviderTest(TestTempDirTestFixture testFixture) : base(testFixture)
        {
        }

        [Theory]
        [InlineData(DotNetCoreConstants.CSharpProjectFileExtension)]
        [InlineData(DotNetCoreConstants.FSharpProjectFileExtension)]
        public void GetRelativePathToProjectFile_ReturnsProjectFile_PresentAtRoot_IfPresent(
            string projectFileExtension)
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedRelativePath = $"WebApp1.{projectFileExtension}";
            var projectFile = Path.Combine(sourceRepoDir, expectedRelativePath);
            File.WriteAllText(projectFile, WebSdkProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var providers = GetProjectFileProviders();

            // Act
            var actualPath = ProjectFileProviderHelper.GetRelativePathToProjectFile(providers, context);

            // Assert
            Assert.Equal(expectedRelativePath, actualPath);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsProjectFile_IfSdkHasBothNameAndVersion()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedRelativePath = "WebApp1.csproj";
            var projectFile = Path.Combine(sourceRepoDir, expectedRelativePath);
            File.WriteAllText(projectFile, WebSdkProjectFileWithVersion);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var providers = GetProjectFileProviders();

            // Act
            var actualPath = ProjectFileProviderHelper.GetRelativePathToProjectFile(providers, context);

            // Assert
            Assert.Equal(expectedRelativePath, actualPath);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsNull_IfSdkHasBothNameAndVersion_AndIsNotWebSdk()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var projectFile = Path.Combine(sourceRepoDir, "WebApp1.csproj");
            File.WriteAllText(projectFile, NonWebSdkProjectFileWithVersion);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var providers = GetProjectFileProviders();

            // Act
            var actual = ProjectFileProviderHelper.GetRelativePathToProjectFile(providers, context);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsProjectFile_IfSdkIsPresentAsElement()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedRelativePath = "WebApp1.csproj";
            var projectFile = Path.Combine(sourceRepoDir, expectedRelativePath);
            File.WriteAllText(projectFile, WebSdkProjectFileWithSdkInfoAsElement);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var providers = GetProjectFileProviders();

            // Act
            var actual = ProjectFileProviderHelper.GetRelativePathToProjectFile(providers, context);

            // Assert
            Assert.Equal(expectedRelativePath, actual);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsNull_IfSdkIsPresentAsElement_AndIsNotWebSdk()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var projectFile = Path.Combine(sourceRepoDir, "WebApp1.csproj");
            File.WriteAllText(projectFile, NonWebSdkProjectFileWithSdkInfoAsElement);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var providers = GetProjectFileProviders();

            // Act
            var actual = ProjectFileProviderHelper.GetRelativePathToProjectFile(providers, context);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsNull_WhenNoInformationAboutSdkIsPresentInProjectFile()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var projectFile = Path.Combine(sourceRepoDir, "WebApp1.csproj");
            File.WriteAllText(projectFile, NoSdkInformationProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var providers = GetProjectFileProviders();

            // Act
            var actual = ProjectFileProviderHelper.GetRelativePathToProjectFile(providers, context);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsProjectFile_IfMultiplePlacesHaveSdkInfo()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedRelativePath = "WebApp1.csproj";
            var projectFile = Path.Combine(sourceRepoDir, expectedRelativePath);
            File.WriteAllText(projectFile, WebSdkProjectFileWithMultipleSdkInfoAsElement);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var providers = GetProjectFileProviders();

            // Act
            var actualPath = ProjectFileProviderHelper.GetRelativePathToProjectFile(providers, context);

            // Assert
            Assert.Equal(expectedRelativePath, actualPath);
        }

        [Theory]
        [InlineData(DotNetCoreConstants.CSharpProjectFileExtension)]
        [InlineData(DotNetCoreConstants.FSharpProjectFileExtension)]
        public void GetRelativePathToProjectFile_ReturnsNull_IfRootProject_IsNotWebSdkProject(
            string projectFileExtension)
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var projectFile = Path.Combine(sourceRepoDir, $"WebApp1.{projectFileExtension}");
            File.WriteAllText(projectFile, NonWebSdkProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var providers = GetProjectFileProviders();

            // Act
            var actual = ProjectFileProviderHelper.GetRelativePathToProjectFile(providers, context);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsAzureFunctionsProjectFile_PresentAtRoot_IfPresent()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedRelativePath = "AzureFunctionsTest.csproj";
            var projectFile = Path.Combine(sourceRepoDir, expectedRelativePath);
            File.WriteAllText(projectFile, AzureFunctionsProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var providers = GetProjectFileProviders();

            // Act
            var actualPath = ProjectFileProviderHelper.GetRelativePathToProjectFile(providers, context);

            // Assert
            Assert.Equal(expectedRelativePath, actualPath);
        }
    }
}
