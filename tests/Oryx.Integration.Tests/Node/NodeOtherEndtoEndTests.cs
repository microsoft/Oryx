// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "node")]
    public class NodeOtherEndtoEndTests : NodeEndToEndTestsBase
    {
        public NodeOtherEndtoEndTests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanBuildAndRunNodeApp_UsingCustomManifestFileLocation()
        {
            // Arrange
            var appOutputDirPath = Directory.CreateDirectory(Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N")))
                .FullName;
            var manifestDirPath = Directory.CreateDirectory(
                Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N"))).FullName;
            var appOutputDirVolume = DockerVolume.CreateMirror(appOutputDirPath);
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var manifestDirVolume = DockerVolume.CreateMirror(manifestDirPath);
            var manifestDir = manifestDirVolume.ContainerDir;
            var nodeVersion = "10.14";
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -manifestDir {manifestDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o /tmp/out --platform node " +
                $"--language-version {nodeVersion} --manifest-dir {manifestDir} " +
                "-p compress_node_modules=tar-gz")
                .AddCommand($"cp -rf /tmp/out/* {appOutputDir}")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { appOutputDirVolume, volume, manifestDirVolume },
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildScript
                },
                $"oryxdevmcr.azurecr.io/public/oryx/node-{nodeVersion}",
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Say It Again", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRunNodeApp_UsingZippedNodeModules_WithoutExtracting()
        {
            // NOTE:
            // 1. Use intermediate directory(which here is local to container) to avoid errors like
            //      "tar: node_modules/form-data: file changed as we read it"
            //    related to zipping files on a folder which is volume mounted.
            // 2. Use output directory within the container due to 'rsync'
            //    having issues with volume mounted directories

            // Arrange
            var appOutputDirPath = Directory.CreateDirectory(Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N")))
                .FullName;
            var appOutputDirVolume = DockerVolume.CreateMirror(appOutputDirPath);
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var nodeVersion = "10.14";
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"cd {appOutputDir}")
                .AddCommand("mkdir -p node_modules")
                .AddCommand("tar -xzf node_modules.tar.gz -C node_modules")
                .AddCommand($"oryx -bindPort {ContainerPort} -skipNodeModulesExtraction")
                .AddCommand(DefaultStartupFilePath)
                .AddDirectoryDoesNotExistCheck("/node_modules")
                .ToString();
            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o /tmp/out --platform node " +
                $"--language-version {nodeVersion} -p compress_node_modules=tar-gz")
                .AddCommand($"cp -rf /tmp/out/* {appOutputDir}")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { appOutputDirVolume, volume },
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildScript
                },
                $"oryxdevmcr.azurecr.io/public/oryx/node-{nodeVersion}",
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runAppScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Say It Again", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRunNodeApp_OnSecondBuild_AfterZippingNodeModules_InFirstBuild()
        {
            // NOTE:
            // 1. Use intermediate directory(which here is local to container) to avoid errors like
            //      "tar: node_modules/form-data: file changed as we read it"
            //    related to zipping files on a folder which is volume mounted.
            // 2. Use output directory within the container due to 'rsync'
            //    having issues with volume mounted directories

            // Arrange
            var appOutputDirPath = Directory.CreateDirectory(Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N")))
                .FullName;
            var appOutputDirVolume = DockerVolume.CreateMirror(appOutputDirPath);
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var nodeVersion = "10.14";
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o /tmp/out --platform node " +
                $"--language-version {nodeVersion} -p compress_node_modules=tar-gz")
                .AddCommand($"oryx build {appDir} -i /tmp/int -o /tmp/out --platform node --language-version {nodeVersion}")
                .AddCommand($"cp -rf /tmp/out/* {appOutputDir}")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { appOutputDirVolume, volume },
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildScript
                },
                $"oryxdevmcr.azurecr.io/public/oryx/node-{nodeVersion}",
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Say It Again", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRunNodeApp_OnSecondBuild_AfterNotZippingNodeModules_InFirstBuild()
        {
            // NOTE:
            // 1. Use intermediate directory(which here is local to container) to avoid errors like
            //      "tar: node_modules/form-data: file changed as we read it"
            //    related to zipping files on a folder which is volume mounted.
            // 2. Use output directory within the container due to 'rsync' 
            //    having issues with volume mounted directories

            // Arrange
            var appOutputDirPath = Directory.CreateDirectory(Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N")))
                .FullName;
            var appOutputDirVolume = DockerVolume.CreateMirror(appOutputDirPath);
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var nodeVersion = "10.14";
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -i /tmp/int -o /tmp/out --platform node --language-version {nodeVersion}")
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o /tmp/out --platform node " +
                $"--language-version {nodeVersion} -p compress_node_modules=tar-gz")
                .AddCommand($"cp -rf /tmp/out/* {appOutputDir}")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { appOutputDirVolume, volume },
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildScript
                },
                $"oryxdevmcr.azurecr.io/public/oryx/node-{nodeVersion}",
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Say It Again", data);
                });
        }

        [Fact]
        public async Task NodeStartupScript_UsesPortEnvironmentVariableValue()
        {
            // Arrange
            var nodeVersion = "10.14";
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform node --language-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"export PORT={ContainerPort}")
                .AddCommand($"oryx -appPath {appDir}")
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
                    buildScript
                },
                $"oryxdevmcr.azurecr.io/public/oryx/node-{nodeVersion}",
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Say It Again", data);
                });
        }

        [Fact]
        public async Task NodeStartupScript_UsesSuppliedBindingPort_EvenIfPortEnvironmentVariableValue_IsPresent()
        {
            // Arrange
            var nodeVersion = "10.14";
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform node --language-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"export PORT=9095")
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
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
                    buildScript
                },
                $"oryxdevmcr.azurecr.io/public/oryx/node-{nodeVersion}",
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Say It Again", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRunNodeApp_UsingYarnForBuild_AndExplicitOutputFile()
        {
            // Arrange
            var appName = "webfrontend-yarnlock";
            var nodeVersion = "10.14";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var startupFilePath = "/tmp/startup.sh";
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform node --language-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -output {startupFilePath} -bindPort {ContainerPort}")
                .AddCommand(startupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                 "/bin/sh", new[] { "-c", buildScript },
                $"oryxdevmcr.azurecr.io/public/oryx/node-{nodeVersion}",
                ContainerPort,
                "/bin/sh", new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Say It Again", data);
                });
        }

        // Run on Linux only as TypeScript seems to create symlinks and this does not work on Windows machines.
        [EnableOnPlatform("LINUX")]
        public async Task CanBuildNodeAppUsingScriptsNodeInPackageJson()
        {
            // Arrange
            var appName = "NodeAndTypeScriptHelloWorld";
            var nodeVersion = "10.14";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform node --language-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
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
                    buildScript
                },
                $"oryxdevmcr.azurecr.io/public/oryx/node-{nodeVersion}",
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("{\"message\":\"Hello World!\"}", data);
                });
        }

        [Fact]
        public async Task Node_Lab2AppServiceApp()
        {
            // Arrange
            var appName = "lab2-appservice";
            var nodeVersion = "10.14";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform node --language-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
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
                    buildScript
                },
                $"oryxdevmcr.azurecr.io/public/oryx/node-{nodeVersion}",
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Welcome to Express", data);
                });
        }

        [Fact]
        public async Task Node_SoundCloudNgrxApp()
        {
            // Arrange
            var appName = "soundcloud-ngrx";
            var nodeVersion = "8.11";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform node --language-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
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
                    buildScript
                },
                $"oryxdevmcr.azurecr.io/public/oryx/node-{nodeVersion}",
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var response = await _httpClient.GetAsync($"http://localhost:{hostPort}/");
                    Assert.True(response.IsSuccessStatusCode);
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("<title>SoundCloud • Angular2 NgRx</title>", data);
                });
        }

        [Fact]
        public async Task Node_CreateReactAppSample()
        {
            // Arrange
            var appName = "create-react-app-sample";
            var nodeVersion = "10.14";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform node --language-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
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
                    buildScript
                },
                $"oryxdevmcr.azurecr.io/public/oryx/node-{nodeVersion}",
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("<title>React App</title>", data);
                });
        }

        [Fact]
        public async Task Node_CreateReactAppSample_SingleImage()
        {
            // Arrange
            var appName = "create-react-app-sample";
            var nodeVersion = "10";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform node --language-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx run-script {appDir} --debug --platform node --platform-version {nodeVersion} " +
                            $"--output {DefaultStartupFilePath} -- -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName: appName,
                output: _output,
                volume: volume,
                buildCmd: "/bin/sh",
                buildArgs: new[] { "-c", buildScript },
                runtimeImageName: "oryxdevmcr.azurecr.io/public/oryx/build",
                ContainerPort,
                runCmd: "/bin/sh",
                runArgs: new[]
                {
                    "-c",
                    runScript
                },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("<title>React App</title>", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NodeExpressApp_UsingSingleImage_AndCustomScript()
        {
            // Arrange
            var appName = "linxnodeexpress";
            var nodeVersion = "10";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;

            // Create a custom startup command
            const string customRunScriptName = "customStartup.sh";
            File.WriteAllText(Path.Join(volume.MountedHostDir, customRunScriptName),
                "#!/bin/bash\n" +
                $"PORT={ContainerPort} node server.js\n");
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform node --language-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx run-script {appDir} --debug --platform node --platform-version {nodeVersion} " +
                            $"--output {customRunScriptName} -- -userStartupCommand {customRunScriptName}")
                .AddCommand($"./{customRunScriptName}")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName: appName,
                output: _output,
                volume: volume,
                buildCmd: "/bin/sh",
                buildArgs: new[] { "-c", buildScript },
                runtimeImageName: "oryxdevmcr.azurecr.io/public/oryx/build",
                ContainerPort,
                runCmd: "/bin/sh",
                runArgs: new[]
                {
                    "-c",
                    runScript
                },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Equal("Hello World from express!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NodeExpressApp_UsingSingleImage_AndCustomStartupCommandOnly()
        {
            // Arrange
            var appName = "linxnodeexpress";
            var nodeVersion = "10";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;

            // Create a custom startup command
            const string customRunCommand = "'npm start'";
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform node --language-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx run-script {appDir} --debug --platform node --platform-version {nodeVersion} " +
                            $"--output {DefaultStartupFilePath} -- -bindPort {ContainerPort} " +
                            $"-userStartupCommand {customRunCommand}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName: appName,
                output: _output,
                volume: volume,
                buildCmd: "/bin/sh",
                buildArgs: new[] { "-c", buildScript },
                runtimeImageName: "oryxdevmcr.azurecr.io/public/oryx/build",
                ContainerPort,
                runCmd: "/bin/sh",
                runArgs: new[]
                {
                    "-c",
                    runScript
                },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Equal("Hello World from express!", data);
                });
        }
    }
}