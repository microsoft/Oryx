// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "python")]
    public class Python27EndToEndTests : PythonEndToEndTestsBase
    {
        public Python27EndToEndTests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanBuildAndRunPythonApp_UsingPython27_AndExplicitOutputStartupFile()
        {
            // Arrange
            var appName = "python2-flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var startupFile = "/tmp/startup.sh";
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} --platform python --language-version 2.7")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -output {startupFile} -bindPort {ContainerPort}")
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
                _imageHelper.GetRuntimeImage("python", "2.7"),
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
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_Python27App_UsingVirtualEnv()
        {
            // Arrange
            var appName = "python2-flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            const string virtualEnvName = "antenv2.7";
            var buildScript = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} --platform python --language-version 2.7 -p virtualenv_name={virtualEnvName}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                // Mimic the commands ran by app service in their derived image.
                .AddCommand("pip install gunicorn")
                .AddCommand("pip install flask")
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
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
                _imageHelper.GetRuntimeImage("python", "2.7"),
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
                    Assert.Contains("Hello World!", data);
                });
        }
    }
}