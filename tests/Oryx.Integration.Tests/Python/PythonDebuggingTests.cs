// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Integration.Tests.VSCodeDebugProtocol;
using Microsoft.Oryx.Tests.Common;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "python")]
    public class PythonDebuggingTests : PythonEndToEndTestsBase
    {
        public PythonDebuggingTests(ITestOutputHelper output, TestTempDirTestFixture tempDir)
            : base(output, tempDir)
        {
        }

        [Theory]
        // [InlineData("2.7", "ptvsd")]
        // [InlineData("3.6", "ptvsd")]
        [InlineData("3.7", "ptvsd", 5637)]
        public async Task CanBuildAndDebugFlaskApp(string pythonVersion, string debugAdapter, int debugPort = 5678)
        {
            // Arrange
            var appName = "flask-app";
            var appVolume = CreateAppVolume(appName);

            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appVolume.ContainerDir} --platform python --platform-version {pythonVersion} --debug")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appVolume.ContainerDir} -bindPort {ContainerPort}" +
                            $" -debugAdapter {debugAdapter} -debugPort {debugPort} -debugWait")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                appVolume,
                "/bin/bash", new[] { "-c", buildScript },
                $"oryxdevmcr.azurecr.io/public/oryx/python-{pythonVersion}",
                debugPort,
                "/bin/bash", new[] { "-c", runScript },
                async (ptvsdHostPort) =>
                {
                    // Send an Initialize request to make sure the debugger is running
                    using (var debugClient = new SimpleDAPClient("127.0.0.1", ptvsdHostPort, "oryxtests"))
                    {
                        dynamic initRes = await debugClient.Initialize();
                        // Deliberatly weak assertion (don't care what's in the response, only that there IS a response)
                        Assert.Equal("event", initRes.type);
                    }
                });
        }
    }
}