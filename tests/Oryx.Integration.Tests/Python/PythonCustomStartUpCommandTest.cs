// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public class PythonCustomStartUpCommandTest : PythonEndToEndTestsBase
    {
        public PythonCustomStartUpCommandTest(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Theory]
        [Trait("category", "python-3.8")]
        [Trait("build-image", "debian-stretch")]
        [InlineData("3.8", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("3.8", ImageTestHelperConstants.OsTypeDebianBullseye)]
        public async Task CanBuildAndRunPythonApp_UsingCustomStartUpScriptAsync(string pythonVersion, string osType)
        {
            // Arrange
            var appName = "http-server-py";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var startupFile = "/tmp/startup.sh";

            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
                .ToString();

            // Using the custom startup script within sample app
            const string customStartUpScript = "customStartup.sh";
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort} " +
                $"-userStartupCommand {appOutputDir}/{customStartUpScript} -output {startupFile}")
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

        [Theory]
        [Trait("category", "python-3.8")]
        [Trait("build-image", "debian-stretch")]
        [InlineData("3.8", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("3.8", ImageTestHelperConstants.OsTypeDebianBullseye)]
        public async Task CanBuildAndRunPythonApp_UsingCustomStartUpCommandAsync(string pythonVersion, string osType)
        {
            // Arrange
            var appName = "http-server-py";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var startupFile = "/tmp/startup.sh";

            // Create a custom startup command
            const string customStartUpCommand = "'gunicorn -w 4 myapp:app'";
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -output {startupFile} " +
                $"-userStartupCommand {customStartUpCommand} -bindPort {ContainerPort}")
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