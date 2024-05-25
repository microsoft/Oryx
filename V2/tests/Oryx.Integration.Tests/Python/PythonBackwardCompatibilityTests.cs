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
    public class PythonBackwardCompatibilityTests : PythonEndToEndTestsBase
    {
        public PythonBackwardCompatibilityTests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        [Trait("category", "python-3.7")]
        [Trait("build-image", "debian-stretch")]
        public async Task CanRunPythonApp_UsingEarlierBuiltPackagesDirectoryAsync()
        {
            // This is AppService's scenario where previously built apps can still run
            // fine.

            // Arrange
            var appName = "django-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var virtualEnvName = "antenv";
            var buildScript = new ShellScriptBuilder()
                // App should run fine even with manifest file not present
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"-p packagedir={PythonConstants.DefaultTargetPackageDirectory}")
                .AddCommand($"rm -f {appDir}/{FilePaths.BuildManifestFileName}")
                .AddFileDoesNotExistCheck($"{appDir}/{FilePaths.BuildManifestFileName}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} " +
                $"-virtualEnvName {virtualEnvName} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/bash",
                new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("python", "3.7", ImageTestHelperConstants.OsTypeDebianBullseye),
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await GetResponseDataAsync($"http://localhost:{hostPort}/staticfiles/css/boards.css");
                    Assert.Contains("CSS file from Boards app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}/staticfiles/css/uservoice.css");
                    Assert.Contains("CSS file from UserVoice app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}/boards/");
                    Assert.Contains("Hello, World! from Boards app", data);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}/uservoice/");
                    Assert.Contains("Hello, World! from Uservoice app", data);
                });
        }

        [Fact]
        [Trait("category", "python-3.7")]
        [Trait("build-image", "debian-stretch")]
        public async Task CanRunPythonApp_WithoutBuildManifestFileAsync()
        {
            // This is AppService's scenario where previously built apps can still run
            // fine.

            // Arrange
            var appName = "django-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var virtualEnvName = "antenv";
            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"-p virtualenv_name={virtualEnvName} --platform {PythonConstants.PlatformName} --platform-version 3.7")
                // App should run fine even with manifest file not present
                .AddCommand($"rm -f {appOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddFileDoesNotExistCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -virtualEnvName {virtualEnvName} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/bash",
                new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("python", "3.7", ImageTestHelperConstants.OsTypeDebianBullseye),
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await GetResponseDataAsync($"http://localhost:{hostPort}/staticfiles/css/boards.css");
                    Assert.Contains("CSS file from Boards app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}/staticfiles/css/uservoice.css");
                    Assert.Contains("CSS file from UserVoice app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}/boards/");
                    Assert.Contains("Hello, World! from Boards app", data);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}/uservoice/");
                    Assert.Contains("Hello, World! from Uservoice app", data);
                });
        }
    }
}