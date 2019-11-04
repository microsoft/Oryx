// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "node")]
    public class NodeTestBuildAndRunAppWithDebugger : NodeEndToEndTestsBase
    {
        public NodeTestBuildAndRunAppWithDebugger(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Theory]
        [MemberData(
           nameof(TestValueGenerator.GetNodeVersions_SupportDebugging),
           MemberType = typeof(TestValueGenerator))]
        public async Task CanBuildAndRunNodeApp_WithDebugger(string nodeVersion)
        {
            // Arrange
            var appOutputDirPath = Directory.CreateDirectory(Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N")))
                .FullName;
            var appOutputDirVolume = DockerVolume.CreateMirror(appOutputDirPath);
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appName = "linxnodeexpress";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appOutputDir} -remoteDebug -debugPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o /tmp/out --platform nodejs " +
                $"--language-version {nodeVersion} -p compress_node_modules=tar-gz")
                .AddCommand($"cp -rf /tmp/out/* {appOutputDir}")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { appOutputDirVolume, volume },
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("node", nodeVersion),
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runAppScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/json/list");
                    Assert.Contains("devtoolsFrontendUrl", data);
                });
        }
    }
}