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
    [Trait("category", "node-12-6")]
    public class NodeTestUsingZippedNodeModules : NodeEndToEndTestsBase
    {
        public NodeTestUsingZippedNodeModules(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Theory]
        [MemberData(nameof(TestValueGenerator.GetNodeVersions), MemberType = typeof(TestValueGenerator))]

        public async Task CanBuildAndRunNodeApp_Using_TarGz_zippedNodeModules(string nodeVersion)
        {
            // Arrange
            var compressFormat = "tar-gz";
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appName = "linxnodeexpress";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
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
                    Assert.Equal("Hello World from express!", data);
                });
        }

        [Theory]
        [InlineData("12")]
        [InlineData("14")]
        public async Task Node_CreateReactAppSample_zippedNodeModules(string nodeVersion)
        {
            // Arrange
            // Use a separate volume for output due to rsync errors
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appName = "create-react-app-sample";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var buildScript = new ShellScriptBuilder()
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} " +
                $"--platform-version {nodeVersion} -p compress_node_modules=zip")
               .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { appOutputDirVolume, volume },
                "/bin/bash",
                new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("node", nodeVersion),
                ContainerPort,
                "/bin/sh",
                new[] { "-c", runAppScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("<title>React App</title>", data);
                });
        }

        [Fact]
        public async Task BuildsAndRunsNodeApp_WhenPruneDevDependenciesIsTrue_AndNodeModulesAreCompressed()
        {
            // Arrange
            // Use a separate volume for output due to rsync errors
            var nodeVersion = "12";
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var buildScript = new ShellScriptBuilder()
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} " +
                $"--platform-version {nodeVersion} -p {NodePlatform.CompressNodeModulesPropertyKey}=zip" +
                $" -p {NodePlatform.PruneDevDependenciesPropertyKey}=true")
               .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { appOutputDirVolume, volume }, Settings.LtsVersionsBuildImageName,
                "/bin/bash",
                new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("node", nodeVersion),
                ContainerPort,
                "/bin/sh",
                new[] { "-c", runAppScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Say It Again", data);
                });
        }

        [Theory(Skip = "Bug#1071724")]
        [InlineData("true")]
        [InlineData("false")]
        public async Task CopiesNodeModulesInSubDirectory_ToDestination_WhenNodeModulesAreCompressed(string pruneDevDependency)
        {
            // Arrange
            // Use a separate volume for output due to rsync errors
            var nodeVersion = "12";
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appName = "node-nested-nodemodules";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .AddDirectoryExistsCheck($"{appOutputDir}/another-directory/node_modules")
                .AddDirectoryDoesNotExistCheck($"{appOutputDir}/node_modules")
                .ToString();
            var buildScript = new ShellScriptBuilder()
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o /tmp/out --platform {NodeConstants.PlatformName} " +
                $"--platform-version {nodeVersion} -p {NodePlatform.CompressNodeModulesPropertyKey}=tar-gz" +
                $" -p {NodePlatform.PruneDevDependenciesPropertyKey}={pruneDevDependency}")
                .AddCommand($"cp -rf /tmp/out/* {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/another-directory/node_modules")
                .AddDirectoryDoesNotExistCheck($"{appOutputDir}/node_modules")
               .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { appOutputDirVolume, volume }, Settings.LtsVersionsBuildImageName,
                "/bin/bash",
                new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("node", nodeVersion),
                ContainerPort,
                "/bin/sh",
                new[] { "-c", runAppScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Welcome to Express", data);
                });
        }
    }
}