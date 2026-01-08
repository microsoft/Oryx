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
            Assert.Contains("install_via_uv() {", text);
            Assert.Contains("uv pip install --cache-dir $cache_dir -r $requirements_file", text);

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
            Assert.Contains("install_packages_with_fallback", text);
            Assert.Contains("install_via_uv() {", text);
            Assert.Contains("install_via_pip() {", text);
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
            Assert.Contains("install_packages_with_fallback", text);
            Assert.Contains("\"--upgrade\"", text);
        }

        [Fact]
        public void GeneratedSnippet_ContainsFallbackLogic_FromUvToPip()
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
                pythonVersion: "3.11",
                runPythonPackageCommand: false,
                customRequirementsTxtPath: null,
                pythonPackageWheelProperty: null,
                pipUpgradeFlag: string.Empty);

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.NotEmpty(text);
            Assert.NotNull(text);
            
            // Verify orchestrator function exists
            Assert.Contains("install_packages_with_fallback() {", text);
            
            // Verify it tries uv first
            Assert.Contains("install_via_uv \"$python_cmd\" \"$cache_dir\" \"$requirements_file\" \"$target_dir\" \"$upgrade_flag\"", text);
            
            // Verify fallback logic exists
            Assert.Contains("if [[ $exit_code != 0 ]]; then", text);
            Assert.Contains("uv pip install failed with exit code", text);
            Assert.Contains("falling back to pip install", text);
            
            // Verify it calls pip on fallback
            Assert.Contains("install_via_pip \"$python_cmd\" \"$cache_dir\" \"$requirements_file\" \"$target_dir\" \"$upgrade_flag\"", text);
            
            // Verify both installation functions are defined
            Assert.Contains("install_via_uv() {", text);
            Assert.Contains("install_via_pip() {", text);
        }
    }
}
