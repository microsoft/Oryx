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
    [Trait("category", "node-16-nuxt")]
    public class NodeNuxtJsAppTest : NodeEndToEndTestsBase
    {
        public const string AppName = "hackernews-nuxtjs";
        public const int ContainerAppPort = 3000;

        public NodeNuxtJsAppTest(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRun_HackerNewsNuxtJsApp_WithoutZippingNodeModulesAsync()
        {
            // Arrange
            var nodeVersion = "16";
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
                // Note: NuxtJS needs the host to be specified this way
                .SetEnvironmentVariable("HOST", "0.0.0.0")
                .SetEnvironmentVariable("PORT", ContainerAppPort.ToString())
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
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}");
                    Assert.Contains("Nuxt HN | News", data);
                });
        }

        [Fact]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRun_HackerNewsNuxtJsApp_UsingZippedNodeModulesAsync()
        {
            // Arrange
            var nodeVersion = "16";
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            string compressFormat = "zip";
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var volume = CreateAppVolume(AppName);
            var appDir = volume.ContainerDir;
            var runAppScript = new ShellScriptBuilder()
                // Note: NuxtJS needs the host to be specified this way
                .SetEnvironmentVariable("HOST", "0.0.0.0")
                .SetEnvironmentVariable("PORT", ContainerAppPort.ToString())
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
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}");
                    Assert.Contains("Nuxt HN | News", data);
                });
        }
    }
}