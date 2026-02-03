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
    public class ExplicitProjectFileProviderTest : ProjectFileProviderTestBase
    {
        public ExplicitProjectFileProviderTest(TestTempDirTestFixture testFixture) : base(testFixture)
        {
        }

        // This is a scenario where our probing of a project could be incorrect. In this case a user can explicitly
        // specify the project file to use and we use it without checking further.
        [Theory]
        [InlineData(DotNetCoreConstants.CSharpProjectFileExtension)]
        [InlineData(DotNetCoreConstants.FSharpProjectFileExtension)]
        public void GetRelativePathToProjectFile_ReturnsFile_IfProjEnvVariableIsSet_AndProjectFileIsNotSupported(
            string projectFileExtension)
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            var projectFile = Path.Combine(webApp1Dir, $"WebApp1.{projectFileExtension}");
            File.WriteAllText(projectFile, NonWebSdkProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var relativeProjectPath = Path.Combine("src", "WebApp1", $"WebApp1.{projectFileExtension}");
            var options = new DetectorOptions();
            options.Project = relativeProjectPath;
            var context = GetContext(sourceRepo);
            var provider = GetProjectFileProvider(options);

            // Act
            var actualFilePath = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(relativeProjectPath, actualFilePath);
        }

        [Fact]
        public void GetRelativePathToProjectFile_Throws_IfPathInProjectEnvVariableValue_DoesNotExist()
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
            var options = new DetectorOptions();
            options.Project = relativeProjectPath;
            var context = GetContext(sourceRepo);
            var provider = GetProjectFileProvider(options);

            // Act & Assert
            var exception = Assert.Throws<InvalidProjectFileException>(
                () => provider.GetRelativePathToProjectFile(context));
            Assert.Contains("Could not find the .NET Core project file specified in PROJECT environment variable", exception.Message);
        }

        [Theory]
        [InlineData(DotNetCoreConstants.CSharpProjectFileExtension)]
        [InlineData(DotNetCoreConstants.FSharpProjectFileExtension)]
        public void GetRelativePathToProjectFile_DoesNotThrow_IfRepoHasMultipleWebApps_AndProjectEnvVariableIsSet(
            string projectFileExtension)
        {
            // Arrange
            var sourceRepoDir = CreateSourceRepoDir();
            var srcDir = CreateDir(sourceRepoDir, "src");
            var webApp1Dir = CreateDir(srcDir, "WebApp1");
            File.WriteAllText(Path.Combine(webApp1Dir, $"WebApp1.{projectFileExtension}"), webApp1Dir);
            var webApp2Dir = CreateDir(srcDir, "WebApp2");
            var projectFile = Path.Combine(webApp2Dir, $"WebApp2.{projectFileExtension}");
            File.WriteAllText(projectFile, webApp1Dir);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var relativeProjectPath = Path.Combine("src", "WebApp2", $"WebApp2.{projectFileExtension}");
            var options = new DetectorOptions();
            options.Project = relativeProjectPath;
            var context = GetContext(sourceRepo);
            var provider = GetProjectFileProvider(options);

            // Act
            var actualFile = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(relativeProjectPath, actualFile);
        }

        // Scenario: There could be multiple project files under the same directory in which case a user might want
        // to choose one explicitly.
        [Fact]
        public void GetRelativePathToProjectFile_ReturnsFile_IfProjEnvVariableIsSet_AndIsAtRoot()
        {
            // Arrange
            var expectedPath = "WebApp1.csproj";
            var sourceRepoDir = CreateSourceRepoDir();
            var projectFile = Path.Combine(sourceRepoDir, expectedPath);
            File.WriteAllText(projectFile, WebSdkProjectFile);
            var sourceRepo = CreateSourceRepo(sourceRepoDir);
            var context = GetContext(sourceRepo);
            var detectorOptions = new DetectorOptions();
            detectorOptions.Project = expectedPath;
            var provider = GetProjectFileProvider(detectorOptions);

            // Act
            var actualFilePath = provider.GetRelativePathToProjectFile(context);

            // Assert
            Assert.Equal(expectedPath, actualFilePath);
        }
    }
}
