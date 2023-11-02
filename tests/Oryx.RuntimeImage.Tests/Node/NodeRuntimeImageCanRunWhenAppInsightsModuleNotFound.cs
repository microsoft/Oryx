// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    public class NodeRuntimeImageCanRunWhenAppInsightsModuleNotFound : NodeRuntimeImageTestBase
    {
        public NodeRuntimeImageCanRunWhenAppInsightsModuleNotFound(
            ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Theory]
        [Trait("category", "runtime-buster")]
        [MemberData(
           nameof(TestValueGenerator.GetBusterNodeVersions),
           MemberType = typeof(TestValueGenerator))]
        public async Task GeneratesScriptForBuster_CanRun_AppInsightsModule_NotFoundAsync(string nodeVersion, string osType)
        {
            // This test is for the following scenario:
            // When we find no application insight dependency in package.json, but env variables  for
            // configuring application insights has been set in portal

            // Arrange
            var appName = "linxnodeexpress";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var imageName = _imageHelper.GetRuntimeImage("node", nodeVersion, osType);
            var aiConnectionString
                = ExtVarNames.UserAppInsightsConnectionStringEnv;
            var aIEnabled = ExtVarNames.UserAppInsightsAgentExtensionVersion;
            int containerDebugPort = 8080;

            var script = new ShellScriptBuilder()
                .AddCommand($"export {aiConnectionString}={TestConstants.AppInsightsConnectionString}")
                .AddCommand($"export {aIEnabled}=TRUE")
                .AddCommand($"cd {appDir}")
                .AddCommand("npm install")
                .AddCommand($"oryx create-script -appPath {appDir}")
                .AddDirectoryExistsCheck($"{appDir}/node_modules")
                .AddDirectoryDoesNotExistCheck($"{appDir}/node_modules/applicationinsights")
                .AddFileExistsCheck($"{FilePaths.NodeGlobalModulesPath}/{FilePaths.NodeAppInsightsLoaderFileName}")
                .AddCommand("./run.sh")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                port: containerDebugPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", script },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World from express!", data);
                },
                dockerCli: _dockerCli);
        }

        [Theory]
        [Trait("category", "runtime-bullseye")]
        [MemberData(
           nameof(TestValueGenerator.GetBullseyeNodeVersions),
           MemberType = typeof(TestValueGenerator))]
        public async Task GeneratesScriptForBullseye_CanRun_AppInsightsModule_NotFoundAsync(string nodeVersion, string osType)
        {
            // This test is for the following scenario:
            // When we find no application insight dependency in package.json, but env variables  for
            // configuring application insights has been set in portal

            // Arrange
            var appName = "linxnodeexpress";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var imageName = _imageHelper.GetRuntimeImage("node", nodeVersion, osType);
            var aiConnectionString
                = ExtVarNames.UserAppInsightsConnectionStringEnv;
            var aIEnabled = ExtVarNames.UserAppInsightsAgentExtensionVersion;
            int containerDebugPort = 8080;

            var script = new ShellScriptBuilder()
                .AddCommand($"export {aiConnectionString}={TestConstants.AppInsightsConnectionString}")
                .AddCommand($"export {aIEnabled}=TRUE")
                .AddCommand($"cd {appDir}")
                .AddCommand("npm install")
                .AddCommand($"oryx create-script -appPath {appDir}")
                .AddDirectoryExistsCheck($"{appDir}/node_modules")
                .AddDirectoryDoesNotExistCheck($"{appDir}/node_modules/applicationinsights")
                .AddFileExistsCheck($"{FilePaths.NodeGlobalModulesPath}/{FilePaths.NodeAppInsightsLoaderFileName}")
                .AddCommand("./run.sh")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                port: containerDebugPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", script },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World from express!", data);
                },
                dockerCli: _dockerCli);
        }

        [Theory]
        [Trait("category", "runtime-bookworm")]
        [MemberData(
            nameof(TestValueGenerator.GetBookwormNodeVersions),
            MemberType = typeof(TestValueGenerator))]
        public async Task GeneratesScriptForBookworm_CanRun_AppInsightsModule_NotFoundAsync(string nodeVersion, string osType)
        {
            // This test is for the following scenario:
            // When we find no application insight dependency in package.json, but env variables for
            // configuring application insights has been set in portal

            // Arrange
            var appName = "linxnodeexpress";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var imageName = _imageHelper.GetRuntimeImage("node", nodeVersion, osType);
            var aiConnectionString
                = ExtVarNames.UserAppInsightsConnectionStringEnv;
            var aIEnabled = ExtVarNames.UserAppInsightsAgentExtensionVersion;
            int containerDebugPort = 8080;

            var script = new ShellScriptBuilder()
                .AddCommand($"export {aiConnectionString}={TestConstants.AppInsightsConnectionString}")
                .AddCommand($"export {aIEnabled}=TRUE")
                .AddCommand($"cd {appDir}")
                .AddCommand("npm install")
                .AddCommand($"oryx create-script -appPath {appDir}")
                .AddDirectoryExistsCheck($"{appDir}/node_modules")
                .AddDirectoryDoesNotExistCheck($"{appDir}/node_modules/applicationinsights")
                .AddFileExistsCheck($"{FilePaths.NodeGlobalModulesPath}/{FilePaths.NodeAppInsightsLoaderFileName}")
                .AddCommand("./run.sh")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                port: containerDebugPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", script },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World from express!", data);
                },
                dockerCli: _dockerCli);
        }

        [Theory]
        [Trait("category", "runtime-buster")]
        [MemberData(
           nameof(TestValueGenerator.GetBusterNodeVersions),
           MemberType = typeof(TestValueGenerator))]
        public async Task GeneratesScriptForBuster_CanRun_With_AppInsights_Env_Variables_NotConfigured_Async(string nodeVersion, string osType)
        {
            // This test is for the following scenario:
            // When we find no application insight dependency in package.json and env variables for
            // configuring application insights has not been set properly in portal

            // Arrange
            var appName = "linxnodeexpress";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var imageName = _imageHelper.GetRuntimeImage("node", nodeVersion, osType);
            var aIEnabled = ExtVarNames.UserAppInsightsAgentExtensionVersion;
            int containerDebugPort = 8080;

            var script = new ShellScriptBuilder()
                .AddCommand($"export {aIEnabled}=disabled")
                .AddCommand($"cd {appDir}")
                .AddCommand("npm install")
                .AddCommand($"oryx create-script -appPath {appDir}")
                .AddDirectoryExistsCheck($"{appDir}/node_modules")
                .AddDirectoryDoesNotExistCheck($"{appDir}/node_modules/applicationinsights")
                .AddCommand("./run.sh")
                .AddFileDoesNotExistCheck($"{appDir}/oryx-appinsightsloader.js")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                port: containerDebugPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", script },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World from express!", data);
                },
                dockerCli: _dockerCli);
        }

        [Theory]
        [Trait("category", "runtime-bullseye")]
        [MemberData(
           nameof(TestValueGenerator.GetBullseyeNodeVersions),
           MemberType = typeof(TestValueGenerator))]
        public async Task GeneratesScriptForBullseye_CanRun_With_AppInsights_Env_Variables_NotConfigured_Async(string nodeVersion, string osType)
        {
            // This test is for the following scenario:
            // When we find no application insight dependency in package.json and env variables for
            // configuring application insights has not been set properly in portal

            // Arrange
            var appName = "linxnodeexpress";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var imageName = _imageHelper.GetRuntimeImage("node", nodeVersion, osType);
            var aIEnabled = ExtVarNames.UserAppInsightsAgentExtensionVersion;
            int containerDebugPort = 8080;

            var script = new ShellScriptBuilder()
                .AddCommand($"export {aIEnabled}=disabled")
                .AddCommand($"cd {appDir}")
                .AddCommand("npm install")
                .AddCommand($"oryx create-script -appPath {appDir}")
                .AddDirectoryExistsCheck($"{appDir}/node_modules")
                .AddDirectoryDoesNotExistCheck($"{appDir}/node_modules/applicationinsights")
                .AddCommand("./run.sh")
                .AddFileDoesNotExistCheck($"{appDir}/oryx-appinsightsloader.js")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                port: containerDebugPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", script },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World from express!", data);
                },
                dockerCli: _dockerCli);
        }

        [Theory]
        [Trait("category", "runtime-bookworm")]
        [MemberData(
           nameof(TestValueGenerator.GetBookwormNodeVersions),
           MemberType = typeof(TestValueGenerator))]
        public async Task GeneratesScriptForBookworm_CanRun_With_AppInsights_Env_Variables_NotConfigured_Async(string nodeVersion, string osType)
        {
            // This test is for the following scenario:
            // When we find no application insight dependency in package.json and env variables for
            // configuring application insights has not been set properly in portal

            // Arrange
            var appName = "linxnodeexpress";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var imageName = _imageHelper.GetRuntimeImage("node", nodeVersion, osType);
            var aIEnabled = ExtVarNames.UserAppInsightsAgentExtensionVersion;
            int containerDebugPort = 8080;

            var script = new ShellScriptBuilder()
                .AddCommand($"export {aIEnabled}=disabled")
                .AddCommand($"cd {appDir}")
                .AddCommand("npm install")
                .AddCommand($"oryx create-script -appPath {appDir}")
                .AddDirectoryExistsCheck($"{appDir}/node_modules")
                .AddDirectoryDoesNotExistCheck($"{appDir}/node_modules/applicationinsights")
                .AddCommand("./run.sh")
                .AddFileDoesNotExistCheck($"{appDir}/oryx-appinsightsloader.js")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                port: containerDebugPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", script },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World from express!", data);
                },
                dockerCli: _dockerCli);
        }

        [Theory]
        [Trait("category", "runtime-buster")]
        [MemberData(
           nameof(TestValueGenerator.GetBusterNodeVersions),
           MemberType = typeof(TestValueGenerator))]
        public async Task GeneratesScriptForBuster_CanRun_With_New_AppInsights_Env_Variable_Set_Async(string nodeVersion, string osType)
        {
            // This test is for the following scenario:
            // When we find the user has set env variable "APPLICATIONINSIGHTS_CONNECTION_STRING" application insight dependency in package.json and env variables for
            // configuring application insights has not been set properly in portal

            // Arrange
            var appName = "linxnodeexpress";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var imageName = _imageHelper.GetRuntimeImage("node", nodeVersion, osType);
            var aIEnabled = ExtVarNames.UserAppInsightsAgentExtensionVersion;
            var connectionStringEnv = ExtVarNames.UserAppInsightsConnectionStringEnv;
            int containerDebugPort = 8080;
            var AppInsightsStartUpLegacyPayLoadMessage = "Application Insights was started with setupString";

            var script = new ShellScriptBuilder()
                .AddCommand($"export {aIEnabled}=Enabled")
                .AddCommand($"export {connectionStringEnv}=alkajsldkajd")
                .AddCommand($"cd {appDir}")
                .AddCommand("npm install")
                .AddCommand($"oryx create-script -appPath {appDir}")
                .AddDirectoryExistsCheck($"{appDir}/node_modules")
                .AddDirectoryDoesNotExistCheck($"{appDir}/node_modules/applicationinsights")
                .AddCommand($"./run.sh > {appDir}/log.log")
                .AddFileDoesNotExistCheck($"{appDir}/oryx-appinsightsloader.js")
                .AddStringDoesNotExistInFileCheck(AppInsightsStartUpLegacyPayLoadMessage, $"{appDir}/log.log")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                port: containerDebugPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", script },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World from express!", data);
                },
                dockerCli: _dockerCli);
        }

        [Theory]
        [Trait("category", "runtime-bullseye")]
        [MemberData(
           nameof(TestValueGenerator.GetBullseyeNodeVersions),
           MemberType = typeof(TestValueGenerator))]
        public async Task GeneratesScriptForBullseye_CanRun_With_New_AppInsights_Env_Variable_Set_Async(string nodeVersion, string osType)
        {
            // This test is for the following scenario:
            // When we find the user has set env variable "APPLICATIONINSIGHTS_CONNECTION_STRING" application insight dependency in package.json and env variables for
            // configuring application insights has not been set properly in portal

            // Arrange
            var appName = "linxnodeexpress";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var imageName = _imageHelper.GetRuntimeImage("node", nodeVersion, osType);
            var aIEnabled = ExtVarNames.UserAppInsightsAgentExtensionVersion;
            var connectionStringEnv = ExtVarNames.UserAppInsightsConnectionStringEnv;
            int containerDebugPort = 8080;
            var AppInsightsStartUpLegacyPayLoadMessage = "Application Insights was started with setupString";

            var script = new ShellScriptBuilder()
                .AddCommand($"export {aIEnabled}=Enabled")
                .AddCommand($"export {connectionStringEnv}=alkajsldkajd")
                .AddCommand($"cd {appDir}")
                .AddCommand("npm install")
                .AddCommand($"oryx create-script -appPath {appDir}")
                .AddDirectoryExistsCheck($"{appDir}/node_modules")
                .AddDirectoryDoesNotExistCheck($"{appDir}/node_modules/applicationinsights")
                .AddCommand($"./run.sh > {appDir}/log.log")
                .AddFileDoesNotExistCheck($"{appDir}/oryx-appinsightsloader.js")
                .AddStringDoesNotExistInFileCheck(AppInsightsStartUpLegacyPayLoadMessage, $"{appDir}/log.log")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                port: containerDebugPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", script },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World from express!", data);
                },
                dockerCli: _dockerCli);
        }

        [Theory]
        [Trait("category", "runtime-bookworm")]
        [MemberData(
           nameof(TestValueGenerator.GetBookwormNodeVersions),
           MemberType = typeof(TestValueGenerator))]
        public async Task GeneratesScriptForBookworm_CanRun_With_New_AppInsights_Env_Variable_Set_Async(string nodeVersion, string osType)
        {
            // This test is for the following scenario:
            // When we find the user has set env variable "APPLICATIONINSIGHTS_CONNECTION_STRING" application insight dependency in package.json and env variables for
            // configuring application insights has not been set properly in portal

            // Arrange
            var appName = "linxnodeexpress";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var imageName = _imageHelper.GetRuntimeImage("node", nodeVersion, osType);
            var aIEnabled = ExtVarNames.UserAppInsightsAgentExtensionVersion;
            var connectionStringEnv = ExtVarNames.UserAppInsightsConnectionStringEnv;
            int containerDebugPort = 8080;
            var AppInsightsStartUpLegacyPayLoadMessage = "Application Insights was started with setupString";

            var script = new ShellScriptBuilder()
                .AddCommand($"export {aIEnabled}=Enabled")
                .AddCommand($"export {connectionStringEnv}=alkajsldkajd")
                .AddCommand($"cd {appDir}")
                .AddCommand("npm install")
                .AddCommand($"oryx create-script -appPath {appDir}")
                .AddDirectoryExistsCheck($"{appDir}/node_modules")
                .AddDirectoryDoesNotExistCheck($"{appDir}/node_modules/applicationinsights")
                .AddCommand($"./run.sh > {appDir}/log.log")
                .AddFileDoesNotExistCheck($"{appDir}/oryx-appinsightsloader.js")
                .AddStringDoesNotExistInFileCheck(AppInsightsStartUpLegacyPayLoadMessage, $"{appDir}/log.log")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                port: containerDebugPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", script },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World from express!", data);
                },
                dockerCli: _dockerCli);
        }

        [Theory]
        [Trait("category", "runtime-buster")]
        [InlineData("14", "")]
        [InlineData("14", "disabled")]
        public async Task GeneratesScriptForBuster_Doesnot_Add_Oryx_AppInsights_Logic_With_IPA_Configuration_Async(
            string nodeVersion,
            string agentExtensionVersionEnvValue)
        {
            // This test is for the following scenario:
            // When we find the user has set env variable "ApplicationInsightsAgent_EXTENSION_VERSION" to '~3' 
            // Oryx should not attach appinsight codeless config to runscript

            // Arrange
            var appName = "linxnodeexpress-appinsights";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var imageName = _imageHelper.GetRuntimeImage("node", nodeVersion, ImageTestHelperConstants.OsTypeDebianBuster);
            //agentextension version will be set to '~3' or '' or 'disabled'
            var agentExtensionVersionEnv = ExtVarNames.UserAppInsightsAgentExtensionVersion;
            var connectionStringEnv = ExtVarNames.UserAppInsightsConnectionStringEnv;
            int containerDebugPort = 8080;
            var OryxAppInsightsAttachString = "--require /usr/local/lib/node_modules/applicationinsights/out/Bootstrap/Oryx.js";

            var script = new ShellScriptBuilder()
                .AddCommand($"export {agentExtensionVersionEnv}={agentExtensionVersionEnvValue}")
                .AddCommand($"export {connectionStringEnv}=alkajsldkajd")
                .AddCommand($"cd {appDir}")
                .AddCommand("npm install")
                .AddCommand($"oryx create-script -appPath {appDir}")
                .AddDirectoryExistsCheck($"{appDir}/node_modules")
                .AddCommand($"./run.sh > {appDir}/log.log")
                .AddStringDoesNotExistInFileCheck(OryxAppInsightsAttachString, $"{appDir}/run.sh")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion, ImageTestHelperConstants.OsTypeDebianBuster),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                port: containerDebugPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", script },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("AppInsights is not configured!", data);
                },
                dockerCli: _dockerCli);
        }

        [Theory]
        [Trait("category", "runtime-bullseye")]
        [InlineData("14", "")]
        [InlineData("14", "disabled")]
        public async Task GeneratesScriptForBullseye_Doesnot_Add_Oryx_AppInsights_Logic_With_IPA_Configuration_Async(
            string nodeVersion,
            string agentExtensionVersionEnvValue)
        {
            // This test is for the following scenario:
            // When we find the user has set env variable "ApplicationInsightsAgent_EXTENSION_VERSION" to '~3' 
            // Oryx should not attach appinsight codeless config to runscript

            // Arrange
            var appName = "linxnodeexpress-appinsights";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var imageName = _imageHelper.GetRuntimeImage("node", nodeVersion, ImageTestHelperConstants.OsTypeDebianBullseye);
            //agentextension version will be set to '~3' or '' or 'disabled'
            var agentExtensionVersionEnv = ExtVarNames.UserAppInsightsAgentExtensionVersion;
            var connectionStringEnv = ExtVarNames.UserAppInsightsConnectionStringEnv;
            int containerDebugPort = 8080;
            var OryxAppInsightsAttachString = "--require /usr/local/lib/node_modules/applicationinsights/out/Bootstrap/Oryx.js";

            var script = new ShellScriptBuilder()
                .AddCommand($"export {agentExtensionVersionEnv}={agentExtensionVersionEnvValue}")
                .AddCommand($"export {connectionStringEnv}=alkajsldkajd")
                .AddCommand($"cd {appDir}")
                .AddCommand("npm install")
                .AddCommand($"oryx create-script -appPath {appDir}")
                .AddDirectoryExistsCheck($"{appDir}/node_modules")
                .AddCommand($"./run.sh > {appDir}/log.log")
                .AddStringDoesNotExistInFileCheck(OryxAppInsightsAttachString, $"{appDir}/run.sh")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion, ImageTestHelperConstants.OsTypeDebianBullseye),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                port: containerDebugPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", script },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("AppInsights is not configured!", data);
                },
                dockerCli: _dockerCli);
        }

    }
}
