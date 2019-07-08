// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Python;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Python
{
    public class PythonBashBuildSnippetTest
    {
        [Fact]
        public void GeneratedSnippet_ContainsCollectStatic_IfDisableCollectStatic_IsFalse()
        {
            // Arrange
            var snippetProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: null,
                virtualEnvironmentModule: null,
                virtualEnvironmentParameters: null,
                packagesDirectory: "packages_dir",
                disableCollectStatic: false,
                compressVirtualEnvCommand: null,
                compressedVirtualEnvFileName: null);

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.Contains("manage.py collectstatic", text);
        }

        [Fact]
        public void GeneratedSnippet_DoesNotContainCollectStatic_IfDisableCollectStatic_IsTrue()
        {
            // Arrange
            var snippetProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: null,
                virtualEnvironmentModule: null,
                virtualEnvironmentParameters: null,
                packagesDirectory: "packages_dir",
                disableCollectStatic: true,
                compressVirtualEnvCommand: null,
                compressedVirtualEnvFileName: null);

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.DoesNotContain("manage.py collectstatic", text);
        }
    }
}
