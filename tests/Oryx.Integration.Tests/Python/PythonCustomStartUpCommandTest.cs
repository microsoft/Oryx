// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "python")]
    public class PythonCustomStartUpCommandTest : PythonEndToEndTestsBase
    {
        public PythonCustomStartUpCommandTest(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Theory]
        [InlineData("2.7")]
        [InlineData("3.6")]
        [InlineData("3.7")]
        public async Task CanBuildAndRunPythonApp_UsingCustomStartUpScript(string pythonVersion)
        {
            // Arrange
            var appName = "http-server-py";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;

            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} --platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
                .ToString();

            // Create a custom startup script
            const string customStartUpScript = "customStartup.sh";
            File.WriteAllText(Path.Join(volume.MountedHostDir, customStartUpScript),
                "#!/bin/bash\n" +
                "pip install gunicorn\n" + 
                $"gunicorn -w 4 myapp:app\n");
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort} -userStartupCommand {customStartUpScript} -output {customStartUpScript}")
                .AddCommand($"./{customStartUpScript}")
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

        [Theory]
        [InlineData("2.7")]
        [InlineData("3.6")]
        [InlineData("3.7")]
        public async Task CanBuildAndRunPythonApp_UsingCustomStartUpCommand(string pythonVersion)
        {
            // Arrange
            var appName = "http-server-py";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var startupFile = "/tmp/startup.sh";

            // Create a custom startup command
            const string customStartUpCommand = "'gunicorn -w 4 myapp:app'";
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} --platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appDir} -output {startupFile} -userStartupCommand {customStartUpCommand} -bindPort {ContainerPort}")
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