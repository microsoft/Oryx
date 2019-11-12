// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "python")]
    public class PythonBackwardCompatibilityTests : PythonEndToEndTestsBase
    {
        public PythonBackwardCompatibilityTests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanRunPythonApp_UsingEarlierBuiltPackagesDirectory()
        {
            // This is AppService's scenario where previously built apps can still run
            // fine.

            // Arrange
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var virtualEnvName = "antenv";
            var buildScript = new ShellScriptBuilder()
                // App should run fine even with manifest file not present
                .AddCommand($"oryx build {appDir} -p packagedir={PythonConstants.DefaultTargetPackageDirectory}")
                .AddCommand($"rm -f {appDir}/{FilePaths.BuildManifestFileName}")
                .AddFileDoesNotExistCheck($"{appDir}/{FilePaths.BuildManifestFileName}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -virtualEnvName {virtualEnvName} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/bash",
                new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("python", "3.7"),
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
        public async Task CanRunPythonApp_WithoutBuildManifestFile()
        {
            // This is AppService's scenario where previously built apps can still run
            // fine.

            // Arrange
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var virtualEnvName = "antenv";
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -p virtualenv_name={virtualEnvName}")
                // App should run fine even with manifest file not present
                .AddCommand($"rm -f {appDir}/{FilePaths.BuildManifestFileName}")
                .AddFileDoesNotExistCheck($"{appDir}/{FilePaths.BuildManifestFileName}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -virtualEnvName {virtualEnvName} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/bash",
                new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("python", "3.7"),
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