// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public class PythonGunicornMultiWorkersTest : PythonEndToEndTestsBase
    {
        public PythonGunicornMultiWorkersTest(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact(Skip = "work item #1122020")]
        [Trait("category", "python-3.7")]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRunPythonApp_UsingGunicornMultipleWorkersAsync()
        {
            // Arrange
            var pythonVersion = "3.7";
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "http-server-py";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var startupFile = "/tmp/startup.sh";

            var buildScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    ExtVarNames.PythonEnableGunicornMultiWorkersEnvVarName,
                    "true")
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
                .ToString();

            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    ExtVarNames.PythonEnableGunicornMultiWorkersEnvVarName,
                    "true")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(startupFile)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("python", pythonVersion, osType),
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