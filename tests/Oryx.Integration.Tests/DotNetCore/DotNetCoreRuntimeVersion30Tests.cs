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
    [Trait("category", "dotnetcore-30")]
    public class DotNetCoreRuntimeVersion30Tests : DotNetCoreEndToEndTestsBase
    {
        public DotNetCoreRuntimeVersion30Tests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanBuildAndRun_FSharpNetCoreApp21WebApp_WithoutSpecifyingPlatformExplicitly()
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", "FSharpNetCoreApp21.WebApp");
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir}")
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
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!", data);
                });

        }

        [Fact]
        public async Task CanBuildAndRun_NetCore30WebApp()
        {
            // Arrange
            var dotnetcoreVersion = "3.0";
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
        public async Task CanBuildAndRun_NetCore30MvcApp()
        {
            // Arrange
            var dotnetcoreVersion = "3.0";
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
                    Assert.Contains("Welcome to ASP.NET Core MVC!", data);
                });
        }

        [Fact]
        public async Task CanRun_SelfContainedApp_TargetedForLinux()
        {
            // ****************************************************************
            // A self-contained app is an app which does not depend on whether a .NET Core runtime is present on the
            // target machine or not. It has all the required dependencies (.dlls etc.) in its published output itself,
            // hence it is called self-contained app.
            //
            // To test if our runtime images run self-contained apps correctly (i.e not using the dotnet runtime
            // installed in the runtime image itself), in this test we publish a self-contained 3.0 app and run it in
            // a 1.1 runtime container. This is because 1.1 runtime container does not have 3.0 bits at all and hence
            // if the app fails to run in that container, then we are doing something wrong. If all is well, this 3.0
            // app should run fine.
            // ****************************************************************

            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp30WebApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
               .Source($"benv dotnet={DotNetCoreSdkVersions.DotNetCore30SdkVersion}")
               .AddCommand($"cd {appDir}")
               .AddCommand($"dotnet publish -c release -r linux-x64 -o {appOutputDir}")
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
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                // NOTE: it is 1.1 version on purpose. Read comments at the beginning of this method for more details
                _imageHelper.GetRuntimeImage("dotnetcore", "1.1"),
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
        public async Task CanRun_NetCore30App_PublishedOnMacMachine_ButRunOnNetCore30RuntimeContainer()
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
                _imageHelper.GetRuntimeImage("dotnetcore", "3.0"),
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
        public async Task CanBuildAndRun_NetCore30WebApp_UsingExplicitStartupCommand()
        {
            // Arrange
            var dotnetcoreVersion = "3.0";
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
        public async Task CanBuildAndRun_NetCore30WebApp_HavingExplicitAssemblyName()
        {
            // Arrange
            var appName = "NetCoreApp30WebAppWithExplicitAssemblyName";
            var dotnetcoreVersion = "3.0";
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
    }
}