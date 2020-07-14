// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class NodeDynamicInstallationTest : NodeJSSampleAppsTestBase
    {
        private readonly string DefaultInstallationRootDir = "/opt/nodejs";

        public NodeDynamicInstallationTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void DynamicallyInstallsNodeRuntimeAndBuilds()
        {
            // Arrange
            // Here 'nodemon' and 'express' are packages specified in package.json
            var devPackageName = "nodemon";
            var prodPackageName = "express";
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .SetEnvironmentVariable(SettingsKeys.EnableDynamicInstall, true.ToString())
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{devPackageName}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{prodPackageName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetLtsVersionsBuildImage(),
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
        public void DynamicInstall_ReInstallsSdk_IfSentinelFileIsNotPresent()
        {
            // Arrange
            var version = "12.16.1"; //NOTE: use the full version so that we know the install directory path
            var installationDir = $"{BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot}/nodejs/{version}";
            var sentinelFile = $"{installationDir}/{SdkStorageConstants.SdkDownloadSentinelFileName}";
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var buildCmd = $"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {NodeConstants.PlatformName} --platform-version {version}";
            var script = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddBuildCommand(buildCmd)
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddFileExistsCheck(sentinelFile)
                .AddCommand($"rm -f {sentinelFile}")
                .AddBuildCommand(buildCmd)
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddFileExistsCheck(sentinelFile)
                .AddBuildCommand(buildCmd)
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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
        public void BuildsApplication_ByDynamicallyInstalling_IntoCustomDynamicInstallationDir()
        {
            // Arrange
            var version = "14.0.0"; //NOTE: use the full version so that we know the install directory path
            var expectedDynamicInstallRootDir = "/foo/bar";
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var buildCmd = $"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {NodeConstants.PlatformName} --platform-version {version} " +
                $"--dynamic-install-root-dir {expectedDynamicInstallRootDir}";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(SettingsKeys.EnableDynamicInstall, true.ToString())
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddBuildCommand(buildCmd)
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddDirectoryExistsCheck(
                $"{expectedDynamicInstallRootDir}/{NodeConstants.PlatformName}/{version}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetBuildImage(),
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

        private string GetSnippetToCleanUpExistingInstallation()
        {
            return $"rm -rf {DefaultInstallationRootDir}; mkdir -p {DefaultInstallationRootDir}";
        }
    }
}