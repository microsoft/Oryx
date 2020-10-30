// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Common;
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
        [InlineData("10", "~2", ExtVarNames.UserAppInsightsKeyEnv)]
        [InlineData("10", "enabled", ExtVarNames.UserAppInsightsConnectionStringEnv)]
        [InlineData("12", "~2", ExtVarNames.UserAppInsightsKeyEnv)]
        [InlineData("12", "enabled", ExtVarNames.UserAppInsightsConnectionStringEnv)]
        //Without pre-IPA bits of appInsights, UserAppInsightsExtensionVersion value will be '~2'
        // and that will enable oryx's appInsight attach logic
        public async Task CanBuildAndRun_App_With_AgentExtension_And_InstrumentKey_Or_ConnectionString(
            string nodeVersion,
            string agentExtensionVersionEnvValue,
            string appInsightKeyOrConnectionString)
        {
            // Arrange
            var appName = "linxnodeexpress-appinsights";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var spcifyNodeVersionCommand = $"--platform {NodeConstants.PlatformName} --platform-version=" + nodeVersion;
            var aIKey = appInsightKeyOrConnectionString;
            var aIEnabled = ExtVarNames.UserAppInsightsExtensionVersion;
            var OryxAppInsightsAttachString = "--require /usr/local/lib/node_modules/applicationinsights/out/Bootstrap/Oryx.js";

            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appDir} {spcifyNodeVersionCommand} --log-file {appDir}/1.log")
                .AddDirectoryExistsCheck($"{appDir}/node_modules").ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"export {aIEnabled}={agentExtensionVersionEnvValue}")
                .AddCommand($"export {aIKey}=asdas")
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .AddFileExistsCheck($"{FilePaths.NodeGlobalModulesPath}/{FilePaths.NodeAppInsightsLoaderFileName}")
                .AddStringExistsInFileCheck(OryxAppInsightsAttachString, $"{appDir}/run.sh")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion),
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

        [Theory]
        [InlineData("10", "~3", ExtVarNames.UserAppInsightsKeyEnv)]
        [InlineData("10", "~3", ExtVarNames.UserAppInsightsConnectionStringEnv)]
        [InlineData("12", "", ExtVarNames.UserAppInsightsKeyEnv)]
        [InlineData("12", "", ExtVarNames.UserAppInsightsConnectionStringEnv)]
        [InlineData("12", "disabled", ExtVarNames.UserAppInsightsKeyEnv)]
        [InlineData("12", "disabled", ExtVarNames.UserAppInsightsConnectionStringEnv)]
        //With New IPA bits of appInsights, UserAppInsightsExtensionVersion value will be '~3'
        // and that will disable oryx's appInsight attach logic
        public async Task CanBuildAndRun_NodeApp_AppInsights_With_NewIPA_Configuration(
            string nodeVersion, 
            string agentExtensionVersionEnvValue,
            string appInsightKeyOrConnectionString)
        {
            // Arrange
            var appName = "linxnodeexpress-appinsights";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var spcifyNodeVersionCommand = $"--platform {NodeConstants.PlatformName} --platform-version=" + nodeVersion;
            var aIKey = appInsightKeyOrConnectionString;
            var aIEnabled = ExtVarNames.UserAppInsightsExtensionVersion;
            var OryxAppInsightsAttachString = "--require /usr/local/lib/node_modules/applicationinsights/out/Bootstrap/Oryx.js";

            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appDir} {spcifyNodeVersionCommand} --log-file {appDir}/1.log")
                .AddDirectoryExistsCheck($"{appDir}/node_modules").ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"export {aIEnabled}={agentExtensionVersionEnvValue}")
                .AddCommand($"export {aIKey}=asdas")
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .AddFileExistsCheck($"{FilePaths.NodeGlobalModulesPath}/{FilePaths.NodeAppInsightsLoaderFileName}")
                .AddStringDoesNotExistInFileCheck(OryxAppInsightsAttachString, $"{appDir}/run.sh")
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
                _imageHelper.GetRuntimeImage("node", nodeVersion),
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
                    Assert.Contains("AppInsights is not configured!", data);
                });
        }
    }
}