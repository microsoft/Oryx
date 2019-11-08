// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "node")]
    public class NodeNuxtJsAppTest : NodeEndToEndTestsBase
    {
        public const string AppName = "helloworld-nuxtjs";
        public const int ContainerAppPort = 3000;

        public NodeNuxtJsAppTest(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanBuildAndRun_HackerNewsNuxtJsApp_WithoutZippingNodeModules()
        {
            // Arrange
            var nodeVersion = "10";
            var volume = CreateAppVolume(AppName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform nodejs --platform-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                // Note: NuxtJS needs the host to be specified this way
                .SetEnvironmentVariable("HOST", "0.0.0.0")
                .SetEnvironmentVariable("PORT", ContainerAppPort.ToString())
                .AddCommand($"oryx -appPath {appDir}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                AppName,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("node", nodeVersion),
                ContainerAppPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}");
                    Assert.Contains("Welcome!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_HackerNewsNuxtJsApp_UsingZippedNodeModules()
        {
            var nodeVersion = "10";
            string compressFormat = "zip";
            // NOTE:
            // 1. Use intermediate directory(which here is local to container) to avoid errors like
            //      "tar: node_modules/form-data: file changed as we read it"
            //    related to zipping files on a folder which is volume mounted.
            // 2. Use output directory within the container due to 'rsync'
            //    having issues with volume mounted directories

            // Arrange
            var appOutputDirPath = Directory.CreateDirectory(Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N")))
                .FullName;
            var appOutputDirVolume = DockerVolume.CreateMirror(appOutputDirPath);
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var volume = CreateAppVolume(AppName);
            var appDir = volume.ContainerDir;
            var runAppScript = new ShellScriptBuilder()
                // Note: NuxtJS needs the host to be specified this way
                .SetEnvironmentVariable("HOST", "0.0.0.0")
                .SetEnvironmentVariable("PORT", ContainerAppPort.ToString())
                .AddCommand($"oryx -appPath {appOutputDir}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o /tmp/out --platform nodejs " +
                $"--platform-version {nodeVersion} -p compress_node_modules={compressFormat}")
                .AddCommand($"cp -rf /tmp/out/. {appOutputDir}")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                AppName,
                _output,
                new List<DockerVolume> { appOutputDirVolume, volume },
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("node", nodeVersion),
                ContainerAppPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runAppScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}");
                    Assert.Contains("Welcome!", data);
                });
        }
    }
}