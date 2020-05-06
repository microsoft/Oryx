// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "python")]
    public class PythonGunicornMultiWorkersTest : PythonEndToEndTestsBase
    {
        public PythonGunicornMultiWorkersTest(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanBuildAndRunPythonApp_UsingGunicornMultipleWorkers()
        {
            // Arrange
            var pythonVersion = "3.7";
            var appName = "http-server-py";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var startupFile = "/tmp/startup.sh";

            var buildScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    ExtVarNames.PythonEnableGunicornMultiWorkersEnvVarName,
                    "true")
                .AddCommand($"oryx build {appDir} --platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
                .ToString();

            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    ExtVarNames.PythonEnableGunicornMultiWorkersEnvVarName,
                    "true")
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(startupFile)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("python", pythonVersion),
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello Gunicorn!", data);
                });
        }
    }
}