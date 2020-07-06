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
            var provider = GetProjectFileProvider();

            // Act
            var actualPath = provider.GetRelativePathToProjectFile(context);

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
            var provider = GetProjectFileProvider();

            // Act
            var actualPath = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(expectedRelativePath, actualPath);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsFile_IfSdkHasBothNameAndVersion_AndIsNotWebSdk()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedRelativePath = "WebApp1.csproj";
            var projectFile = Path.Combine(sourceRepoDir, expectedRelativePath);
            File.WriteAllText(projectFile, NonWebSdkProjectFileWithVersion);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var provider = GetProjectFileProvider();

            // Act
            var actual = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(expectedRelativePath, actual);
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
            var provider = GetProjectFileProvider();

            // Act
            var actual = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(expectedRelativePath, actual);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsFile_IfSdkIsPresentAsElement_AndIsNotWebSdk()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedRelativePath = "WebApp1.csproj";
            var projectFile = Path.Combine(sourceRepoDir, expectedRelativePath);
            File.WriteAllText(projectFile, NonWebSdkProjectFileWithSdkInfoAsElement);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var provider = GetProjectFileProvider();

            // Act
            var actual = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(expectedRelativePath, actual);
        }

        [Fact]
        public void GetRelativePathToProjectFile_ReturnsFile_WhenNoInformationAboutSdkIsPresentInProjectFile()
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedRelativePath = "WebApp1.csproj";
            var projectFile = Path.Combine(sourceRepoDir, expectedRelativePath);
            File.WriteAllText(projectFile, NoSdkInformationProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var provider = GetProjectFileProvider();

            // Act
            var actual = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(expectedRelativePath, actual);
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
            var provider = GetProjectFileProvider();

            // Act
            var actualPath = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(expectedRelativePath, actualPath);
        }

        [Theory]
        [InlineData(DotNetCoreConstants.CSharpProjectFileExtension)]
        [InlineData(DotNetCoreConstants.FSharpProjectFileExtension)]
        public void GetRelativePathToProjectFile_ReturnsFile_IfRootProject_IsNotWebSdkProject(
            string projectFileExtension)
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var expectedRelativePath = $"WebApp1.{projectFileExtension}";
            var projectFile = Path.Combine(sourceRepoDir, expectedRelativePath);
            File.WriteAllText(projectFile, NonWebSdkProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var provider = GetProjectFileProvider();

            // Act
            var actual = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(expectedRelativePath, actual);
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
            var provider = GetProjectFileProvider();

            // Act
            var actualPath = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(expectedRelativePath, actualPath);
        }
    }
}
