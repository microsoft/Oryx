// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Integration.Tests.VSCodeDebugProtocol;
using Microsoft.Oryx.Tests.Common;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "python")]
    public class PythonDebuggingTests : PythonEndToEndTestsBase
    {
        private const int DefaultPtvsdPort = 5678;

        public PythonDebuggingTests(ITestOutputHelper output, TestTempDirTestFixture tempDir)
            : base(output, tempDir)
        {
        }

        [Theory]
        [InlineData("2.7")]
        [InlineData("3.6")]
        [InlineData("3.7", 5637)] // Test with a non-default port as well
        public async Task CanBuildAndDebugFlaskApp(string pythonVersion, int? debugPort = null)
        {
            // Arrange
            var appName = "flask-app";
            var appVolume = CreateAppVolume(appName);
            var scriptGenDebugPortArg = debugPort.HasValue ? $"-debugPort {debugPort.Value}" : string.Empty;

            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appVolume.ContainerDir} --platform python --platform-version {pythonVersion} --debug")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appVolume.ContainerDir} -bindPort {ContainerPort}" +
                            $" -debugAdapter ptvsd {scriptGenDebugPortArg} -debugWait")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                appVolume,
                "/bin/bash", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("python", pythonVersion),
                debugPort.GetValueOrDefault(DefaultPtvsdPort),
                "/bin/bash", new[] { "-c", runScript },
                async (ptvsdHostPort) =>
                {
                    // Send an Initialize request to make sure the debugger is running
                    using (var debugClient = new SimpleDAPClient("127.0.0.1", ptvsdHostPort, "oryxtests"))
                    {
                        string initResponse = await debugClient.Initialize();
                        // Deliberatly weak assertion; don't care what's in the response, only that there IS a response
                        Assert.False(string.IsNullOrEmpty(initResponse));
                    }
                });
        }
    }
}