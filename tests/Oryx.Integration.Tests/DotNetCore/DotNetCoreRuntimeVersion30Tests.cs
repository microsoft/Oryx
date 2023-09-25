// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "dotnetcore-3.0")]
    public class DotNetCoreRuntimeVersion30Tests : DotNetCoreEndToEndTestsBase
    {
        public DotNetCoreRuntimeVersion30Tests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRun_NetCore30WebAppAsync()
        {
            // Arrange
            var dotnetcoreVersion = "3.0";
            var osType = ImageTestHelperConstants.OsTypeDebianBuster;
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp30WebApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int --platform {DotNetCoreConstants.PlatformName} " +
               $"--platform-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp30WebApp,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", dotnetcoreVersion, osType),
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
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRun_NetCore30MvcAppAsync()
        {
            // Arrange
            var dotnetcoreVersion = "3.0";
            var osType = ImageTestHelperConstants.OsTypeDebianBuster;
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp30MvcApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int --platform {DotNetCoreConstants.PlatformName} " +
               $"--platform-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp30WebApp,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", dotnetcoreVersion, osType),
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
                    Assert.Contains("Welcome to ASP.NET Core MVC!", data);
                });
        }

        [Fact]
        [Trait("build-image", "debian-stretch")]
        public async Task CanRun_NetCore30App_PublishedOnMacMachine_ButRunOnNetCore30RuntimeContainerAsync()
        {
            // This test verifies that we fallback to using 'dotnet TodoAppFromMac.dll' since the executable
            // file 'TodoAppFromMac' was indeed generated from a Mac OS and cannot be run in a Linux container.

            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", "TodoAppFromMac");
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                _imageHelper.GetRuntimeImage("dotnetcore", "3.0", ImageTestHelperConstants.OsTypeDebianBuster),
                _output,
                new List<DockerVolume>() { volume },
                environmentVariables: null,
                ContainerPort,
                link: null,
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
                },
                new DockerCli());
        }

        [Fact]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRun_NetCore30WebApp_UsingExplicitStartupCommandAsync()
        {
            // Arrange
            var dotnetcoreVersion = "3.0";
            var osType = ImageTestHelperConstants.OsTypeDebianBuster;
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp30WebApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var startupCommand = "./NetCoreApp30.WebApp";
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
               $"--platform {DotNetCoreConstants.PlatformName} " +
               $"--platform-version {dotnetcoreVersion}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -userStartupCommand {startupCommand} " +
                $"-bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp30WebApp,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", dotnetcoreVersion, osType),
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
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRun_NetCore30WebApp_HavingExplicitAssemblyNameAsync()
        {
            // Arrange
            var appName = "NetCoreApp30WebAppWithExplicitAssemblyName";
            var dotnetcoreVersion = "3.0";
            var osType = ImageTestHelperConstants.OsTypeDebianBuster;
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int --platform {DotNetCoreConstants.PlatformName} " +
               $"--platform-version {dotnetcoreVersion} -o {appOutputDir}")
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
                _imageHelper.GetRuntimeImage("dotnetcore", dotnetcoreVersion, osType),
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
    }
}