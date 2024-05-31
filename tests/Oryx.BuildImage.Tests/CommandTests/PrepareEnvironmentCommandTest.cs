// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildImage.Tests;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using BuildScriptGeneratorCli = Microsoft.Oryx.BuildScriptGeneratorCli;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.BuildImage.Tests
{
    public class PrepareEnvironmentCommandTest : SampleAppsTestBase
    {
        private DockerVolume CreateWebFrontEndVolume() => DockerVolume.CreateMirror(
            Path.Combine(_hostSamplesDir, "nodejs", "webfrontend"));

        public PrepareEnvironmentCommandTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact, Trait("category", "githubactions")]
        public void DetectsAndInstallsPlatformIfNotPresent()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddCommand($"oryx prep -s {appDir}")
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
                    Assert.Contains(
                        $"Downloading and extracting '{NodeConstants.PlatformName}' version",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void DetectsAndInstallsPlatformVersion_SpecifiedByEnvironmentVariable()
        {
            // Arrange
            var nodeVersion = "12.19.0";
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var subDir = Guid.NewGuid();
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable("NODE_VERSION", nodeVersion)
                .AddCommand($"oryx prep -s {appDir}")
                .AddDirectoryExistsCheck($"{Constants.TemporaryInstallationDirectoryRoot}/nodejs/{nodeVersion}")
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
                    Assert.Contains(
                        $"Downloading and extracting '{NodeConstants.PlatformName}' version",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void DetectsAndInstallsPlatformVersion_SpecifiedByBuildEnvFile()
        {
            // Arrange
            var nodeVersion = "12.19.0";
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddCommand($"echo 'NODE_VERSION=\"{nodeVersion}\"' > {appDir}/build.env")
                .AddCommand($"oryx prep -s {appDir}")
                .AddDirectoryExistsCheck(
                $"{Constants.TemporaryInstallationDirectoryRoot}/{NodeConstants.PlatformName}/{nodeVersion}")
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
                    Assert.Contains(
                        $"Downloading and extracting '{NodeConstants.PlatformName}' version",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void SkipsDetectionAndInstallsPlatform()
        {
            // Arrange
            var nodeVersion = "12.19.0";
            var pythonVersion = "3.10.0";
            var script = new ShellScriptBuilder()
                .AddCommand(
                $"oryx prep --skip-detection --platforms-and-versions " +
                $"'{NodeConstants.PlatformName}={nodeVersion}, {PythonConstants.PlatformName}={pythonVersion}'")
                .AddDirectoryExistsCheck(
                $"{Constants.TemporaryInstallationDirectoryRoot}/{NodeConstants.PlatformName}/{nodeVersion}")
                .AddDirectoryExistsCheck(
                $"{Constants.TemporaryInstallationDirectoryRoot}/{PythonConstants.PlatformName}/{pythonVersion}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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
        public void SkipsDetectionAndInstallsPlatformsFromFile()
        {
            // Arrange
            var nodeVersion = "16.5.0";
            var pythonVersion = "3.10.0";
            var versionsFile = "/tmp/versions.txt";
            var script = new ShellScriptBuilder()
                .AddCommand($"echo 'nodejs={nodeVersion}' >> {versionsFile}")
                .AddCommand($"echo >> {versionsFile}")
                .AddCommand($"echo '#A comment' >> {versionsFile}")
                .AddCommand($"echo 'python={pythonVersion}' >> {versionsFile}")
                .AddCommand($"oryx prep --skip-detection --platforms-and-versions-file {versionsFile}")
                .AddDirectoryExistsCheck(
                $"{Constants.TemporaryInstallationDirectoryRoot}/{NodeConstants.PlatformName}/{nodeVersion}")
                .AddDirectoryExistsCheck(
                $"{Constants.TemporaryInstallationDirectoryRoot}/{PythonConstants.PlatformName}/{pythonVersion}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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
        public void InstallsPlatformAtCustomInstallationRootDirectory()
        {
            // Arrange
            var nodeVersion = "16.5.0";
            var customDynamicInstallRootDir = "/foo/bar";
            var expectedText =
                $"Node path is: {customDynamicInstallRootDir}/{NodeConstants.PlatformName}/{nodeVersion}/bin";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable("DYNAMIC_INSTALL_ROOT_DIR", customDynamicInstallRootDir)
                .AddCommand(
                $"oryx prep --skip-detection --platforms-and-versions " +
                $"'{NodeConstants.PlatformName}={nodeVersion}'")
                .AddDirectoryExistsCheck(
                $"{customDynamicInstallRootDir}/{NodeConstants.PlatformName}/{nodeVersion}")
                .AddCommand($"source benv node={nodeVersion}")
                .AddCommand("nodePath=$(which node)")
                .AddCommand("echo \"Node path is: $nodePath\"")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Fact, Trait("category", "latest")]
        public void InstallsPlatformsAtBuiltInInstallDirAsRoot()
        {
            // Arrange
            var nodeVersion = "14.0.0";
            var customDynamicInstallRootDir = "/foo/bar";
            var expectedText =
                $"Node path is: {customDynamicInstallRootDir}/{NodeConstants.PlatformName}/{nodeVersion}/bin";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(BuildScriptGeneratorCli.SettingsKeys.EnableDynamicInstall, true.ToString())
                .SetEnvironmentVariable("DYNAMIC_INSTALL_ROOT_DIR", customDynamicInstallRootDir)
                .AddCommand(
                $"oryx prep --skip-detection --platforms-and-versions " +
                $"'{NodeConstants.PlatformName}={nodeVersion}'")
                .AddDirectoryExistsCheck(
                $"{customDynamicInstallRootDir}/{NodeConstants.PlatformName}/{nodeVersion}")
                .AddCommand($"source benv node={nodeVersion}")
                .AddCommand("nodePath=$(which node)")
                .AddCommand("echo \"Node path is: $nodePath\"")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetBuildImage(),
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
    }
}
