// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "node-14-skipped")]
    public class NodeNextJsAppTest : NodeEndToEndTestsBase
    {
        public const string AppName = "blog-starter-nextjs";
        public const int ContainerAppPort = 3000;
        public NodeNextJsAppTest(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact(Skip = "next blogger app is broken")]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRun_BlogStarterNextJsApp_WithoutZippingNodeModulesAsync()
        {
            // Arrange
            var nodeVersion = "14";
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var volume = CreateAppVolume(AppName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
               $"--platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"export PORT={ContainerAppPort}")
                .AddCommand($"oryx create-script -appPath {appOutputDir}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                AppName,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                ContainerAppPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Welcome to your new blog built with Next.js", data);
                });
        }

        [Fact(Skip = "next blogger app is broken")]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRun_BlogStarterNextJsApp_UsingZippedNodeModulesAsync()
        {
            // Arrange
            var nodeVersion = "14";
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            string compressFormat = "tar-gz";
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var volume = CreateAppVolume(AppName);
            var appDir = volume.ContainerDir;
            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"export PORT={ContainerAppPort}")
                .AddCommand($"oryx create-script -appPath {appOutputDir}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} " +
                $"--platform-version {nodeVersion} -p compress_node_modules={compressFormat}")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                ContainerAppPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runAppScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Welcome to your new blog built with Next.js", data);
                });
        }
    }
}