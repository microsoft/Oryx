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
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.BuildImage.Tests
{
    public class ExecSetupCommandTest : SampleAppsTestBase
    {
        private DockerVolume CreateWebFrontEndVolume() => DockerVolume.CreateMirror(
            Path.Combine(_hostSamplesDir, "nodejs", "webfrontend"));

        public ExecSetupCommandTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void ExecutesCommand_AfterInstallingPlatformIfNotPresentAlready()
        {
            // Arrange
            var nodeVersion = "17.6.0";
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var subDir = Guid.NewGuid();
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable("NODE_VERSION", nodeVersion)
                .AddCommand($"oryx exec -s {appDir} '$node --version'")
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
                    Assert.Contains(
                        $"v{nodeVersion}",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void ExecutesCommand_AfterInstallingPlatformVersionSpecifiedByBuildEnvFile()
        {
            // Arrange
            var nodeVersion = "17.6.0";
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var subDir = Guid.NewGuid();
            var script = new ShellScriptBuilder()
                .AddCommand($"echo 'NODE_VERSION=\"{nodeVersion}\"' > {appDir}/build.env")
                .AddCommand($"oryx exec -s {appDir} '$node --version'")
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
    }
}
