// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using ScriptGenerator = Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "dotnetcore")]
    public class DotNetCoreRuntimeVersion21Tests : DotNetCoreEndToEndTestsBase
    {
        public DotNetCoreRuntimeVersion21Tests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanRunApp_WithoutBuildManifestFile()
        {
            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -o {appOutputDir} --platform dotnet --language-version {dotnetcoreVersion}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                // NOTE: Delete the manifest file explicitly
                .AddCommand($"rm -f {appOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddFileDoesNotExistCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
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
            var appOutputDir = $"{appDir}/myoutputdir/appOutput";
            var manifestDir = $"{appDir}/myoutputdir/manifestDir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -o {appOutputDir} --platform dotnet --language-version {dotnetcoreVersion} " +
                $"--manifest-dir {manifestDir}")
                .AddFileExistsCheck($"{manifestDir}/{FilePaths.BuildManifestFileName}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -bindPort {ContainerPort} -manifestDir {manifestDir}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
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
            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", "NetCoreApp21WithExplicitAssemblyName");
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -o {appOutputDir} --platform dotnet --language-version {dotnetcoreVersion}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                // NOTE: Delete the manifest file explicitly
                .AddCommand($"rm -f {appOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddFileDoesNotExistCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
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
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} --platform dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                // NOTE: Make sure the current directory is the output directory
                .AddCommand($"cd {appOutputDir}")
                .AddCommand($"oryx -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
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
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
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
            var appOutputDir = $"{appDir}/myoutputdir";
            var tempAppDir = "/tmp/app";
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {tempAppDir}")
                .AddCommand($"cp -rf {appOutputDir}/* {tempAppDir}")
                .AddCommand(
                $"oryx -appPath {appOutputDir} -output {startupFilePath} " +
                $"-userStartupCommand {startupCommand} -bindPort {ContainerPort}")
                .AddCommand(startupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
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
            var appOutputDir = $"{appDir}/myoutputdir";
            var tempAppDir = "/tmp/app";
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {tempAppDir}")
                .AddCommand($"cp -rf {appOutputDir}/* {tempAppDir}")
                .AddCommand($"echo '#!/bin/bash' >> {userStartupFile}")
                .AddCommand($"echo 'cd {tempAppDir}' >> {userStartupFile}")
                .AddCommand($"echo 'dotnet {tempAppDir}/{NetCoreApp21WebApp}.dll' >> {userStartupFile}")
                .AddCommand($"chmod +x {userStartupFile}")
                .AddCommand(
                $"oryx -appPath {appOutputDir} -userStartupCommand {userStartupFile} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
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
        public async Task CanBuildAndRun_NetCore21WebApp_HavingNestedProjectDirectory_AndNoLanguageVersionSwitch()
        {
            // Arrange
            var appName = "MultiWebAppRepo";
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var repoDir = volume.ContainerDir;
            var setProjectEnvVariable = "export PROJECT=src/WebApp1/WebApp1.csproj";
            var appOutputDir = $"{repoDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(setProjectEnvVariable)
                .AddCommand($"oryx build {repoDir} -o {appOutputDir}") // Do not specify language and version
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(setProjectEnvVariable)
                .AddCommand(
                $"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
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
            var appOutputDir = $"{repoDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {repoDir} -o {appOutputDir}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
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
        public async Task CanBuildAndRunApp_WhenOutputIsZipped_AndIntermediateDir_IsNotUsed()
        {
            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} --platform dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}" +
                $" -p {ScriptGenerator.Constants.ZipAllOutputBuildPropertyKey}=true")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
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
        public async Task CanBuildAndRunApp_WhenOutputIsZipped_AndIntermediateDir_IsUsed()
        {
            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} --platform dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}" +
                $" -p {ScriptGenerator.Constants.ZipAllOutputBuildPropertyKey}=true")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -runFromPath /tmp/output -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
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
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
               .SetEnvironmentVariable("PROJECT", "src/WebApp1/WebApp1.csproj")
               .AddCommand($"oryx build {appDir} -o {appOutputDir}")
               .SetEnvironmentVariable("PROJECT", "src/WebApp2/WebApp2.csproj")
               .AddCommand($"oryx build {appDir} -o {appOutputDir}")
               .AddFileExistsCheck($"{appOutputDir}/MyWebApp.dll")
               .AddFileExistsCheck($"{appOutputDir}/MyWebApp.runtimeconfig.json")
               .AddFileExistsCheck($"{appOutputDir}/WebApp2.dll")
               .AddFileExistsCheck($"{appOutputDir}/WebApp2.runtimeconfig.json")
               .ToString();
            var startupCommand = $"\"dotnet WebApp2.dll\"";
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -userStartupCommand {startupCommand} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
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
            var volume = DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "DotNetCore", appName));
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var defaultAppVolume = CreateDefaultWebAppVolume();
            var defaultAppDir = defaultAppVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
               .SetEnvironmentVariable("PROJECT", "src/WebApp1/WebApp1.csproj")
               .AddCommand($"oryx build {appDir} -o {appOutputDir}")
               .SetEnvironmentVariable("PROJECT", "src/WebApp2/WebApp2.csproj")
               .AddCommand($"oryx build {appDir} -o {appOutputDir}")
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
                $"oryx -appPath {appOutputDir} -defaultAppFilePath {defaultAppDir}/output/{DefaultWebApp}.dll " +
                $"-bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                new List<DockerVolume> { volume, defaultAppVolume },
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

        [Theory]
        [InlineData("doestnotexist")]
        [InlineData("./doestnotexist")]
        [InlineData("dotnet doesnotexist.dll")]
        public async Task CanRunApp_UsingDefaultApp_WhenStartupCommand_IsNotValid(string command)
        {
            // Arrange
            var volume = DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp));
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var defaultAppVolume = CreateDefaultWebAppVolume();
            var defaultAppDir = defaultAppVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -o {appOutputDir}")
               // Create a default web app
               .AddCommand($"cd {defaultAppDir} && dotnet publish -c Release -o {defaultAppDir}/output")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -userStartupCommand \"{command}\" " +
                $"-defaultAppFilePath {defaultAppDir}/output/{DefaultWebApp}.dll " +
                $"-bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                new List<DockerVolume> { volume, defaultAppVolume },
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
            var volume = DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp));
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var renamedAppName = $"{NetCoreApp21WebApp}-renamed";
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -o {appOutputDir}")
               // Rename the project file to get different set of publish output from the earlier build
               .AddCommand($"mv {appDir}/{NetCoreApp21WebApp}.csproj {appDir}/{renamedAppName}.csproj")
               // Rebuild again
               .AddCommand($"oryx build {appDir} -o {appOutputDir}")
               .AddFileExistsCheck($"{appOutputDir}/{NetCoreApp21WebApp}.dll")
               .AddFileExistsCheck($"{appOutputDir}/{NetCoreApp21WebApp}.runtimeconfig.json")
               .AddFileExistsCheck($"{appOutputDir}/{renamedAppName}.dll")
               .AddFileExistsCheck($"{appOutputDir}/{renamedAppName}.runtimeconfig.json")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
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
    }
}