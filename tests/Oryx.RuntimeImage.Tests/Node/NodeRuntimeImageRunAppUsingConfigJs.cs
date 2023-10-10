// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    public class NodeRuntimeImageRunAppUsingConfigJs : NodeRuntimeImageTestBase
    {
        public NodeRuntimeImageRunAppUsingConfigJs(
            ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Theory]
        [Trait("category", "runtime-buster")]
        [MemberData(nameof(TestValueGenerator.GetBusterNodeVersions_SupportPm2), MemberType = typeof(TestValueGenerator))]
        public async Task RunBusterNodeAppUsingConfigJsAsync(string nodeVersion, string osType)
        {

            var appName = "express-config-js";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var dir = volume.ContainerDir;
            int containerPort = 80;

            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"cd {dir}/app")
                .AddCommand("npm install")
                .AddCommand("cd ..")
                .AddCommand($"oryx create-script -bindPort {containerPort} -usePM2")
                .AddCommand("./run.sh")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                containerPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", runAppScript },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Equal("Hello World from express!", data);
                },
                dockerCli: _dockerCli);

        }

        [Theory]
        [Trait("category", "runtime-bullseye")]
        [MemberData(nameof(TestValueGenerator.GetBullseyeNodeVersions_SupportPm2), MemberType = typeof(TestValueGenerator))]
        public async Task RunBullseyeNodeAppUsingConfigJsAsync(string nodeVersion, string osType)
        {

            var appName = "express-config-js";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var dir = volume.ContainerDir;
            int containerPort = 80;

            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"cd {dir}/app")
                .AddCommand("npm install")
                .AddCommand("cd ..")
                .AddCommand($"oryx create-script -bindPort {containerPort} -usePM2")
                .AddCommand("./run.sh")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                containerPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", runAppScript },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Equal("Hello World from express!", data);
                },
                dockerCli: _dockerCli);

        }
    }
}
