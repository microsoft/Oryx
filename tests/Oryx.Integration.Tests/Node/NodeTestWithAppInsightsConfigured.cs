// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "node")]
    public class NodeTestWithAppInsightsConfigured : NodeEndToEndTestsBase
    {
        public NodeTestWithAppInsightsConfigured(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Theory]
        [MemberData(
           nameof(TestValueGenerator.GetNodeVersions),
           MemberType = typeof(TestValueGenerator))]
        // From 1.7.2 onward appinsights sdk have new environment variable "APPLICATIONINSIGHTS_CONNECTION_STRING"
        // instead  of "APPINSIGHTS_INSTRUMENTATIONKEY"
        public async Task CanBuildAndRun_NodeApp_WithAppInsights_Old_Env_Configured(string nodeVersion)
        {
            // Arrange
            var appName = "linxnodeexpress";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var spcifyNodeVersionCommand = "--platform {NodeConstants.PlatformName} --platform-version=" + nodeVersion;
            var aIKey = ExtVarNames.UserAppInsightsKeyEnv;
            var aIEnabled = ExtVarNames.UserAppInsightsEnableEnv;
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -o {appDir} {spcifyNodeVersionCommand} --log-file {appDir}/1.log")
                .AddDirectoryExistsCheck($"{appDir}/node_modules").ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"export {aIKey}=asdas")
                .AddCommand($"export {aIEnabled}=true")
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .AddFileExistsCheck($"{FilePaths.NodeGlobalModulesPath}/{FilePaths.NodeAppInsightsLoaderFileName}")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { volume },
                Settings.BuildImageName,
                "/bin/bash",
                 new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetTestRuntimeImage("node", nodeVersion),
                new List<EnvironmentVariable> { new EnvironmentVariable(aIKey, "asdasda"), new EnvironmentVariable(aIEnabled, "TRUE") },
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
                    Assert.Contains("Hello World from express!", data);
                });
        }

        [Theory]
        [InlineData("10")]
        [InlineData("12")]
        // From 1.7.2 onward appinsights sdk have new environment variable "APPLICATIONINSIGHTS_CONNECTION_STRING"
        // instead  of "APPINSIGHTS_INSTRUMENTATIONKEY"
        public async Task CanBuildAndRun_NodeApp_WithAppInsights_New_Env_Variable(string nodeVersion)
        {
            // Arrange
            var appName = "linxnodeexpress-appinsights";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var spcifyNodeVersionCommand = "--platform {NodeConstants.PlatformName} --platform-version=" + nodeVersion;
            var connectionString = ExtVarNames.UserAppInsightsConnectionStringEnv;
            var aIEnabled = ExtVarNames.UserAppInsightsEnableEnv;
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -o {appDir} {spcifyNodeVersionCommand} --log-file {appDir}/1.log")
                .AddDirectoryExistsCheck($"{appDir}/node_modules").ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"export {connectionString}=asdas")
                .AddCommand($"export {aIEnabled}=true")
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand($"cat run.sh")
                .AddCommand(DefaultStartupFilePath)
                .AddFileExistsCheck($"{FilePaths.NodeGlobalModulesPath}/{FilePaths.NodeAppInsightsLoaderFileName}")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { volume },
                Settings.BuildImageName,
                "/bin/bash",
                 new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetTestRuntimeImage("node", nodeVersion),
                new List<EnvironmentVariable> { new EnvironmentVariable(connectionString, "asdasda"), new EnvironmentVariable(aIEnabled, "true") },
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
                    Assert.Contains("AppInsights is set to send telemetry!", data);
                });
        }

        [Theory]
        [InlineData("10")]
        [InlineData("12")]
        // From 1.7.2 onward appinsights sdk have new environment variable "APPLICATIONINSIGHTS_CONNECTION_STRING"
        // instead  of "APPINSIGHTS_INSTRUMENTATIONKEY"
        public async Task CanBuildAndRun_NodeApp_Without_AppInsights_Old_Env_Variable_Configuration(string nodeVersion)
        {
            // Arrange
            var appName = "linxnodeexpress-appinsights";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var spcifyNodeVersionCommand = "--platform {NodeConstants.PlatformName} --platform-version=" + nodeVersion;
            var aIKey = ExtVarNames.UserAppInsightsKeyEnv;
            var aIEnabled = ExtVarNames.UserAppInsightsEnableEnv;
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -o {appDir} {spcifyNodeVersionCommand} --log-file {appDir}/1.log")
                .AddDirectoryExistsCheck($"{appDir}/node_modules").ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"export {aIEnabled}=disabled")
                .AddCommand($"export {aIKey}=asdas")
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .AddFileExistsCheck($"{FilePaths.NodeGlobalModulesPath}/{FilePaths.NodeAppInsightsLoaderFileName}")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { volume },
                Settings.BuildImageName,
                "/bin/bash",
                 new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetTestRuntimeImage("node", nodeVersion),
                new List<EnvironmentVariable> { new EnvironmentVariable(aIKey, "asdasda"), new EnvironmentVariable(aIEnabled, "disabled") },
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
                    Assert.Contains("AppInsights is not configured!", data);
                });
        }

        [Theory]
        [InlineData("10")]
        [InlineData("12")]
        // From 1.7.2 onward appinsights sdk have new environment variable "APPLICATIONINSIGHTS_CONNECTION_STRING"
        // instead  of "APPINSIGHTS_INSTRUMENTATIONKEY"
        public async Task CanBuildAndRun_NodeApp_Without_AppInsights_New_Env_Variable_Configuration(string nodeVersion)
        {
            // Arrange
            var appName = "linxnodeexpress-appinsights";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var spcifyNodeVersionCommand = "--platform {NodeConstants.PlatformName} --platform-version=" + nodeVersion;
            var connectionString = ExtVarNames.UserAppInsightsConnectionStringEnv;
            var aIEnabled = ExtVarNames.UserAppInsightsEnableEnv;
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -o {appDir} {spcifyNodeVersionCommand} --log-file {appDir}/1.log")
                .AddDirectoryExistsCheck($"{appDir}/node_modules").ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"export {connectionString}=asdas")
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .AddFileExistsCheck($"{FilePaths.NodeGlobalModulesPath}/{FilePaths.NodeAppInsightsLoaderFileName}")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { volume },
                Settings.BuildImageName,
                "/bin/bash",
                 new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetTestRuntimeImage("node", nodeVersion),
                new List<EnvironmentVariable> { new EnvironmentVariable(connectionString, "asdasda"), new EnvironmentVariable(aIEnabled, "") },
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
                    Assert.Contains("AppInsights is not configured!", data);
                });
        }
        
        [Theory]
        [InlineData("10")]
        [InlineData("12")]
        // From 1.7.2 onward appinsights sdk have new environment variable "APPLICATIONINSIGHTS_CONNECTION_STRING"
        // instead  of "APPINSIGHTS_INSTRUMENTATIONKEY"
        public async Task CanBuildAndRun_NodeApp_AppInsights_Old_Env_Variable_Configuration(string nodeVersion)
        {
            // Arrange
            var appName = "linxnodeexpress-appinsights";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var spcifyNodeVersionCommand = "--platform {NodeConstants.PlatformName} --platform-version=" + nodeVersion;
            var aIKey = ExtVarNames.UserAppInsightsKeyEnv;
            var aIEnabled = ExtVarNames.UserAppInsightsEnableEnv;
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -o {appDir} {spcifyNodeVersionCommand} --log-file {appDir}/1.log")
                .AddDirectoryExistsCheck($"{appDir}/node_modules").ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"export {aIEnabled}=~2")
                .AddCommand($"export {aIKey}=asdas")
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .AddFileExistsCheck($"{FilePaths.NodeGlobalModulesPath}/{FilePaths.NodeAppInsightsLoaderFileName}")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { volume },
                Settings.BuildImageName,
                "/bin/bash",
                 new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetTestRuntimeImage("node", nodeVersion),
                new List<EnvironmentVariable> { new EnvironmentVariable(aIKey, "asdas"), new EnvironmentVariable(aIEnabled, "~2") },
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
                    Assert.Contains("AppInsights is set to send telemetry!", data);
                });
        }
    }
}