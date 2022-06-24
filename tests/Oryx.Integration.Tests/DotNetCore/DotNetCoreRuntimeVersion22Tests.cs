// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "dotnetcore-22")]
    public class DotNetCoreRuntimeVersion22Tests : DotNetCoreEndToEndTestsBase
    {
        public DotNetCoreRuntimeVersion22Tests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore22WebApp()
        {
            // Arrange
            var dotnetcoreVersion = "2.2";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp22WebApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int --platform {DotNetCoreConstants.PlatformName} " +
               $"--platform-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();

            // NOTE: Do NOT change this as we want to make sure that all the non-existent directories in the path
            //       are created.
            var startupFile = "/tmp/a/b/c/startup.sh";

            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort} -output {startupFile}")
                .AddCommand(startupFile)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp22WebApp,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", dotnetcoreVersion),
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore22WebApp_UsingDebugConfiguration()
        {
            // Arrange
            var dotnetcoreVersion = "2.2";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp22WebApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(EnvironmentSettingsKeys.MSBuildConfiguration, "Debug")
                .AddCommand($"oryx build {appDir} -i /tmp/int --platform {DotNetCoreConstants.PlatformName} " +
                $"--platform-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp22WebApp,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", dotnetcoreVersion),
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World! from Debug build.", data);
                });
        }

        [Fact]
        public async Task DotNetCoreStartupScript_UsesPortEnvironmentVariableValue()
        {
            // Arrange
            var dotnetcoreVersion = "2.2";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp22WebApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int --platform {DotNetCoreConstants.PlatformName} " +
               $"--platform-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"export PORT={ContainerPort}")
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp22WebApp,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", dotnetcoreVersion),
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task StartupScript_UsesSuppliedBindingPort_EvenIfPortEnvironmentVariableValue_IsPresent()
        {
            // Arrange
            var dotnetcoreVersion = "2.2";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp22WebApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int --platform {DotNetCoreConstants.PlatformName} " +
               $"--platform-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"export PORT=9095")
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp22WebApp,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", dotnetcoreVersion),
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore22WebApp_HavingExplicitAssemblyName_WithWhiteSpaceInIt()
        {
            // Arrange
            var appName = "NetCoreApp22WithExplicitAssemblyName";
            var dotnetcoreVersion = "2.2";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int --platform {DotNetCoreConstants.PlatformName} " +
               $"--platform-version {dotnetcoreVersion} -o {appOutputDir}")
               .AddFileExistsCheck($"{appOutputDir}/foo  bar.dll")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", dotnetcoreVersion),
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_UsingDefaultApp_WhenCannotFindAnyStartupFile()
        {
            // Arrange
            var defaultAppVolume = CreateDefaultWebAppVolume();
            var defaultAppDir = defaultAppVolume.ContainerDir;
            var doesNotContainApp = "/tmp/does-not-contain-app";
            var buildImageScript = new ShellScriptBuilder()
               // create a default web app
               .AddCommand($"cd {defaultAppDir} && dotnet publish -c Release -o {defaultAppDir}/output")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {doesNotContainApp}")
                .AddCommand(
                $"oryx create-script -appPath {doesNotContainApp} " +
                $"-defaultAppFilePath {defaultAppDir}/output/{DefaultWebApp}.dll " +
                $"-bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp22WebApp,
                _output,
                defaultAppVolume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", "2.1"),
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/executingDir");
                    Assert.Contains($"App is running from directory: {defaultAppDir}", data);
                    data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Running default web app", data);
                });
        }
    }
}