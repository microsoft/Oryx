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
    [Trait("category", "node")]
    public class NodeAngularAppTest : NodeEndToEndTestsBase
    {
        public NodeAngularAppTest(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        public const int PortInContainer = 4200;

        // Official Node.js version that is supported by Angular CLI 6.0+ is 8.9 or greater
        [Theory]
        [InlineData("14")]
        public async Task CanBuildAndRun_Angular6App_WithoutCompressedNodeModules(string nodeVersion)
        {
            // Arrange
            var appName = "angular6app";
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
                _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                    Assert.Contains("Angular6app", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_Angular6App_With_NodeModule_Dir_Exists_InRoot_WithoutCompression()
        {
            // Arrange
            var nodeVersion = "14";
            var appName = "angular6app";
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
                .AddCommand("mkdir -p node_modules")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                    Assert.Contains("Angular6app", data);
                });

            // This is to test a situation where an appservice user is restarting
            // the app multiple times without deploying/pushing code changes
            // We are using same volume mount here and just creating a new container 
            // everytime to see if the symbolink link that gets created cause any issue
            for (int i = 0; i < 3; i++)
            {
                await EndToEndTestHelper.RunAndAssertAppAsync(
                    imageName: _imageHelper.GetRuntimeImage("node", nodeVersion),
                    output: _output,
                    volumes: new List<DockerVolume> { appOutputDirVolume },
                    environmentVariables: null,
                    port: PortInContainer,
                    link: null,
                    runCmd: "/bin/sh",
                    runArgs: new[] { "-c", runScript },
                    assertAction: async (hostPort) =>
                    {
                        var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                        Assert.Contains("Angular6app", data);
                    },
                    dockerCli: new DockerCli());
            }
        }

        [Fact]
        public async Task CanBuildAndRun_Angular6App_With_NodeModule_Dir_Exists_InAppDir_WithoutCompression()
        {
            // Arrange
            var nodeVersion = "14";
            var appName = "angular6app";
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
                .AddCommand($"mkdir -p {appOutputDir}/node_modules")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                    Assert.Contains("Angular6app", data);
                });

            // This is to test a situation where an appservice user is restarting
            // the app multiple times without deploying/pushing code changes
            // We are using same volume mount here and just creating a new container 
            // everytime to see if the symbolink link that gets created cause any issue
            for (int i = 0; i < 3; i++)
            {
                await EndToEndTestHelper.RunAndAssertAppAsync(
                    imageName: _imageHelper.GetRuntimeImage("node", nodeVersion),
                    output: _output,
                    volumes: new List<DockerVolume> { appOutputDirVolume },
                    environmentVariables: null,
                    port: PortInContainer,
                    link: null,
                    runCmd: "/bin/sh",
                    runArgs: new[] { "-c", runScript },
                    assertAction: async (hostPort) =>
                    {
                        var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                        Assert.Contains("Angular6app", data);
                    },
                    dockerCli: new DockerCli());
            }
        }

        [Fact]
        public async Task CanBuildAndRun_Angular6App_With_NodeModule_SymLink_Exists_InRoot_WithoutCompression()
        {
            // Arrange
            var nodeVersion = "14";
            var appName = "angular6app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir}" +
               $" --platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                    Assert.Contains("Angular6app", data);
                });

            // This is to test a situation where an appservice user is restarting
            // the app multiple times without deploying/pushing code changes
            // We are using same volume mount here and just creating a new container 
            // everytime to see if the symbolink link that gets created cause any issue
            for (int i = 0; i < 3; i++)
            {
                await EndToEndTestHelper.RunAndAssertAppAsync(
                    imageName: _imageHelper.GetRuntimeImage("node", nodeVersion),
                    output: _output,
                    volumes: new List<DockerVolume> { appOutputDirVolume },
                    environmentVariables: null,
                    port: PortInContainer,
                    link: null,
                    runCmd: "/bin/sh",
                    runArgs: new[] { "-c", runScript },
                    assertAction: async (hostPort) =>
                    {
                        var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                        Assert.Contains("Angular6app", data);
                    },
                    dockerCli: new DockerCli());
            }
        }

        [Fact]
        public async Task CanBuildAndRun_Angular6App_With_NodeModule_SymLink_Exists_InAppDir_WithoutCompression()
        {
            // Arrange
            var nodeVersion = "14";
            var appName = "angular6app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir}" +
               $" --platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable("PORT", PortInContainer.ToString())
                .AddCommand("mkdir -p /tmp/abcd")
                .AddCommand($"ln -sfn /tmp/abcd {appOutputDir}/node_modules")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                    Assert.Contains("Angular6app", data);
                });

            // This is to test a situation where an appservice user is restarting
            // the app multiple times without deploying/pushing code changes
            // We are using same volume mount here and just creating a new container 
            // everytime to see if the symbolink link that gets created cause any issue
            for (int i = 0; i < 3; i++)
            {
                await EndToEndTestHelper.RunAndAssertAppAsync(
                    imageName: _imageHelper.GetRuntimeImage("node", nodeVersion),
                    output: _output,
                    volumes: new List<DockerVolume> { appOutputDirVolume },
                    environmentVariables: null,
                    port: PortInContainer,
                    link: null,
                    runCmd: "/bin/sh",
                    runArgs: new[] { "-c", runScript },
                    assertAction: async (hostPort) =>
                    {
                        var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                        Assert.Contains("Angular6app", data);
                    },
                    dockerCli: new DockerCli());
            }
        }

        [Fact]
        public async Task CanBuildAndRunAngular6_WithDevAndProdDependencies_NodeModule_Dir_Exists_InAppDir_UsingCompression()
        {
            // Arrange
            var nodeVersion = "14";
            string compressFormat = "tar-gz";
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appName = "angular6app";
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
                _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                    Assert.Contains("Angular6app", data);
                });

            // Re-run the runtime container multiple times against the same output to catch any issues.
            var dockerCli = new DockerCli();
            for (var i = 0; i < 3; i++)
            {
                var restartAppScript = new ShellScriptBuilder()
                .SetEnvironmentVariable("PORT", PortInContainer.ToString())
                .AddCommand($"cat > {appOutputDir}/{i}.txt")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {PortInContainer}")
                .AddCommand(DefaultStartupFilePath)
                .AddFileExistsCheck($"{appOutputDir}/{i}.txt")
                .ToString();

                await EndToEndTestHelper.RunAndAssertAppAsync(
                    imageName: _imageHelper.GetRuntimeImage("node", nodeVersion),
                    output: _output,
                    volumes: new List<DockerVolume> { appOutputDirVolume, volume },
                    environmentVariables: new List<EnvironmentVariable>(),
                    port: PortInContainer,
                    link: null,
                    runCmd: "/bin/sh",
                    runArgs: new[]
                    {
                    "-c",
                    restartAppScript
                    },
                    assertAction: async (hostPort) =>
                    {
                        var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                        Assert.Contains("Angular6app", data);
                    },
                    dockerCli);
            }
        }

        [Fact]
        public async Task CanBuildAndRunAngular6_WithDevAndProdDependencies_NodeModule_SymLink_Exists_InRootDir_UsingCompression()
        {
            // Arrange
            var nodeVersion = "14";
            string compressFormat = "tar-gz";
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appName = "angular6app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runAppScript = new ShellScriptBuilder()
                .SetEnvironmentVariable("PORT", PortInContainer.ToString())
                .AddCommand("mkdir -p /tmp/abcd")
                .AddCommand("ln -sfn /tmp/abcd ./node_modules")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                    Assert.Contains("Angular6app", data);
                });

            // Re-run the runtime container multiple times against the same output to catch any issues.
            var dockerCli = new DockerCli();
            for (var i = 0; i < 3; i++)
            {
                var restartAppScript = new ShellScriptBuilder()
                .SetEnvironmentVariable("PORT", PortInContainer.ToString())
                .AddCommand($"cat > {appOutputDir}/{i}.txt")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {PortInContainer}")
                .AddCommand(DefaultStartupFilePath)
                .AddFileExistsCheck($"{appOutputDir}/{i}.txt")
                .ToString();

                await EndToEndTestHelper.RunAndAssertAppAsync(
                    imageName: _imageHelper.GetRuntimeImage("node", nodeVersion),
                    output: _output,
                    volumes: new List<DockerVolume> { appOutputDirVolume, volume },
                    environmentVariables: new List<EnvironmentVariable>(),
                    port: PortInContainer,
                    link: null,
                    runCmd: "/bin/sh",
                    runArgs: new[]
                    {
                    "-c",
                    restartAppScript
                    },
                    assertAction: async (hostPort) =>
                    {
                        var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                        Assert.Contains("Angular6app", data);
                    },
                    dockerCli);
            }
        }

        [Theory]
        [InlineData("14")]
        public async Task CanBuildAndRunAngular6_WithDevAndProdDependencies_UsingCompressedNodeModules(string nodeVersion)
        {
            // Arrange
            string compressFormat = "tar-gz";
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appName = "angular6app";
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
                _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                    Assert.Contains("Angular6app", data);
                });
        }

        // Official Node.js version that is supported by Angular CLI 8.0+ is 10.9 or greater
        [Theory]
        [InlineData("14")]
        public async Task CanBuildAndRun_Angular8App_WithoutCompressedNodeModules(string nodeVersion)
        {
            // Arrange
            var appName = "angular8app";
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
                _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                    Assert.Contains("Angular8app", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_Angular8App_NodeModules_Dir_Exists_InRoot_WithoutCompression()
        {
            // Arrange
            var nodeVersion = "14";
            var appName = "angular8app";
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
                .AddCommand("mkdir -p node_modules")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                    Assert.Contains("Angular8app", data);
                });

            // This is to test a situation where an appservice user is restarting
            // the app multiple times without deploying/pushing code changes
            // We are using same volume mount here and just creating a new container 
            // everytime to see if the symbolink link that gets created cause any issue

            for (int i = 0; i < 3; i++)
            {
                var restartScript = new ShellScriptBuilder()
                .SetEnvironmentVariable("PORT", PortInContainer.ToString())
                .AddCommand($"cat > {appDir}/{i}.txt")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {PortInContainer}")
                .AddCommand(DefaultStartupFilePath)
                .AddFileExistsCheck($"{appDir}/{i}.txt")
                .ToString();

                await EndToEndTestHelper.RunAndAssertAppAsync(
                    imageName: _imageHelper.GetRuntimeImage("node", nodeVersion),
                    output: _output,
                    volumes: new List<DockerVolume> { appOutputDirVolume },
                    environmentVariables: null,
                    port: PortInContainer,
                    link: null,
                    runCmd: "/bin/sh",
                    runArgs: new[] { "-c", runScript },
                    assertAction: async (hostPort) =>
                    {
                        var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                        Assert.Contains("Angular8app", data);
                    },
                    dockerCli: new DockerCli());
            }
        }

        [Fact]
        public async Task CanBuildAndRun_Angular8App_NodeModules_SymLink_Exists_InRoot_WithoutCompression()
        {
            // Arrange
            var nodeVersion = "14";
            var appName = "angular8app";
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
                _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                    Assert.Contains("Angular8app", data);
                });

            // This is to test a situation where an appservice user is restarting
            // the app multiple times without deploying/pushing code changes
            // We are using same volume mount here and just creating a new container 
            // everytime to see if the symbolink link that gets created cause any issue

            for (int i = 0; i < 3; i++)
            {
                await EndToEndTestHelper.RunAndAssertAppAsync(
                    imageName: _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                        Assert.Contains("Angular8app", data);
                    },
                    dockerCli: new DockerCli());
            }
        }

        [Fact]
        public async Task CanBuildAndRunAngular8_WithDevAndProdDependencies_NodeModules_Dir_Exists_InRoot_UsingCompression()
        {
            // Arrange
            var nodeVersion = "14";
            string compressFormat = "tar-gz";
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appName = "angular8app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runAppScript = new ShellScriptBuilder()
                .SetEnvironmentVariable("PORT", PortInContainer.ToString())
                .AddCommand("mkdir -p node_modules")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                    Assert.Contains("Angular8app", data);
                });

            // This is to test a situation where an appservice user is restarting
            // the app multiple times without deploying/pushing code changes
            // We are using same volume mount here and just creating a new container 
            // everytime to see if the symbolink link that gets created cause any issue

            for (int i = 0; i < 3; i++)
            {
                var reStartAppScript = new ShellScriptBuilder()
                .SetEnvironmentVariable("PORT", PortInContainer.ToString())
                .AddCommand($"cat > {appOutputDir}/{i}.txt")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {PortInContainer}")
                .AddCommand(DefaultStartupFilePath)
                .AddFileExistsCheck($"{appOutputDir}/{i}.txt")
                .ToString();

                await EndToEndTestHelper.RunAndAssertAppAsync(
                    imageName: _imageHelper.GetRuntimeImage("node", nodeVersion),
                    output: _output,
                    volumes: new List<DockerVolume> { appOutputDirVolume, volume },
                    environmentVariables: null,
                    port: PortInContainer,
                    link: null,
                    runCmd: "/bin/sh",
                    runArgs: new[] { "-c", reStartAppScript },
                    assertAction: async (hostPort) =>
                    {
                        var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                        Assert.Contains("Angular8app", data);
                    },
                    dockerCli: new DockerCli());
            }
        }

        [Fact]
        public async Task CanBuildAndRunAngular8_WithDevAndProdDependencies_NodeModules_Dir_Exists_InAppDir_UsingCompression()
        {
            // Arrange
            var nodeVersion = "14";
            string compressFormat = "tar-gz";
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appName = "angular8app";
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
                _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                    Assert.Contains("Angular8app", data);
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
                    imageName: _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                        Assert.Contains("Angular8app", data);
                    },
                    dockerCli: new DockerCli());
            }
        }

        [Fact]
        public async Task CanBuildAndRunAngular8_WithDevAndProdDependencies_NodeModules_SymLink_Exists_InRoot_UsingCompression()
        {
            // Arrange
            var nodeVersion = "14";
            string compressFormat = "tar-gz";
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appName = "angular8app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runAppScript = new ShellScriptBuilder()
                .SetEnvironmentVariable("PORT", PortInContainer.ToString())
                .AddCommand("mkdir -p /tmp/abcd")
                .AddCommand("ln -sfn /tmp/abcd ./node_modules")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                    Assert.Contains("Angular8app", data);
                });

            // This is to test a situation where an appservice user is restarting
            // the app multiple times without deploying/pushing code changes
            // We are using same volume mount here and just creating a new container 
            // everytime to see if the symbolink link that gets created cause any issue

            for (int i = 0; i < 3; i++)
            {
                var reRunScript = new ShellScriptBuilder()
                .SetEnvironmentVariable("PORT", PortInContainer.ToString())
                .AddCommand($"cat > {appOutputDir}/{i}.txt")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {PortInContainer}")
                .AddCommand(DefaultStartupFilePath)
                .AddFileExistsCheck($"{appOutputDir}/{i}.txt")
                .ToString();

                await EndToEndTestHelper.RunAndAssertAppAsync(
                    imageName: _imageHelper.GetRuntimeImage("node", nodeVersion),
                    output: _output,
                    volumes: new List<DockerVolume> { appOutputDirVolume, volume },
                    environmentVariables: null,
                    port: PortInContainer,
                    link: null,
                    runCmd: "/bin/sh",
                    runArgs: new[] { "-c", runAppScript },
                    assertAction: async (hostPort) =>
                    {
                        var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                        Assert.Contains("Angular8app", data);
                    },
                    dockerCli: new DockerCli());
            }
        }

        [Fact]
        public async Task CanBuildAndRunAngular8_WithDevAndProdDependencies_NodeModules_SymLink_Exists_InAppDir_UsingCompression()
        {
            // Arrange
            var nodeVersion = "14";
            string compressFormat = "tar-gz";
            int count = 0;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appName = "angular8app";
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
                _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                    Assert.Contains("Angular8app", data);
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
                    imageName: _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                        Assert.Contains("Angular8app", data);
                    },
                    dockerCli: new DockerCli());
            }
        }

        [Theory]
        [InlineData("14")]
        public async Task CanBuildAndRunAngular8_WithDevAndProdDependencies_UsingCompressedNodeModules(string nodeVersion)
        {
            // Arrange
            string compressFormat = "tar-gz";
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appName = "angular8app";
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
                _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                    Assert.Contains("Angular8app", data);
                });
        }
    }
}