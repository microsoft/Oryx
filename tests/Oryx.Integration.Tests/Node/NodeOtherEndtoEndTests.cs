// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public class NodeOtherEndtoEndTests : NodeEndToEndTestsBase
    {

        protected static string NodeVersion = "14";

        public NodeOtherEndtoEndTests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }


        [Fact]
        [Trait("category", "node-14-stretch-4")]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRunNodeApp_UsingCustomManifestFileLocationAsync()
        {
            // Arrange
            var manifestDirPath = Directory.CreateDirectory(
                Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N"))).FullName;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var manifestDirVolume = DockerVolume.CreateMirror(manifestDirPath);
            var manifestDir = manifestDirVolume.ContainerDir;
            var nodeVersion = NodeVersion;
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -manifestDir {manifestDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} " +
                $"--platform-version {nodeVersion} --manifest-dir {manifestDir} " +
                "-p compress_node_modules=tar-gz")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
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
        [Trait("category", "node-14-stretch-4")]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRunNodeApp_UsingZippedNodeModules_WithoutExtractingAsync()
        {
            // Arrange
            var appOutputDirPath = Directory.CreateDirectory(Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N")))
                .FullName;
            var appOutputDirVolume = DockerVolume.CreateMirror(appOutputDirPath);
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var nodeVersion = NodeVersion;
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"cd {appOutputDir}")
                .AddCommand("mkdir -p node_modules")
                .AddCommand("tar -xzf node_modules.tar.gz -C node_modules")
                .AddCommand($"oryx create-script -bindPort {ContainerPort} -skipNodeModulesExtraction")
                .AddCommand(DefaultStartupFilePath)
                .AddDirectoryDoesNotExistCheck("/node_modules")
                .ToString();
            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} " +
                $"--platform-version {nodeVersion} -p compress_node_modules=tar-gz")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
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

        [Theory(Skip = "Bug#1071724")]
        [Trait("category", "node-14-skipped")]
        [Trait("build-image", "debian-stretch")]
        [InlineData("true")]
        [InlineData("false")]
        public async Task CopiesNodeModulesInSubDirectory_ToDestinationAre_WithoutCompressedNodeModulesAsync(string pruneDevDependency)
        {
            // Arrange
            // Use a separate volume for output due to rsync errors
            var appOutputDirPath = Directory.CreateDirectory(Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N")))
                .FullName;
            var nodeVersion = "14";
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appOutputDirVolume = DockerVolume.CreateMirror(appOutputDirPath);
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appName = "node-nested-nodemodules";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .AddDirectoryExistsCheck($"{appOutputDir}/another-directory/node_modules")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .ToString();
            var buildScript = new ShellScriptBuilder()
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o /tmp/out --platform {NodeConstants.PlatformName} " +
                $"--platform-version {nodeVersion}" +
                $" -p {NodePlatform.PruneDevDependenciesPropertyKey}={pruneDevDependency}")
                .AddCommand($"cp -rf /tmp/out/* {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/another-directory/node_modules")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
               .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { appOutputDirVolume, volume }, Settings.LtsVersionsBuildImageName,
                "/bin/bash",
                new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                ContainerPort,
                "/bin/sh",
                new[] { "-c", runAppScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Welcome to Express", data);
                });
        }

        [Fact]
        [Trait("category", "node-14-stretch-4")]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRunNodeApp_OnSecondBuild_AfterZippingNodeModules_InFirstBuildAsync()
        {
            // Arrange
            var appOutputDirPath = Directory.CreateDirectory(Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N")))
                .FullName;
            var appOutputDirVolume = DockerVolume.CreateMirror(appOutputDirPath);
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var nodeVersion = NodeVersion;
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} " +
                $"--platform-version {nodeVersion} -p compress_node_modules=tar-gz")
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} " +
                $"--platform-version {nodeVersion}")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
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
        [Trait("category", "node-14-stretch-4")]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRunNodeApp_OnSecondBuild_AfterNotZippingNodeModules_InFirstBuildAsync()
        {
            // Arrange
            var appOutputDirPath = Directory.CreateDirectory(Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N")))
                .FullName;
            var appOutputDirVolume = DockerVolume.CreateMirror(appOutputDirPath);
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var nodeVersion = NodeVersion;
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} " +
                $"--platform-version {nodeVersion} -p compress_node_modules=tar-gz")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
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
        [Trait("category", "node-14-stretch-4")]
        [Trait("build-image", "debian-stretch")]
        public async Task NodeStartupScript_UsesPortEnvironmentVariableValueAsync()
        {
            // Arrange
            var nodeVersion = NodeVersion;
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"export PORT={ContainerPort}")
                .AddCommand($"oryx create-script -appPath {appDir}")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
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

        [Theory]
        [Trait("build-image", "debian-stretch")]
        [InlineData("ecosystem.config.js"), Trait("category", "node-14-stretch-4")]
        [InlineData("ecosystem.config.yaml"), Trait("category", "node-14-stretch-4")]
        [InlineData("ecosystem.config.yml"), Trait("category", "node-14-stretch-4")]
        public async Task CanRunNodeApp_WithoutPm2_EvenThoughPm2SpecificFilesArePresentInRepoAsync(string pm2ConfigFileName)
        {
            // Arrange
            // NOTE: this version does not have PM2 installed and so if PM2 was used in the startup script this test
            // will fail.
            var nodeVersion = NodeVersion;
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            // Create the pm2's config file
            File.WriteAllText(Path.Combine(volume.MountedHostDir, pm2ConfigFileName), "");
            var buildScript = new ShellScriptBuilder()
               .AddCommand(
                $"oryx build {appDir} --platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
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
        [Trait("category", "node-14-stretch-4")]
        [Trait("build-image", "debian-stretch")]
        public async Task NodeStartupScript_UsesSuppliedBindingPort_EvenIfPortEnvironmentVariableValue_IsPresentAsync()
        {
            // Arrange
            var nodeVersion = NodeVersion;
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"export PORT=9095")
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
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
        [Trait("category", "node-14-stretch-4")]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRunNodeApp_UsingYarnForBuild_AndExplicitOutputFileAsync()
        {
            // Arrange
            var appName = "webfrontend-yarnlock";
            var nodeVersion = NodeVersion;
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var startupFilePath = "/tmp/startup.sh";
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appDir} -output {startupFilePath} -bindPort {ContainerPort}")
                .AddCommand(startupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                 "/bin/sh", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                ContainerPort,
                "/bin/sh", new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Say It Again", data);
                });
        }

        // Run on Linux only as TypeScript seems to create symlinks and this does not work on Windows machines.
        [EnableOnPlatform("LINUX"), Trait("category", "node-14-stretch-4")]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildNodeAppUsingScriptsNodeInPackageJsonAsync()
        {
            // Arrange
            var appName = "NodeAndTypeScriptHelloWorld";
            var nodeVersion = NodeVersion;
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
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
        [Trait("category", "node-14-stretch-4")]
        [Trait("build-image", "debian-stretch")]
        public async Task Node_Lab2AppServiceAppAsync()
        {
            // Arrange
            var appName = "lab2-appservice";
            var nodeVersion = NodeVersion;
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
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

        [Fact(Skip = "bug: 1505700")]
        [Trait("category", "node-14-skipped")]
        [Trait("build-image", "debian-stretch")]
        public async Task Node_SoundCloudNgrxAppAsync()
        {
            // Arrange
            var appName = "soundcloud-ng18";
            var nodeVersion = "18.20.8";
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
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
        [Trait("category", "node-14-stretch-4")]
        [Trait("build-image", "debian-stretch")]
        public async Task Node_CreateReactAppSampleAsync()
        {
            // Arrange
            var appName = "create-react-app-sample";
            var nodeVersion = NodeVersion;
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
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

        [Fact(Skip = "get rid of single image, #1088920")]
        [Trait("category", "node-14-skipped")]
        [Trait("build-image", "debian-stretch")]
        public async Task Node_CreateReactAppSample_SingleImageAsync()
        {
            // Arrange
            var appName = "create-react-app-sample";
            var nodeVersion = "10";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx run-script {appDir} --debug --platform {NodeConstants.PlatformName} --platform-version {nodeVersion} " +
                            $"--output {DefaultStartupFilePath} -- -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName: appName,
                output: _output,
                volume: volume,
                buildCmd: "/bin/sh",
                buildArgs: new[] { "-c", buildScript },
                runtimeImageName: _imageHelper.GetBuildImage(),
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

        [Fact(Skip = "get rid of single image, #1088920")]
        [Trait("category", "node-14-skipped")]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRun_NodeExpressApp_UsingSingleImage_AndCustomScriptAsync()
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
               .AddCommand($"oryx build {appDir} --platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx run-script {appDir} --debug --platform {NodeConstants.PlatformName} --platform-version {nodeVersion} " +
                            $"--output {customRunScriptName} -- -userStartupCommand {customRunScriptName}")
                .AddCommand($"./{customRunScriptName}")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName: appName,
                output: _output,
                volume: volume,
                buildCmd: "/bin/sh",
                buildArgs: new[] { "-c", buildScript },
                runtimeImageName: _imageHelper.GetBuildImage(),
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

        [Fact(Skip = "get rid of single image, #1088920")]
        [Trait("category", "node-14-skipped")]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRun_NodeExpressApp_UsingSingleImage_AndCustomStartupCommandOnlyAsync()
        {
            // Arrange
            var appName = "linxnodeexpress";
            var nodeVersion = "10";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;

            // Create a custom startup command
            const string customRunCommand = "'npm start'";
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx run-script {appDir} --debug --platform {NodeConstants.PlatformName} --platform-version {nodeVersion} " +
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
                runtimeImageName: _imageHelper.GetBuildImage(),
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
        [Trait("category", "node-14-stretch-4")]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRunNodeApp_UsingNestedOutputDirectoryAsync()
        {
            // Arrange
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -o {appDir}/output")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appDir}/output -bindPort {ContainerPort}")
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
                _imageHelper.GetRuntimeImage("node", NodeVersion, ImageTestHelperConstants.OsTypeDebianBullseye),
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
        [Trait("category", "node-14-stretch-4")]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRunNodeApp_UsingIntermediateDir_AndNestedOutputDirectoryAsync()
        {
            // Arrange
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appDir}/output")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appDir}/output -bindPort {ContainerPort}")
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
                _imageHelper.GetRuntimeImage("node", NodeVersion, ImageTestHelperConstants.OsTypeDebianBullseye),
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
    }
}