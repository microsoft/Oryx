﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class PythonDynamicInstallationTest : PythonSampleAppsTestBase
    {
        private readonly string DefaultInstallationRootDir = "/opt/python";

        public PythonDynamicInstallationTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact, Trait("category", "ltsversions")]
        public void PipelineTestInvocationLtsVersions()
        {
            var imageTestHelper = new ImageTestHelper();
            GeneratesScript_AndBuildsPython(imageTestHelper.GetLtsVersionsBuildImage(), "3.8.1");
            GeneratesScript_AndBuildsPython(imageTestHelper.GetLtsVersionsBuildImage(), "3.8.3");
        }

        [Fact, Trait("category", "githubactions")]
        public void PipelineTestInvocationGithubActions()
        {
            var imageTestHelper = new ImageTestHelper();
            GeneratesScript_AndBuildsPython(imageTestHelper.GetGitHubActionsBuildImage(), "3.8.1");
            GeneratesScript_AndBuildsPython(imageTestHelper.GetGitHubActionsBuildImage(), "3.8.3");
            GeneratesScript_AndBuildsPython(imageTestHelper.GetGitHubActionsBuildImage("github-actions-buster"), "3.9.0");
            GeneratesScript_AndBuildsPython(imageTestHelper.GetGitHubActionsBuildImage("github-actions-bullseye"), "3.10.4");      
        }

        private void GeneratesScript_AndBuildsPython(string imageName, string version)
        {
            // Please note:
            // This test method has at least 1 wrapper function that pases the imageName parameter.

            // Arrange
            var installationDir = $"{BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot}/" +
                $"python/{version}";
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddBuildCommand(
                $"{appDir} --platform {PythonConstants.PlatformName} --platform-version {version} -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = imageName,
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        $"Python Version: {installationDir}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory, Trait("category", "jamstack")]
        [InlineData("3.10.4")]
        public void GeneratesScript_AndBuildsPython_JamstackBuildImage(string version)
        {
            // Arrange
            var installationDir = "/opt/" + $"python/{version}";
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddBuildCommand(
                $"{appDir} --platform {PythonConstants.PlatformName} --platform-version {version} -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetAzureFunctionsJamStackBuildImage("azfunc-jamstack-bullseye"),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        $"Python Version: {installationDir}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory, Trait("category", "githubactions")]
        [InlineData("3.8.0b3")]
        [InlineData("3.9.0b1")]
        public void GeneratesScript_AndBuildsPythonPreviewVersion(string previewVersion)
        {
            // Arrange
            var installationDir = $"{BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot}/" +
                $"python/{previewVersion}";
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddBuildCommand(
                $"{appDir} --platform {PythonConstants.PlatformName} --platform-version {previewVersion} " +
                $"-o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _restrictedPermissionsImageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        $"Python Version: {installationDir}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void DynamicInstall_ReInstallsSdk_IfSentinelFileIsNotPresent()
        {
            // Arrange
            var version = "3.8.1"; //NOTE: use the full version so that we know the install directory path
            var installationDir = $"{BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot}/" +
                $"python/{version}";
            var sentinelFile = $"{installationDir}/{SdkStorageConstants.SdkDownloadSentinelFileName}";
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var buildCmd = $"{appDir} --platform {PythonConstants.PlatformName} --platform-version {version} " +
                $"-o {appOutputDir}";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddBuildCommand(buildCmd)
                .AddFileExistsCheck(sentinelFile)
                .AddCommand($"rm -f {sentinelFile}")
                .AddBuildCommand(buildCmd)
                .AddFileExistsCheck(sentinelFile)
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void BuildsAzureFunctionApp()
        {
            // Arrange
            var version = "3.8.1";
            var installationDir = $"{BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot}/" +
                $"python/{version}";
            var appName = "Python_HttpTriggerSample";
            var volume = DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "azureFunctionsApps", appName));
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddDefaultTestEnvironmentVariables()
                .AddBuildCommand(
                $"{appDir} --platform {PythonConstants.PlatformName} --platform-version {version} -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        $"Python Version: {installationDir}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "latest")]
        public void BuildsApplication_ByDynamicallyInstalling_IntoCustomDynamicInstallationDir()
        {
            // Arrange
            var version = "3.6.9";
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var expectedDynamicInstallRootDir = "/foo/bar";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddBuildCommand(
                $"{appDir} --platform {PythonConstants.PlatformName} --platform-version {version} -o {appOutputDir}" +
                $" --dynamic-install-root-dir {expectedDynamicInstallRootDir}")
                .AddDirectoryExistsCheck(
                $"{expectedDynamicInstallRootDir}/{PythonConstants.PlatformName}/{version}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        $"Python Version: {expectedDynamicInstallRootDir}/{PythonConstants.PlatformName}" +
                        $"/{version}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void GeneratesScript_AndBuilds_WithPackageDir()
        {
            // Arrange
            var version = "3.6.9";
            var appName = "flask-app";
            var installationDir = $"{BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot}" +
                $"/python/{version}";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var packagesDir = ".python_packages/lib/python3.7/site-packages";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --platform {PythonConstants.PlatformName} " +
                $"--platform-version {version} -p packagedir={packagesDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{packagesDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        $"Python Version: {installationDir}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        private string GetSnippetToCleanUpExistingInstallation()
        {
            return $"rm -rf {DefaultInstallationRootDir}; mkdir -p {DefaultInstallationRootDir}";
        }
    }
}
