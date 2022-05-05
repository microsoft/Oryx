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
    [Trait("category", "golang")]
    public class GolangBuildRunTests : GolangEndToEndTestsBase
    {
        public GolangBuildRunTests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanRunApp_WithoutBuildManifestFile()
        {
            // Arrange
            var golangVersion = "1.17";
            var hostDir = Path.Combine(_hostSamplesDir, "golang", GolangHelloWorldWebApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " + 
                $"--platform {GolangConstants.PlatformName} --platform-version {golangVersion}")
                .AddCommand(
                $"oryx run-script {appOutputDir} --output {DefaultStartupFilePath} --debug")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            // Assert
            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: "oryxdevmcr.azurecr.io/public/oryx/build-and-run-buster:latest",
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