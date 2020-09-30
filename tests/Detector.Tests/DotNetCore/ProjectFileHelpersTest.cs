// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Xml.Linq;
using Microsoft.Oryx.Detector.DotNetCore;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.Detector.Tests.DotNetCore
{
    public class ProjectFileHelpersTest : ProjectFileProviderTestBase
    {
        public ProjectFileHelpersTest(TestTempDirTestFixture testFixture) : base(testFixture)
        {
        }

        [Theory]
        [InlineData(NonWebSdkProjectFile)]
        [InlineData(AzureFunctionsProjectFile)]
        public void IsAspNetCoreWebApplicationProject_ReturnsFalse_WhenProject_IsNotWebSdkProject(string projectFile)
        {
            // Arrange
            var xdoc = XDocument.Load(new StringReader(projectFile));

            // Act
            var actual = ProjectFileHelpers.IsAspNetCoreWebApplicationProject(xdoc);

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void IsAspNetCoreWebApplicationProject_ReturnsTrue_WhenProject_IsWebSdkProject()
        {
            // Arrange
            var xdoc = XDocument.Load(new StringReader(WebSdkProjectFile));

            // Act
            var actual = ProjectFileHelpers.IsAspNetCoreWebApplicationProject(xdoc);

            // Assert
            Assert.True(actual);
        }

        [Theory]
        [InlineData(AzureFunctionsProjectFile)]
        [InlineData(AzureFunctionsProjectFileWithoutAzureFunctionsVersionProperty)]
        public void IsAzureFunctionsProject_ReturnsTrue_WhenProject_IsAzureFunctionsProject(string projectFile)
        {
            // Arrange
            var xdoc = XDocument.Load(new StringReader(projectFile));

            // Act
            var actual = ProjectFileHelpers.IsAzureFunctionsProject(xdoc);

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void IsAzureBlazorWebAssemblyProject_ReturnsTrue_WhenProject_Is_AzureBlazorWebAssemblyProject()
        {
            // Arrange
            var projectFile = ProjectFileProviderTestBase.AzureBlazorWasmClientNetStandardProjectFile;
            var xdoc = XDocument.Load(new StringReader(projectFile));

            // Act
            var actual = ProjectFileHelpers.IsBlazorWebAssemblyProject(xdoc);

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void IsAzureBlazorWebAssemblyProject_ReturnsTrue_WhenProject_Is_NonNetStandardBlazorWebAssemblyProject()
        {
            // Arrange0
            var projectFile = ProjectFileProviderTestBase.AzureBlazorWasmClientNet5ProjectFile;
            var xdoc = XDocument.Load(new StringReader(projectFile));

            // Act
            var actual = ProjectFileHelpers.IsBlazorWebAssemblyProject(xdoc);

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void IsAzureBlazorWebAssemblyProject_ReturnsFalse_WhenProject_IsNot_AzureBlazorWebAssemblyProject()
        {
            // Arrange
            var projectFile = ProjectFileProviderTestBase.AzureNonBlazorWasmProjectFile;
            var xdoc = XDocument.Load(new StringReader(projectFile));

            // Act
            var actual = ProjectFileHelpers.IsBlazorWebAssemblyProject(xdoc);

            // Assert
            Assert.False(actual);
        }

        public static TheoryData<string, string, string> GetRelativePathToRootData
        {
            get
            {
                var data = new TheoryData<string, string, string>();

                var repoRoot = Path.Combine("c:", "apps");
                var expectedPath = Path.Combine("src", "webapp1", "webapp1.csproj");
                var projectFile = Path.Combine(repoRoot, expectedPath);
                data.Add(projectFile, repoRoot, expectedPath);

                repoRoot = Path.Combine("c:", "apps");
                expectedPath = "webapp1.csproj";
                projectFile = Path.Combine(repoRoot, expectedPath);
                data.Add(projectFile, repoRoot, expectedPath);

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(GetRelativePathToRootData))]
        public void GetRelativePathToRoot_ReturnsPathRelativeToRoot(
            string projectFile,
            string repoRoot,
            string expectedRelativePath)
        {
            // Arrange & Act
            var actualPath = ProjectFileHelpers.GetRelativePathToRoot(projectFile, repoRoot);

            // Assert
            Assert.Equal(expectedRelativePath, actualPath);
        }
    }
}