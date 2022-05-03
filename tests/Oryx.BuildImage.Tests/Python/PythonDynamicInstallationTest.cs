// --------------------------------------------------------------------------------------------
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

        public static TheoryData<string, string> ImageNameData
        {
            get
            {
                var imageTestHelper = new ImageTestHelper();
                var data = new TheoryData<string, string>();
                data.Add(imageTestHelper.GetLtsVersionsBuildImage(), "3.8.1");
                data.Add(imageTestHelper.GetLtsVersionsBuildImage(), "3.8.3");
                data.Add(imageTestHelper.GetGitHubActionsBuildImage(), "3.8.1");
                data.Add(imageTestHelper.GetGitHubActionsBuildImage(), "3.8.3");
                data.Add(imageTestHelper.GetGitHubActionsBuildImage("github-actions-buster"), "3.9.0");
                return data;
            }
        }

        [Theory]
        [MemberData(nameof(ImageNameData))]
        public void GeneratesScript_AndBuildsPython(string imageName, string version)
        {
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

        [Theory]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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
