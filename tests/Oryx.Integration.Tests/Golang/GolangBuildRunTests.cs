// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Golang;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public class GolangBuildRunTests : GolangEndToEndTestsBase
    {
        public GolangBuildRunTests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        [Trait("category", "golang-1.17")]
        [Trait("build-image", "full-debian-buster")]
        public async Task RunGolang117BuildRunTestsAsync_WithFullBuster()
        {
            await CanRunApp_WithoutBuildManifestFileAsync("1.17", ImageTestHelperConstants.FullBuster);
        }

        [Fact]
        [Trait("category", "golang-1.17")]
        [Trait("build-image", "full-debian-bullseye")]
        public async Task RunGolang117BuildRunTestsAsync_WithFullBullseye()
        {
            await CanRunApp_WithoutBuildManifestFileAsync("1.17", ImageTestHelperConstants.FullBullseye);
        }

        [Fact]
        [Trait("category", "golang-1.18")]
        [Trait("build-image", "full-debian-buster")]
        public async Task RunGolang118BuildRunTestsAsync_WithFullBuster()
        {
            await CanRunApp_WithoutBuildManifestFileAsync("1.18", ImageTestHelperConstants.FullBuster);
        }

        [Fact]
        [Trait("category", "golang-1.18")]
        [Trait("build-image", "full-debian-bullseye")]
        public async Task RunGolang118BuildRunTestsAsync_WithFullBullseye()
        {
            await CanRunApp_WithoutBuildManifestFileAsync("1.18", ImageTestHelperConstants.FullBullseye);
        }

        [Fact]
        [Trait("category", "golang-1.19")]
        [Trait("build-image", "full-debian-buster")]
        public async Task RunGolang119BuildRunTestsAsync_WithFullBuster()
        {
            await CanRunApp_WithoutBuildManifestFileAsync("1.19", ImageTestHelperConstants.FullBuster);
        }

        [Fact]
        [Trait("category", "golang-1.19")]
        [Trait("build-image", "full-debian-bullseye")]
        public async Task RunGolang119BuildRunTestsAsync_WithFullBullseye()
        {
            await CanRunApp_WithoutBuildManifestFileAsync("1.19", ImageTestHelperConstants.FullBullseye);
        }

        private async Task CanRunApp_WithoutBuildManifestFileAsync(string golangVersion, string imageTag)
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "golang", GolangHelloWorldWebApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var imageTestHelper = new ImageTestHelper();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " + 
                $"--platform {GolangConstants.PlatformName} --platform-version {golangVersion}")
                .AddCommand(
                $"oryx run-script --platform {GolangConstants.PlatformName} --platform-version {golangVersion} {appOutputDir}  --output {DefaultStartupFilePath} --debug")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            // Assert
            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: imageTestHelper.GetBuildImage(imageTag),
                output: _output,
                volumes: new List<DockerVolume> { appOutputDirVolume, volume },
                environmentVariables: null,
                port: ContainerPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", runtimeImageScript },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!!!", data);
                },
                dockerCli: new DockerCli());
        }
    }
}