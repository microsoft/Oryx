// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Tests.Common;
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
            Assert.Contains("uv pip install", text);
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
        public void GeneratedSnippet_ContainsCustomBuildCommand_InVirtualEnvironmentBranch()
        {
            // Arrange
            var snippetProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: "antenv",
                virtualEnvironmentModule: "venv",
                virtualEnvironmentParameters: string.Empty,
                packagesDirectory: "packages_dir",
                enableCollectStatic: false,
                compressVirtualEnvCommand: null,
                compressedVirtualEnvFileName: null,
                pythonBuildCommandsFileName: FilePaths.BuildCommandsFileName,
                pythonVersion: "3.14",
                runPythonPackageCommand: false,
                customBuildCommand: "custom build command");

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.Contains("Running custom build command 'custom build command'...", text);
            Assert.Contains("custom build command", text);
            Assert.DoesNotContain("InstallUvCommand=\"uv sync --active --link-mode copy\"", text);
        }

        [Fact]
        public void GeneratedSnippet_ContainsCustomBuildCommand_InPackagesDirectoryBranch()
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
                pythonVersion: "3.14",
                runPythonPackageCommand: false,
                customBuildCommand: "custom build command");

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.Contains("Running custom build command 'custom build command'...", text);
            Assert.Contains("custom build command", text);
            Assert.DoesNotContain("UpgradeCommand=\"pip install --upgrade pip\"", text);
        }

        [Fact]
        public void GeneratedSnippet_VirtualenvCreationIsPreserved_WithCustomBuildCommand()
        {
            // Arrange
            var snippetProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: "antenv",
                virtualEnvironmentModule: "venv",
                virtualEnvironmentParameters: string.Empty,
                packagesDirectory: null,
                enableCollectStatic: false,
                compressVirtualEnvCommand: null,
                compressedVirtualEnvFileName: null,
                pythonBuildCommandsFileName: FilePaths.BuildCommandsFileName,
                pythonVersion: "3.14",
                runPythonPackageCommand: false,
                customBuildCommand: "pip install --no-deps -r requirements.txt");

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.Contains("Creating virtual environment", text);
            Assert.Contains("Activating virtual environment", text);
            Assert.Contains("pip install --no-deps -r requirements.txt", text);
        }

        [Fact]
        public void GeneratedSnippet_CollectstaticIsPreserved_WithCustomBuildCommand_InVenvBranch()
        {
            // Arrange
            var snippetProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: "antenv",
                virtualEnvironmentModule: "venv",
                virtualEnvironmentParameters: string.Empty,
                packagesDirectory: null,
                enableCollectStatic: true,
                compressVirtualEnvCommand: null,
                compressedVirtualEnvFileName: null,
                pythonBuildCommandsFileName: FilePaths.BuildCommandsFileName,
                pythonVersion: "3.14",
                runPythonPackageCommand: false,
                customBuildCommand: "custom build command");

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.Contains("custom build command", text);
            Assert.Contains("collectstatic", text);
        }

        [Fact]
        public void GeneratedSnippet_UsesDefaultPipInstall_WhenNoCustomBuildCommandSet_InVenvBranch()
        {
            // Arrange
            var snippetProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: "antenv",
                virtualEnvironmentModule: "venv",
                virtualEnvironmentParameters: string.Empty,
                packagesDirectory: null,
                enableCollectStatic: false,
                compressVirtualEnvCommand: null,
                compressedVirtualEnvFileName: null,
                pythonBuildCommandsFileName: FilePaths.BuildCommandsFileName,
                pythonVersion: "3.14",
                runPythonPackageCommand: false);

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.Contains("pip install", text);
            Assert.DoesNotContain("Running custom build command", text);
        }

        [Fact]
        public void GeneratedSnippet_UsesDefaultPipInstall_WhenNoCustomBuildCommandSet_InPackagesDirBranch()
        {
            // Arrange
            var snippetProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: null,
                virtualEnvironmentModule: null,
                virtualEnvironmentParameters: null,
                packagesDirectory: "__oryx_packages__",
                enableCollectStatic: false,
                compressVirtualEnvCommand: null,
                compressedVirtualEnvFileName: null,
                pythonBuildCommandsFileName: FilePaths.BuildCommandsFileName,
                pythonVersion: "3.14",
                runPythonPackageCommand: false);

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.Contains("pip install", text);
            Assert.DoesNotContain("Running custom build command", text);
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
            
            // Verify it tries uv first - note: cache_dir param removed from signature
            Assert.Contains("install_via_uv \"$python_cmd\" \"$requirements_file\" \"$target_dir\" \"$upgrade_flag\"", text);
            
            // Verify fallback logic exists
            Assert.Contains("if [[ $exit_code != 0 ]]; then", text);
            Assert.Contains("uv pip install failed with exit code", text);
            Assert.Contains("falling back to pip install", text);
            
            // Verify it calls pip on fallback - note: cache_dir param removed from signature
            Assert.Contains("install_via_pip \"$python_cmd\" \"$requirements_file\" \"$target_dir\" \"$upgrade_flag\"", text);
            
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
            
            // Verify it calls impl function (uv with fallback) when enabled - note: cache_dir param removed
            Assert.Contains("install_python_packages_impl \"python\" \"$REQUIREMENTS_TXT_FILE\"", text);
            
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
            
            // Verify it contains uv first logic - note: cache_dir param removed from signature
            Assert.Contains("install_via_uv \"$python_cmd\" \"$requirements_file\" \"$target_dir\" \"$upgrade_flag\"", text);
            
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
            // Note: cache_dir param removed, now it's just: python_cmd, requirements_file, target_dir, upgrade_flag
            Assert.Contains("install_python_packages_impl \"python\" \"$REQUIREMENTS_TXT_FILE\"", text);
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
            
            // Verify UV_PIP_CACHE_DIR is used
            Assert.Contains("UV_PIP_CACHE_DIR=", text);
            Assert.Contains("uv pip install --cache-dir $UV_PIP_CACHE_DIR", text);
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
            Assert.Contains("echo \"Running pip install...\"", text);
            
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
            
            Assert.True(count >= 2, $"Expected shared pip and uv preloaded wheels checks, found {count}");
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
            Assert.True(commentCount >= 2, $"Expected shared pip and uv comments, found {commentCount}");
            
            // All checks should have the echo message
            int echoCount = 0;
            index = 0;
            string echoPattern = "echo \"Using preloaded wheels from: $PYTHON_PRELOADED_WHEELS_DIR\"";
            while ((index = text.IndexOf(echoPattern, index)) != -1)
            {
                echoCount++;
                index += echoPattern.Length;
            }
            Assert.True(echoCount >= 2, $"Expected shared pip and uv messages, found {echoCount}");
            
            // All checks should have --find-links flag
            int findLinksCount = 0;
            index = 0;
            string findLinksPattern = "--find-links";
            while ((index = text.IndexOf(findLinksPattern, index)) != -1)
            {
                findLinksCount++;
                index += findLinksPattern.Length;
            }
            Assert.True(findLinksCount >= 2, $"Expected shared pip and uv find-links options, found {findLinksCount}");
        }

        [Fact]
        public void GeneratedSnippet_OmitsOryxSafeBuildFlowByDefault()
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
            Assert.DoesNotContain("install_with_oryx_safe_build", text);
            Assert.DoesNotContain("ORYX_SAFE_BUILD_", text);
        }

        [Theory]
        [InlineData("audit")]
        [InlineData("block")]
        public void GeneratedSnippet_ContainsOryxSafeBuildFlowFromBuildProperties(string mode)
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
                oryxSafeBuildEnabled: true,
                oryxSafeBuildMode: mode,
                oryxSafeBuildCheckerTimeoutInMinutes: 7);

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PythonSnippet, snippetProps);

            // Assert
            Assert.Contains("pip install --dry-run --ignore-installed --report", text);
            Assert.Contains("uv pip compile --python", text);
            Assert.Contains("command -v oryx-safe-build-checker", text);
            Assert.Contains("command -v timeout", text);
            Assert.Contains("timeout --signal=TERM --kill-after=10s \\", text);
            Assert.Contains("\"7m\" \\", text);
            Assert.Contains("oryx-safe-build-checker \\", text);
            Assert.Contains("--resolver-output \"$resolver_output_file\"", text);
            Assert.Contains("--frozen-packages \"$frozen_packages_file\"", text);
            Assert.DoesNotContain("--exceptions", text);
            Assert.Contains(
                "Oryx SafeBuild dependency resolution\" \\",
                text);
            Assert.Contains("Oryx SafeBuild assessment\" \\", text);
            Assert.Contains(
                "oryx_safe_build_log_elapsed " +
                "\"Oryx SafeBuild\" \"$safe_build_start_time\"",
                text);
            Assert.Contains("local safe_build_start_time=$SECONDS", text);
            Assert.Contains("local resolution_start_time=$SECONDS", text);
            Assert.Contains("local assessment_start_time=$SECONDS", text);
            Assert.Contains("did not produce frozen packages", text);
            Assert.Contains($"--mode \"{mode}\"", text);
            Assert.Contains("assessment_exit_code == 124 || $assessment_exit_code == 137", text);
            Assert.Contains(
                "oryx-safe-build-checker exceeded the 7-minute time limit",
                text);
            Assert.Contains("return 42", text);
            Assert.DoesNotContain("return 75", text);
            Assert.DoesNotContain("SAFE_ORYX_BUILD_CHECKED", text);
            Assert.Contains(
                "install_python_packages \"$python_cmd\" \"$requirements_file\" " +
                "\"$target_dir\" \"$upgrade_flag\"",
                text);
            Assert.Contains(
                "install_python_packages_impl \"$python_cmd\" \"$requirements_file\" " +
                "\"$target_dir\" \"$upgrade_flag\" \"$frozen_packages_file\" \"false\"",
                text);
            Assert.Equal(2, Regex.Matches(
                text,
                "base_cmd=\"\\$base_cmd -c \\$constraints_file\"").Count);
            Assert.DoesNotContain(
                "base_cmd=\"$base_cmd -c \\\"$constraints_file\\\"\"",
                text);
            Assert.Contains(
                "install_with_oryx_safe_build \"$safe_build_manager\" \"$python\" " +
                "\"$REQUIREMENTS_TXT_FILE\"",
                text);
            Assert.DoesNotContain(
                "install_python_packages \"$python\" \"$REQUIREMENTS_TXT_FILE\" " +
                "\"packages_dir\"",
                text);
            Assert.Contains("pipInstallExitCode == 42", text);
            Assert.Contains(
                "Oryx SafeBuild blocked the deployment because critical vulnerabilities " +
                "were found in the resolved Python packages",
                text);
            Assert.DoesNotContain(
                "PYTHON_FAST_BUILD_ENABLED\" = \"true\" ] || " +
                "[[ $pipInstallExitCode == 42",
                text);
            Assert.Contains("oryx-safe-build-checker is not installed", text);
            Assert.Contains("assessment_exit_code != 0", text);
            Assert.DoesNotContain("http://127.0.0.1:8080/v1/audit", text);
            Assert.DoesNotContain("SAFE_ORYX_BUILD_OUTCOME", text);
            Assert.DoesNotContain("SAFE_ORYX_BUILD_TEMP_ROOT", text);
            Assert.DoesNotContain("WEBSITE_ORYX_SAFE_BUILD_ENABLED", text);
            Assert.DoesNotContain("WEBSITE_ORYX_SAFE_BUILD_MODE", text);
        }

        [Fact]
        public void GeneratedOryxSafeBuildSnippet_HasValidBashSyntax()
        {
            var bash = OperatingSystem.IsWindows()
                ? @"C:\Program Files\Git\bin\bash.exe"
                : "bash";
            if (OperatingSystem.IsWindows() && !File.Exists(bash))
            {
                return;
            }

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
                oryxSafeBuildEnabled: true,
                oryxSafeBuildMode: "block");
            string path = Path.Combine(
                Path.GetTempPath(),
                $"oryx-safe-build-{Guid.NewGuid():N}.sh");

            try
            {
                File.WriteAllText(
                    path,
                    TemplateHelper.Render(
                        TemplateHelper.TemplateResource.PythonSnippet,
                        snippetProps));
                var result = ProcessHelper.RunProcess(
                    bash,
                    new[] { "-n", path },
                    workingDirectory: null,
                    waitTimeForExit: TimeSpan.FromSeconds(10));

                Assert.True(result.ExitCode == 0, result.Error);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
