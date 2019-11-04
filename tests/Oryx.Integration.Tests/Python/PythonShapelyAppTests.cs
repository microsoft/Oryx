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
    public class PythonShapelyAppTests : PythonEndToEndTestsBase
    {
        public PythonShapelyAppTests(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Theory]
        [MemberData(nameof(TestValueGenerator.GetPythonVersions), MemberType = typeof(TestValueGenerator))]
        public async Task CanBuildAndRun_ShapelyFlaskApp_UsingVirtualEnv(string pythonVersion)
        {
            // Arrange
            var appName = "shapely-flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform python --language-version {pythonVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var imageVersion = _imageHelper.GetRuntimeImage("python", pythonVersion);

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

        [Theory]
        [MemberData(nameof(TestValueGenerator.GetPythonVersions), MemberType = typeof(TestValueGenerator))]
        public async Task CanBuildAndRun_ShapelyFlaskApp_PackageDir(string pythonVersion)
        {
            // Arrange
            const string packageDir = "orx_packages";
            var appName = "shapely-flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand(
                $"oryx build {appDir} --platform python --platform-version {pythonVersion} -p packagedir={packageDir}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var imageVersion = _imageHelper.GetRuntimeImage("python", pythonVersion);

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