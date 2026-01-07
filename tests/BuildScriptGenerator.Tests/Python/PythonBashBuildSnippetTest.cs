// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Common;
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
                enableCollectStatic: true,
                compressVirtualEnvCommand: null,
                compressedVirtualEnvFileName: null,
                pythonBuildCommandsFileName: FilePaths.BuildCommandsFileName,
                pythonVersion: null,
                runPythonPackageCommand: false
                );

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.Contains("manage.py collectstatic", text);
        }

        [Fact]
        public void GeneratedSnippet_Contains_BuildCommands_And_PythonVersion_Info()
        {
            // Arrange
            var snippetProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: null,
                virtualEnvironmentModule: null,
                virtualEnvironmentParameters: null,
                packagesDirectory: "packages_dir",
                enableCollectStatic: true,
                compressVirtualEnvCommand: null,
                compressedVirtualEnvFileName: null,
                pythonBuildCommandsFileName: FilePaths.BuildCommandsFileName,
                pythonVersion: "3.6",
                runPythonPackageCommand: false
                );

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.NotEmpty(text);
            Assert.NotNull(text);
            Assert.Contains("COMMAND_MANIFEST_FILE=\"oryx-build-commands.txt\"", text);

        }

        [Fact]
        public void GeneratedSnippet_ContainsBuildCommand_WhenCustomRequirementsTxtExists()
        {
            // Arrange
            var snippetProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: null,
                virtualEnvironmentModule: null,
                virtualEnvironmentParameters: null,
                packagesDirectory: "packages_dir",
                enableCollectStatic: true,
                compressVirtualEnvCommand: null,
                compressedVirtualEnvFileName: null,
                pythonBuildCommandsFileName: FilePaths.BuildCommandsFileName,
                pythonVersion: "3.6",
                runPythonPackageCommand: false,
                customRequirementsTxtPath: "foo/requirements.txt"
                );

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.NotEmpty(text);
            Assert.NotNull(text);
            Assert.Contains("uv pip install --cache-dir $PIP_CACHE_DIR -r $REQUIREMENTS_TXT_FILE", text);

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
                enableCollectStatic: false,
                compressVirtualEnvCommand: null,
                compressedVirtualEnvFileName: null,
                pythonBuildCommandsFileName: FilePaths.BuildCommandsFileName,
                pythonVersion: null,
                runPythonPackageCommand: false);

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.DoesNotContain("manage.py collectstatic", text);
        }

        [Fact]
        public void GeneratedSnippet_DoesNotContainPackageWheelType_If_PackageWheelType_IsNotProvided()
        {
            // Arrange
            var snippetProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: null,
                virtualEnvironmentModule: null,
                virtualEnvironmentParameters: null,
                packagesDirectory: "packages_dir",
                enableCollectStatic: false,
                compressVirtualEnvCommand: null,
                compressedVirtualEnvFileName: null,
                pythonBuildCommandsFileName: FilePaths.BuildCommandsFileName,
                pythonVersion: null,
                runPythonPackageCommand: true);

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.DoesNotContain("Creating universal package wheel", text);
            Assert.Contains("setup.py sdist --formats=gztar,zip,tar bdist_wheel", text);
        }

        [Fact]
        public void GeneratedSnippet_DoesNotContainPackageWheelType_When_PackageCommand_IsNotPresent()
        {
            // Arrange
            var snippetProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: null,
                virtualEnvironmentModule: null,
                virtualEnvironmentParameters: null,
                packagesDirectory: "packages_dir",
                enableCollectStatic: false,
                compressVirtualEnvCommand: null,
                compressedVirtualEnvFileName: null,
                runPythonPackageCommand: false,
                pythonBuildCommandsFileName: FilePaths.BuildCommandsFileName,
                pythonVersion: null,
                pythonPackageWheelProperty: "universal");

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.DoesNotContain("Creating universal package wheel", text);
            Assert.DoesNotContain("Creating non universal package wheel", text);
        }

        [Fact]
        public void GeneratedSnippet_ContainsPackageWheelType_When_PackageCommandAndPackageWheelType_IsPresent()
        {
            // Arrange
            var snippetProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: null,
                virtualEnvironmentModule: null,
                virtualEnvironmentParameters: null,
                packagesDirectory: "packages_dir",
                enableCollectStatic: false,
                compressVirtualEnvCommand: null,
                compressedVirtualEnvFileName: null,
                runPythonPackageCommand: true,
                pythonBuildCommandsFileName: FilePaths.BuildCommandsFileName,
                pythonVersion: null,
                pythonPackageWheelProperty: "universal");

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.Contains("Creating universal package wheel", text);
            Assert.Contains("setup.py sdist --formats=gztar,zip,tar bdist_wheel --universal", text);
        }

        [Fact]
        public void GeneratedSnippet_DisablePipUpgradeFlag_IfPipUpgradeFlag_IsEmpty()
        {
            // Arrange
            var snippetProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: null,
                virtualEnvironmentModule: null,
                virtualEnvironmentParameters: null,
                packagesDirectory: "packages_dir",
                enableCollectStatic: false,
                compressVirtualEnvCommand: null, 
                compressedVirtualEnvFileName: null,
                runPythonPackageCommand: true,
                pythonBuildCommandsFileName: FilePaths.BuildCommandsFileName,
                pythonVersion: null,
                pythonPackageWheelProperty: "universal",
                pipUpgradeFlag: string.Empty);

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.NotEmpty(text);
            Assert.NotNull(text);
            string extectedString = "uv pip install --cache-dir $PIP_CACHE_DIR -r $REQUIREMENTS_TXT_FILE --target=\"" + snippetProps.PackagesDirectory + "\"  | ts $TS_FMT";
            Assert.Contains(extectedString, text);
        }

        [Fact]
        public void GeneratedSnippet_EnablePipUpgradeFlag()
        {
            // Arrange
            var snippetProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: null,
                virtualEnvironmentModule: null,
                virtualEnvironmentParameters: null,
                packagesDirectory: "packages_dir",
                enableCollectStatic: false,
                compressVirtualEnvCommand: null,
                compressedVirtualEnvFileName: null,
                runPythonPackageCommand: true,
                pythonBuildCommandsFileName: FilePaths.BuildCommandsFileName,
                pythonVersion: null,
                pythonPackageWheelProperty: "universal",
                pipUpgradeFlag: "--upgrade");

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.NotEmpty(text);
            Assert.NotNull(text);
            string extectedString = "uv pip install --cache-dir $PIP_CACHE_DIR -r $REQUIREMENTS_TXT_FILE --target=\"" + snippetProps.PackagesDirectory + "\" --upgrade | ts $TS_FMT";
            Assert.Contains(extectedString, text);
        }
    }
}
