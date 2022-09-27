// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public class PythonShapelyAppTests : PythonEndToEndTestsBase
    {
        public PythonShapelyAppTests(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Fact]
        [Trait("category", "python-37")]
        public async Task RunPython37ShapelyAppTests()
        {
            await CanBuildAndRun_ShapelyFlaskApp_UsingVirtualEnvAsync("3.7");
            await CanBuildAndRun_ShapelyFlaskApp_PackageDirAsync("3.7");
        }

        [Fact]
        [Trait("category", "python-38")]
        public async Task RunPython38ShapelyAppTests()
        {
            await CanBuildAndRun_ShapelyFlaskApp_UsingVirtualEnvAsync("3.8");
            await CanBuildAndRun_ShapelyFlaskApp_PackageDirAsync("3.8");
        }

        [Fact]
        [Trait("category", "python-39")]
        public async Task RunPython39ShapelyAppTests()
        {
            await CanBuildAndRun_ShapelyFlaskApp_UsingVirtualEnvAsync("3.9");
            await CanBuildAndRun_ShapelyFlaskApp_PackageDirAsync("3.9");
        }

        private async Task CanBuildAndRun_ShapelyFlaskApp_UsingVirtualEnvAsync(string pythonVersion)
        {
            // Arrange
            var appName = "shapely-flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
               $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var imageVersion = _imageHelper.GetRuntimeImage("python", pythonVersion);

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
                imageVersion,
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
                    Assert.Contains("Hello Shapely, Area is: 314", data);
                });
        }

        private async Task CanBuildAndRun_ShapelyFlaskApp_PackageDirAsync(string pythonVersion)
        {
            // Arrange
            const string packageDir = "orx_packages";
            var appName = "shapely-flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion} -p packagedir={packageDir}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var imageVersion = _imageHelper.GetRuntimeImage("python", pythonVersion);

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
                imageVersion,
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
                    Assert.Contains("Hello Shapely, Area is: 314", data);
                });
        }
    }
}