// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Node;
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
           nameof(TestValueGenerator.GetNodeVersions_SupportDebugging),
           MemberType = typeof(TestValueGenerator))]
        public async Task GeneratesScript_CanRun_AppInsightsModule_NotFound(string nodeVersion)
        {
            // This test is for the following scenario: 
            // When we find injectedAppInsight=True in the manifest file, we assume that appinsights
            // has been injected and it's installed during build (npm install). But for some reason if we 
            // don't see the appinsights node_module we shouldn't break the app. We should run the app 
            // and additionally print the exception message

            // Arrange
            var appName = "linxnodeexpress";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var imageName = string.Concat("oryxdevmcr.azurecr.io/public/oryx/node-", nodeVersion);
            var manifestFileContent = $"'{NodeConstants.InjectedAppInsights}=\"True\"'";
            var aiNodesdkLoaderContent = @"try {
                var appInsights = require('applicationinsights');  
                if (process.env.APPINSIGHTS_INSTRUMENTATIONKEY)
                { 
                    appInsights.setup().start();
                } 
                }catch (e) { 
                    console.log(e); 
                } ";

            int containerDebugPort = 8080;

            var script = new ShellScriptBuilder()
                .CreateFile($"{appDir}/{FilePaths.BuildManifestFileName}", manifestFileContent)
                .CreateFile($"{appDir}/oryx-appinsightsloader.js", $"\"{aiNodesdkLoaderContent}\"")
                .AddCommand($"cd {appDir}")
                .AddCommand("npm install")
                .AddCommand($"oryx -appPath {appDir}")
                .AddDirectoryExistsCheck($"{appDir}/node_modules")
                .AddDirectoryDoesNotExistCheck($"{appDir}/node_modules/applicationinsights")
                .AddCommand("./run.sh")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: $"oryxdevmcr.azurecr.io/public/oryx/node-{nodeVersion}",
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
