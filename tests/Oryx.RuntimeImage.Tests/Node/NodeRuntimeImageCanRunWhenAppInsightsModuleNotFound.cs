// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    public class NodeRuntimeImageCanRunWhenAppInsightsModuleNotFound : NodeRuntimeImageTestBase
    {
        public NodeRuntimeImageCanRunWhenAppInsightsModuleNotFound(
            ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Theory]
        [MemberData(
           nameof(TestValueGenerator.GetNodeVersions),
           MemberType = typeof(TestValueGenerator))]
        public async Task GeneratesScript_CanRun_AppInsightsModule_NotFound(string nodeVersion)
        {
            // This test is for the following scenario:
            // When we find no application insight dependency in package.json, but env variables  for
            // configuring application insights has been set in portal

            // Arrange
            var appName = "linxnodeexpress";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var imageName = _imageHelper.GetRuntimeImage("node", nodeVersion);
            var aIKey = ExtVarNames.UserAppInsightsKeyEnv;
            var aIEnabled = ExtVarNames.UserAppInsightsEnableEnv;
            int containerDebugPort = 8080;

            var script = new ShellScriptBuilder()
                .AddCommand($"export {aIKey}=asdas")
                .AddCommand($"export {aIEnabled}=TRUE")
                .AddCommand($"cd {appDir}")
                .AddCommand("npm install")
                .AddCommand($"oryx -appPath {appDir}")
                .AddDirectoryExistsCheck($"{appDir}/node_modules")
                .AddDirectoryDoesNotExistCheck($"{appDir}/node_modules/applicationinsights")
                .AddCommand("./run.sh")
                .AddFileExistsCheck($"{appDir}/oryx-appinsightsloader.js")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                port: containerDebugPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", script },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World from express!", data);
                },
                dockerCli: _dockerCli);
        }

        [Theory]
        [MemberData(
           nameof(TestValueGenerator.GetNodeVersions),
           MemberType = typeof(TestValueGenerator))]
        public async Task GeneratesScript_CanRun_AppInsights_NotConfigured(string nodeVersion)
        {
            // This test is for the following scenario:
            // When we find no application insight dependency in package.json and env variables for
            // configuring application insights has not been set properly in portal

            // Arrange
            var appName = "linxnodeexpress";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var imageName = _imageHelper.GetRuntimeImage("node", nodeVersion);
            var aIEnabled = ExtVarNames.UserAppInsightsEnableEnv;
            int containerDebugPort = 8080;

            var script = new ShellScriptBuilder()
                .AddCommand($"export {aIEnabled}=disabled")
                .AddCommand($"cd {appDir}")
                .AddCommand("npm install")
                .AddCommand($"oryx -appPath {appDir}")
                .AddDirectoryExistsCheck($"{appDir}/node_modules")
                .AddDirectoryDoesNotExistCheck($"{appDir}/node_modules/applicationinsights")
                .AddCommand("./run.sh")
                .AddFileDoesNotExistCheck($"{appDir}/oryx-appinsightsloader.js")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                port: containerDebugPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", script },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World from express!", data);
                },
                dockerCli: _dockerCli);
        }
    }
}
