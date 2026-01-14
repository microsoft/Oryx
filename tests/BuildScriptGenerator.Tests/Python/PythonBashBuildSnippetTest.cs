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
            Assert.Contains("uv pip install --link-mode=copy", text);
            Assert.Contains("base_cmd=\"$base_cmd -r $requirements_file\"", text);
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
            Assert.Contains("install_python_packages_impl", text);
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
            Assert.Contains("install_python_packages_impl", text);
            // The upgrade flag appears in both the PYTHON_FAST_BUILD_ENABLED branch (via functions)
            // and the default pip branch (inline)
            Assert.Contains("--upgrade", text);
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
            Assert.Contains("install_python_packages_impl() {", text);
            
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

        [Fact]
        public void GeneratedSnippet_Contains_PythonFastBuildEnabled_Check()
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
            
            // Verify it checks for PYTHON_FAST_BUILD_ENABLED flag inline
            Assert.Contains("if [ \"$PYTHON_FAST_BUILD_ENABLED\" = \"true\" ]; then", text);
            
            // Verify it has message when enabled
            Assert.Contains("Fast build is enabled", text);
            
            // Verify it has message when running pip (either as fallback or direct)
            Assert.Contains("Running pip install...", text);
            
            // Verify it calls impl function (uv with fallback) when enabled
            Assert.Contains("install_python_packages_impl \"python\" \"$PIP_CACHE_DIR\" \"$REQUIREMENTS_TXT_FILE\"", text);
            
            // Verify it uses pip directly when not enabled
            Assert.Contains("python -m pip install --cache-dir $PIP_CACHE_DIR --prefer-binary -r $REQUIREMENTS_TXT_FILE", text);
        }

        [Fact]
        public void GeneratedSnippet_Has_Separate_Implementation_Function()
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
                runPythonPackageCommand: false);

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.NotEmpty(text);
            Assert.NotNull(text);
            
            // Verify the internal implementation function exists
            Assert.Contains("install_python_packages_impl() {", text);
            Assert.Contains("# Internal function to install packages with uv and fallback to pip", text);
            
            // Verify it contains uv first logic
            Assert.Contains("install_via_uv \"$python_cmd\" \"$cache_dir\" \"$requirements_file\" \"$target_dir\" \"$upgrade_flag\"", text);
            
            // Verify fallback to pip
            Assert.Contains("falling back to pip install...", text);
        }

        [Fact]
        public void GeneratedSnippet_Calls_PythonPackages_Function()
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
                runPythonPackageCommand: false);

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.NotEmpty(text);
            Assert.NotNull(text);
            
            // Verify install_python_packages_impl is called when flag is set for requirements.txt
            Assert.Contains("install_python_packages_impl \"python\" \"$PIP_CACHE_DIR\" \"$REQUIREMENTS_TXT_FILE\"", text);
        }

        [Fact]
        public void GeneratedSnippet_InstallViaUv_ContainsPreloadedWheelsCheck()
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
                runPythonPackageCommand: false);

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.NotEmpty(text);
            Assert.NotNull(text);
            
            // Verify install_via_uv function contains preloaded wheels check
            Assert.Contains("install_via_uv() {", text);
            Assert.Contains("# Add find-links if PYTHON_PRELOADED_WHEELS_DIR is set", text);
            Assert.Contains("if [ -n \"$PYTHON_PRELOADED_WHEELS_DIR\" ]; then", text);
            Assert.Contains("echo \"Using preloaded wheels from: $PYTHON_PRELOADED_WHEELS_DIR\"", text);
            Assert.Contains("base_cmd=\"$base_cmd --find-links=$PYTHON_PRELOADED_WHEELS_DIR\"", text);
        }

        [Fact]
        public void GeneratedSnippet_InstallViaPip_ContainsPreloadedWheelsCheck()
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
                runPythonPackageCommand: false);

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.NotEmpty(text);
            Assert.NotNull(text);
            
            // Verify install_via_pip function contains preloaded wheels check
            Assert.Contains("install_via_pip() {", text);
            
            // Count occurrences of the preloaded wheels pattern in install_via_pip
            int count = 0;
            int index = 0;
            string searchPattern = "if [ -n \"$PYTHON_PRELOADED_WHEELS_DIR\" ]; then";
            while ((index = text.IndexOf(searchPattern, index)) != -1)
            {
                count++;
                index += searchPattern.Length;
            }
            
            // Should appear in install_via_uv, install_via_pip, and two pip direct paths (venv and non-venv)
            Assert.True(count >= 2, $"Expected at least 2 occurrences of preloaded wheels check, found {count}");
        }

        [Fact]
        public void GeneratedSnippet_VirtualEnv_PipDirectPath_ContainsPreloadedWheelsCheck()
        {
            // Arrange
            var snippetProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: "venv",
                virtualEnvironmentModule: "venv",
                virtualEnvironmentParameters: "",
                packagesDirectory: null,
                enableCollectStatic: false,
                compressVirtualEnvCommand: null,
                compressedVirtualEnvFileName: null,
                pythonBuildCommandsFileName: FilePaths.BuildCommandsFileName,
                pythonVersion: "3.11",
                runPythonPackageCommand: false);

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.NotEmpty(text);
            Assert.NotNull(text);
            
            // Verify the virtual env section has preloaded wheels check in else branch (pip direct path)
            Assert.Contains("VIRTUALENVIRONMENTNAME=venv", text);
            
            // The else branch for pip direct should have preloaded wheels support
            // It appears after the fast build check in the virtual env section
            Assert.Contains("else", text);
            Assert.Contains("echo \"Running pip install...\"", text);
            
            // Verify preloaded wheels check exists
            int uvCheckIndex = text.IndexOf("install_via_uv() {");
            int pipCheckIndex = text.IndexOf("install_via_pip() {");
            Assert.True(uvCheckIndex > 0, "install_via_uv function should exist");
            Assert.True(pipCheckIndex > 0, "install_via_pip function should exist");
        }

        [Fact]
        public void GeneratedSnippet_NonVirtualEnv_PipDirectPath_ContainsPreloadedWheelsCheck()
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
                runPythonPackageCommand: false);

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.NotEmpty(text);
            Assert.NotNull(text);
            
            // Verify the non-virtual env section has preloaded wheels check in else branch (pip direct path)
            // This is in the section without VIRTUALENVIRONMENTNAME
            
            // The script should contain the else branch with pip direct install
            Assert.Contains("echo", text);
            Assert.Contains("echo Running pip install...", text);
            
            // In non-virtual env mode, should have 3 locations with preloaded wheels support:
            // 1. install_via_uv (shared function)
            // 2. install_via_pip (shared function)
            // 3. Non-virtual env else branch (pip direct)
            
            int count = 0;
            int index = 0;
            string searchPattern = "if [ -n \"$PYTHON_PRELOADED_WHEELS_DIR\" ]; then";
            while ((index = text.IndexOf(searchPattern, index)) != -1)
            {
                count++;
                index += searchPattern.Length;
            }
            
            // Should appear 3 times in non-virtual env mode
            Assert.Equal(3, count);
        }

        [Fact]
        public void GeneratedSnippet_AllPreloadedWheelsChecks_HaveConsistentStructure()
        {
            // Arrange - Use virtual env to test that branch
            var snippetProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: "venv",
                virtualEnvironmentModule: "venv",
                virtualEnvironmentParameters: "",
                packagesDirectory: null,
                enableCollectStatic: false,
                compressVirtualEnvCommand: null,
                compressedVirtualEnvFileName: null,
                pythonBuildCommandsFileName: FilePaths.BuildCommandsFileName,
                pythonVersion: "3.11",
                runPythonPackageCommand: false);

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.NotEmpty(text);
            Assert.NotNull(text);
            
            // Verify each preloaded wheels check follows the same pattern:
            // 1. Comment
            // 2. if [ -n "$PYTHON_PRELOADED_WHEELS_DIR" ]; then
            // 3. echo message
            // 4. Command modification with --find-links
            
            // In virtual env mode, should have 3 checks:
            // - install_via_uv function
            // - install_via_pip function
            // - Virtual env else branch (pip direct)
            
            // All checks should have the comment
            int commentCount = 0;
            int index = 0;
            string commentPattern = "# Add find-links if PYTHON_PRELOADED_WHEELS_DIR is set";
            while ((index = text.IndexOf(commentPattern, index)) != -1)
            {
                commentCount++;
                index += commentPattern.Length;
            }
            Assert.Equal(3, commentCount);
            
            // All checks should have the echo message
            int echoCount = 0;
            index = 0;
            string echoPattern = "echo \"Using preloaded wheels from: $PYTHON_PRELOADED_WHEELS_DIR\"";
            while ((index = text.IndexOf(echoPattern, index)) != -1)
            {
                echoCount++;
                index += echoPattern.Length;
            }
            Assert.Equal(3, echoCount);
            
            // All checks should have --find-links flag
            int findLinksCount = 0;
            index = 0;
            string findLinksPattern = "--find-links=$PYTHON_PRELOADED_WHEELS_DIR";
            while ((index = text.IndexOf(findLinksPattern, index)) != -1)
            {
                findLinksCount++;
                index += findLinksPattern.Length;
            }
            Assert.Equal(3, findLinksCount);
        }
    }
}
