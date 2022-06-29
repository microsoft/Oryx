// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "dotnetcore-21")]
    public class DotNetCoreRuntimeVersion21Tests : DotNetCoreEndToEndTestsBase
    {
        public DotNetCoreRuntimeVersion21Tests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanRunApp_WithoutBuildManifestFile()
        {
            //NOTE: This test simulates the scenario where a user publishes the app via Visual Studio

            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} --platform {DotNetCoreConstants.PlatformName} " +
                $"--platform-version {dotnetcoreVersion}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.OsTypeFileName}")
                // NOTE: Delete the manifest file explicitly
                .AddCommand($"rm -f {appOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddFileDoesNotExistCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
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
        public async Task CanRunApp_WithCustomBuildManifestFileLocation()
        {
            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var manifestDirVolume = CreateAppOutputDirVolume();
            var manifestDir = manifestDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} --platform {DotNetCoreConstants.PlatformName} " +
                $"--platform-version {dotnetcoreVersion} " +
                $"--manifest-dir {manifestDir}")
                .AddFileExistsCheck($"{manifestDir}/{FilePaths.BuildManifestFileName}")
                // Additionally ensure that the ostype file is placed where the manifest file is, not the output dir
                .AddFileExistsCheck($"{manifestDir}/{FilePaths.OsTypeFileName}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort} -manifestDir {manifestDir}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                new[] { volume, appOutputDirVolume, manifestDirVolume },
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
        public async Task CanRunApp_NetCore21WebApp_NetCoreApp21WithExplicitAssemblyName_AndNoBuildManifestFile()
        {
            //NOTE: This test simulates the scenario where a user publishes the app via Visual Studio

            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", "NetCoreApp21WithExplicitAssemblyName");
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} --platform {DotNetCoreConstants.PlatformName} " +
                $"--platform-version {dotnetcoreVersion}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                // NOTE: Delete the manifest file explicitly
                .AddCommand($"rm -f {appOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddFileDoesNotExistCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
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
        public async Task CanBuildAndRun_NetCore21WebApp_WhenOutputDirIsCurrentDirectory()
        {
            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp);
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
                // NOTE: Make sure the current directory is the output directory
                .AddCommand($"cd {appOutputDir}")
                .AddCommand($"oryx create-script -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
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
        public async Task CanBuildAndRun_NetCore21WebApp_HavingExplicitAssemblyName()
        {
            // Arrange
            var appName = "NetCoreApp21WithExplicitAssemblyName";
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
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

        public static TheoryData<string> StartupCommandData
        {
            get
            {
                var tempAppDir = "/tmp/app";
                var data = new TheoryData<string>();

                data.Add($"'dotnet {tempAppDir}/{NetCoreApp21WebApp}.dll'");
                data.Add($"'echo \"foo bar\" && dotnet {tempAppDir}/{NetCoreApp21WebApp}.dll'");
                data.Add($"'bash -c \"echo foo && dotnet {tempAppDir}/{NetCoreApp21WebApp}.dll\"'");
                data.Add($"'key=\"a;b;c\" dotnet {tempAppDir}/{NetCoreApp21WebApp}.dll'");

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(StartupCommandData))]
        public async Task CanBuildAndRun_NetCore21WebApp_UsingExplicitStartupCommand(string startupCommand)
        {
            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var startupFilePath = "/tmp/run.sh";
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var tempAppDir = "/tmp/app";
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int --platform {DotNetCoreConstants.PlatformName} " +
                $"--platform-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {tempAppDir}")
                .AddCommand($"cp -rf {appOutputDir}/* {tempAppDir}")
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -output {startupFilePath} " +
                $"-userStartupCommand {startupCommand} -bindPort {ContainerPort}")
                .AddCommand(startupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
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
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/appDllLocation");
                    Assert.Equal($"Location: {tempAppDir}/{NetCoreApp21WebApp}.dll", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_UsingExplicitStartupScriptFile()
        {
            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var userStartupFile = "/tmp/userStartup.sh";
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var tempAppDir = "/tmp/app";
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int --platform {DotNetCoreConstants.PlatformName} " +
                $"--platform-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {tempAppDir}")
                .AddCommand($"cp -rf {appOutputDir}/* {tempAppDir}")
                .AddCommand($"echo '#!/bin/bash' >> {userStartupFile}")
                .AddCommand($"echo 'cd {tempAppDir}' >> {userStartupFile}")
                .AddCommand($"echo 'dotnet {tempAppDir}/{NetCoreApp21WebApp}.dll' >> {userStartupFile}")
                .AddCommand($"chmod +x {userStartupFile}")
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -userStartupCommand {userStartupFile} " +
                $"-bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
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
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/appDllLocation");
                    Assert.Equal($"Location: {tempAppDir}/{NetCoreApp21WebApp}.dll", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore21WebApp_HavingNestedProjectDirectory_AndNoPlatformVersionSwitch()
        {
            // Arrange
            var appName = "MultiWebAppRepo";
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var repoDir = volume.ContainerDir;
            var setProjectEnvVariable = "export PROJECT=src/WebApp1/WebApp1.csproj";
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(setProjectEnvVariable)
                .AddCommand($"oryx build {repoDir} -i /tmp/int -o {appOutputDir}") // Do not specify platform and version
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(setProjectEnvVariable)
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
                    Assert.Contains("Hello World! from WebApp1", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore21WebApp_HavingMultipleProjects()
        {
            // Arrange
            var appName = "NetCoreApp22MultiProjectApp";
            var dotnetcoreVersion = "2.2";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var repoDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {repoDir} -i /tmp/int -o {appOutputDir}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
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
        public async Task CanRunApp_HavingMultipleRuntimeConfigJsonFiles_AndExplicitStartupCommand()
        {
            // Scenario: When a user does a rename of a project and publishes it, there would be new files
            // (.dll and .runtimeconfig.json) having this new name. If the user does not clean the output directory
            // having the old files, then we would be having multiple runtimeconfig.json files in which case we will
            // be unable to choose. This scenario happens with VS Publish but can also happen with any app which does
            // not go through Oryx's build.
            // Here we are trying to simulate that scenario and verifying that in this kind of situation, a user could
            // workaround by supplying an explicit startup command.

            // Arrange
            var appName = "MultiWebAppRepo";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var appVolume = DockerVolume.CreateMirror(hostDir);
            var appDir = appVolume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
               .SetEnvironmentVariable("PROJECT", "src/WebApp1/WebApp1.csproj")
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir}")
               .SetEnvironmentVariable("PROJECT", "src/WebApp2/WebApp2.csproj")
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir}")
               .AddFileExistsCheck($"{appOutputDir}/MyWebApp.dll")
               .AddFileExistsCheck($"{appOutputDir}/MyWebApp.runtimeconfig.json")
               .AddFileExistsCheck($"{appOutputDir}/WebApp2.dll")
               .AddFileExistsCheck($"{appOutputDir}/WebApp2.runtimeconfig.json")
               .ToString();
            var startupCommand = $"\"dotnet WebApp2.dll\"";
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -userStartupCommand {startupCommand} " +
                $"-bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                new[] { appVolume, appOutputDirVolume },
                _imageHelper.GetLtsVersionsBuildImage(),
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
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/appDllLocation");
                    Assert.Contains($"Location: {appOutputDir}/WebApp2.dll", data);
                });
        }

        [Fact]
        public async Task CanRunApp_UsingDefaultApp_WhenHavingMultipleRuntimeConfigJsonFiles()
        {
            // Arrange
            var appName = "MultiWebAppRepo";
            var appVolume = DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "DotNetCore", appName));
            var appDir = appVolume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var defaultAppVolume = CreateDefaultWebAppVolume();
            var defaultAppDir = defaultAppVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
               .SetEnvironmentVariable("PROJECT", "src/WebApp1/WebApp1.csproj")
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir}")
               .SetEnvironmentVariable("PROJECT", "src/WebApp2/WebApp2.csproj")
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir}")
               .AddFileExistsCheck($"{appOutputDir}/MyWebApp.dll")
               .AddFileExistsCheck($"{appOutputDir}/MyWebApp.runtimeconfig.json")
               .AddFileExistsCheck($"{appOutputDir}/WebApp2.dll")
               .AddFileExistsCheck($"{appOutputDir}/WebApp2.runtimeconfig.json")
               // Create a default web app
               .AddCommand($"cd {defaultAppDir} && dotnet publish -c Release -o {defaultAppDir}/output")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"rm -f {appOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} " +
                $"-defaultAppFilePath {defaultAppDir}/output/{DefaultWebApp}.dll " +
                $"-bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                new List<DockerVolume> { appVolume, defaultAppVolume, appOutputDirVolume },
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
                    Assert.Equal("Running default web app", data);
                });
        }

        [Fact]
        public async Task CanRunCorrectApp_WhenOutputHasMultipleRuntimeConfigJsonFiles_DueToProjectFileRenaming()
        {
            // Arrange
            var appVolume = DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp));
            var appDir = appVolume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var renamedAppName = $"{NetCoreApp21WebApp}-renamed";
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir}")
               // Rename the project file to get different set of publish output from the earlier build
               .AddCommand($"mv {appDir}/{NetCoreApp21WebApp}.csproj {appDir}/{renamedAppName}.csproj")
               // Rebuild again
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir}")
               .AddFileExistsCheck($"{appOutputDir}/{NetCoreApp21WebApp}.dll")
               .AddFileExistsCheck($"{appOutputDir}/{NetCoreApp21WebApp}.runtimeconfig.json")
               .AddFileExistsCheck($"{appOutputDir}/{renamedAppName}.dll")
               .AddFileExistsCheck($"{appOutputDir}/{renamedAppName}.runtimeconfig.json")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                new[] { appVolume, appOutputDirVolume },
                _imageHelper.GetLtsVersionsBuildImage(),
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
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/appDllLocation");
                    Assert.Contains($"Location: {appOutputDir}/{renamedAppName}.dll", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRunApp_WhenRecursiveLookUpIsDisabled_ButProjectSettingIsSupplied()
        {
            // Arrange
            var appName = "MultiWebAppRepo";
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var repoDir = volume.ContainerDir;
            var setProjectEnvVariable = "export PROJECT=src/WebApp1/WebApp1.csproj";
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(setProjectEnvVariable)
                .SetEnvironmentVariable(SettingsKeys.DisableRecursiveLookUp, "true")
                .AddCommand($"oryx build {repoDir} -i /tmp/int -o {appOutputDir}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(setProjectEnvVariable)
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
                    Assert.Contains("Hello World! from WebApp1", data);
                });
        }
    }
}