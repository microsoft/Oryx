// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public class NodeAngularAppTest : NodeEndToEndTestsBase
    {
        public NodeAngularAppTest(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        public const int PortInContainer = 4200;

        // Official Node.js version that is supported by Angular CLI 16.0+ is 16.13 or greater
        [Theory(Skip = "Temporarily skipping Angular 16 tests: Work item 1565890")]
        [Trait("build-image", "debian-stretch")]
        [InlineData("16", ImageTestHelperConstants.OsTypeDebianBuster), Trait("category", "node-16")]
        [InlineData("16", ImageTestHelperConstants.OsTypeDebianBullseye), Trait("category", "node-16")]
        public async Task CanBuildAndRunAngular16_WithDevAndProdDependencies_UsingCompressedNodeModulesAsync(
            string nodeVersion,
            string osType)
        {
            // Arrange
            string compressFormat = "tar-gz";
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appName = "angular16app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runAppScript = new ShellScriptBuilder()
                .SetEnvironmentVariable("PORT", PortInContainer.ToString())
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {PortInContainer}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} " +
                $"--platform-version {nodeVersion} -p compress_node_modules={compressFormat}")
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
                PortInContainer,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runAppScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Angular16App", data);
                });
        }

        [Theory(Skip = "Temporarily skipping Angular 16 tests: Work item 1565890")]
        [Trait("build-image", "debian-stretch")]
        [InlineData("16", ImageTestHelperConstants.OsTypeDebianBuster), Trait("category", "node-16")]
        [InlineData("16", ImageTestHelperConstants.OsTypeDebianBullseye), Trait("category", "node-16")]
        public async Task CanBuildAndRun_Angular16App_WithoutCompressedNodeModulesAsync(
            string nodeVersion,
            string osType)
        {
            // Arrange
            var appName = "angular16app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
               $"--platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable("PORT", PortInContainer.ToString())
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {PortInContainer}")
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
                    buildScript
                },
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                PortInContainer,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Angular16app", data);
                });
        }

        [Theory(Skip = "Temporarily skipping Angular 16 tests: Work item 1565890")]
        [Trait("build-image", "debian-stretch")]
        [InlineData("16", ImageTestHelperConstants.OsTypeDebianBuster), Trait("category", "node-16")]
        [InlineData("16", ImageTestHelperConstants.OsTypeDebianBullseye), Trait("category", "node-16")]
        public async Task CanBuildAndRun_Angular14App_NodeModules_SymLink_Exists_InRoot_WithoutCompressionAsync(
            string nodeVersion,
            string osType)
        {
            // Arrange
            var appName = "angular16app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
               $"--platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable("PORT", PortInContainer.ToString())
                .AddCommand("mkdir -p /tmp/abcd")
                .AddCommand("ln -sfn /tmp/abcd ./node_modules")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {PortInContainer}")
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
                    buildScript
                },
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                PortInContainer,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Angular16app", data);
                });
            // This is to test a situation where an appservice user is restarting
            // the app multiple times without deploying/pushing code changes
            // We are using same volume mount here and just creating a new container 
            // everytime to see if the symbolink link that gets created cause any issue
            for (int i = 0; i < 3; i++)
            {
                await EndToEndTestHelper.RunAndAssertAppAsync(
                    imageName: _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                    output: _output,
                    volumes: new List<DockerVolume> { volume, appOutputDirVolume },
                    environmentVariables: null,
                    port: PortInContainer,
                    link: null,
                    runCmd: "/bin/sh",
                    runArgs: new[] { "-c", runScript },
                    assertAction: async (hostPort) =>
                    {
                        var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                        Assert.Contains("Angular16app", data);
                    },
                    dockerCli: new DockerCli());
            }
        }

        [Theory(Skip = "Temporarily skipping Angular 16 tests: Work item 1565890")]
        [Trait("build-image", "debian-stretch")]
        [InlineData("16", ImageTestHelperConstants.OsTypeDebianBuster), Trait("category", "node-16")]
        [InlineData("16", ImageTestHelperConstants.OsTypeDebianBullseye), Trait("category", "node-16")]
        public async Task CanBuildAndRunAngular16_WithDevAndProdDependencies_NodeModules_Dir_Exists_InAppDir_UsingCompressionAsync(
            string nodeVersion,
            string osType)
        {
            // Arrange
            string compressFormat = "tar-gz";
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appName = "angular16app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runAppScript = new ShellScriptBuilder()
                .SetEnvironmentVariable("PORT", PortInContainer.ToString())
                .AddCommand($"mkdir -p {appDir}/node_modules")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {PortInContainer}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} " +
                $"--platform-version {nodeVersion} -p compress_node_modules={compressFormat}")
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
                PortInContainer,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runAppScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Angular16app", data);
                });
            // This is to test a situation where an appservice user is restarting
            // the app multiple times without deploying/pushing code changes
            // We are using same volume mount here and just creating a new container 
            // everytime to see if the symbolink link that gets created cause any issue
            for (int i = 0; i < 3; i++)
            {
                var restartAppScript = new ShellScriptBuilder()
                .SetEnvironmentVariable("PORT", PortInContainer.ToString())
                .AddCommand($"cat > {appOutputDir}/{i}.txt")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {PortInContainer}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
                await EndToEndTestHelper.RunAndAssertAppAsync(
                    imageName: _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                    output: _output,
                    volumes: new List<DockerVolume> { appOutputDirVolume, volume },
                    environmentVariables: null,
                    port: PortInContainer,
                    link: null,
                    runCmd: "/bin/sh",
                    runArgs: new[] { "-c", restartAppScript },
                    assertAction: async (hostPort) =>
                    {
                        var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                        Assert.Contains("Angular16app", data);
                    },
                    dockerCli: new DockerCli());
            }
        }

        [Theory(Skip = "Temporarily skipping Angular 16 tests: Work item 1565890")]
        [Trait("build-image", "debian-stretch")]
        [InlineData("16", ImageTestHelperConstants.OsTypeDebianBuster), Trait("category", "node-16")]
        [InlineData("16", ImageTestHelperConstants.OsTypeDebianBullseye), Trait("category", "node-16")]
        public async Task CanBuildAndRunAngular16_WithDevAndProdDependencies_NodeModules_SymLink_Exists_InAppDir_UsingCompressionAsync(
            string nodeVersion,
            string osType)
        {
            // Arrange
            string compressFormat = "tar-gz";
            int count = 0;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appName = "angular16app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runAppScript = new ShellScriptBuilder()
                .SetEnvironmentVariable("PORT", PortInContainer.ToString())
                .AddCommand("mkdir -p /tmp/abcd")
                .AddCommand($"ln -sfn /tmp/abcd {appDir}/node_modules")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {PortInContainer}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} " +
                $"--platform-version {nodeVersion} -p compress_node_modules={compressFormat}")
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
                PortInContainer,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runAppScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Angular16app", data);
                });
            // This is to test a situation where an appservice user is restarting
            // the app multiple times without deploying/pushing code changes
            // We are using same volume mount here and just creating a new container 
            // everytime to see if the symbolink link that gets created cause any issue
            for (count = 0; count < 3; count++)
            {
                var reRunScript = new ShellScriptBuilder()
                .SetEnvironmentVariable("PORT", PortInContainer.ToString())
                .AddCommand($"cat > {appOutputDir}/{count}.txt")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {PortInContainer}")
                .AddCommand(DefaultStartupFilePath)
                .AddFileExistsCheck($"{appOutputDir}/{count}.txt")
                .ToString();
                await EndToEndTestHelper.RunAndAssertAppAsync(
                    imageName: _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                    output: _output,
                    volumes: new List<DockerVolume> { appOutputDirVolume, volume },
                    environmentVariables: null,
                    port: PortInContainer,
                    link: null,
                    runCmd: "/bin/sh",
                    runArgs: new[] { "-c", reRunScript },
                    assertAction: async (hostPort) =>
                    {
                        var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                        Assert.Contains("Angular16app", data);
                    },
                    dockerCli: new DockerCli());
            }
        }
    }
}