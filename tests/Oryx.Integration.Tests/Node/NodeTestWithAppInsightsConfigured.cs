// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "node")]
    public class NodeTestWithAppInsightsConfigured : NodeEndToEndTestsBase
    {
        public NodeTestWithAppInsightsConfigured(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Theory]
        [MemberData(nameof(TestValueGenerator.GetNodeVersions_SupportDebugging),
            MemberType = typeof(TestValueGenerator))]
        public async Task CanBuildAndRun_NodeApp_WithAppInsights_Configured(string nodeVersion)
        {
            // Arrange
            var appName = "linxnodeexpress";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var spcifyNodeVersionCommand = "--platform nodejs --language-version=" + nodeVersion;
            var aIKey = "APPINSIGHTS_INSTRUMENTATIONKEY";
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -o {appDir} {spcifyNodeVersionCommand} --log-file {appDir}/1.log")
                .AddDirectoryExistsCheck($"{appDir}/node_modules")
                .AddFileExistsCheck($"{appDir}/oryx-appinsightsloader.js")
                .AddFileExistsCheck($"{appDir}/{FilePaths.BuildManifestFileName}")
                .AddStringExistsInFileCheck(
                $"{NodeConstants.InjectedAppInsights}=\"True\"", $"{appDir}/{FilePaths.BuildManifestFileName}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"export {aIKey}=asdas")
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .AddFileExistsCheck($"{appDir}/oryx-appinsightsloader.js")
                .AddFileExistsCheck($"{appDir}/{FilePaths.BuildManifestFileName}")
                .AddStringExistsInFileCheck(
                $"{NodeConstants.InjectedAppInsights}=\"True\"",
                $"{appDir}/{FilePaths.BuildManifestFileName}")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { volume },
                Settings.BuildImageName,
                "/bin/bash",
                 new[]
                {
                    "-c",
                    buildScript
                },
                $"oryxdevmcr.azurecr.io/public/oryx/node-{nodeVersion}",
                new List<EnvironmentVariable> { new EnvironmentVariable(aIKey, "asdasda") },
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World from express!", data);
                });
        }
    }
}