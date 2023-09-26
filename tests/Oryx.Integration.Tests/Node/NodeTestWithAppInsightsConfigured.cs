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
    [Trait("category", "node-14-stretch-2")]
    public class NodeTestWithAppInsightsConfigured : NodeEndToEndTestsBase
    {
        public NodeTestWithAppInsightsConfigured(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Theory]
        [Trait("build-image", "debian-stretch")]
        [InlineData("14", ImageTestHelperConstants.OsTypeDebianBuster, "~2", ExtVarNames.UserAppInsightsConnectionStringEnv, TestConstants.AppInsightsConnectionString)]
        [InlineData("14", ImageTestHelperConstants.OsTypeDebianBuster, "enabled", ExtVarNames.UserAppInsightsConnectionStringEnv, "InstrumentationKey=value1;key2=value2;key3=value3")]
        [InlineData("14", ImageTestHelperConstants.OsTypeDebianBullseye, "~2", ExtVarNames.UserAppInsightsConnectionStringEnv, TestConstants.AppInsightsConnectionString)]
        [InlineData("14", ImageTestHelperConstants.OsTypeDebianBullseye, "enabled", ExtVarNames.UserAppInsightsConnectionStringEnv, "InstrumentationKey=value1;key2=value2;key3=value3")]
        //Without pre-IPA bits of appInsights, UserAppInsightsExtensionVersion value will be '~2'
        // and that will enable oryx's appInsight attach logic
        public async Task CanBuildAndRun_App_With_AgentExtension_And_InstrumentKey_Or_ConnectionStringAsync(
            string nodeVersion,
            string osType,
            string agentExtensionVersionEnvValue,
            string appInsightKeyOrConnectionString,
            string envVarValue)
        {
            // Arrange
            var appName = "linxnodeexpress-appinsights";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var spcifyNodeVersionCommand = $"--platform {NodeConstants.PlatformName} --platform-version=" + nodeVersion;
            var aiConnectionString = appInsightKeyOrConnectionString;
            var aIEnabled = ExtVarNames.UserAppInsightsAgentExtensionVersion;
            var OryxAppInsightsAttachString = "--require /usr/local/lib/node_modules/applicationinsights/out/Bootstrap/Oryx.js";

            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} {spcifyNodeVersionCommand} --log-file {appDir}/1.log")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules").ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"export {aIEnabled}={agentExtensionVersionEnvValue}")
                .AddCommand($"export {aiConnectionString}={TestConstants.AppInsightsConnectionString}")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .AddFileExistsCheck($"{FilePaths.NodeGlobalModulesPath}/{FilePaths.NodeAppInsightsLoaderFileName}")
                .AddStringExistsInFileCheck(OryxAppInsightsAttachString, $"{appOutputDir}/run.sh")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { volume, appOutputDirVolume },
                Settings.BuildImageName,
                "/bin/bash",
                 new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                new List<EnvironmentVariable> { new EnvironmentVariable(aiConnectionString, envVarValue), new EnvironmentVariable(aIEnabled, "~2") },
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
        [Trait("build-image", "debian-stretch")]
        [InlineData("14", ImageTestHelperConstants.OsTypeDebianBuster, "~3", ExtVarNames.UserAppInsightsConnectionStringEnv, "InstrumentationKey=value1;key2=value2;key3=value3")]
        [InlineData("14", ImageTestHelperConstants.OsTypeDebianBuster, "", ExtVarNames.UserAppInsightsConnectionStringEnv, "InstrumentationKey=value1;key2=value2;key3=value3")]
        [InlineData("14", ImageTestHelperConstants.OsTypeDebianBuster, "disabled", ExtVarNames.UserAppInsightsConnectionStringEnv, "InstrumentationKey=value1;key2=value2;key3=value3")]
        [InlineData("14", ImageTestHelperConstants.OsTypeDebianBullseye, "~3", ExtVarNames.UserAppInsightsConnectionStringEnv, "InstrumentationKey=value1;key2=value2;key3=value3")]
        [InlineData("14", ImageTestHelperConstants.OsTypeDebianBullseye, "", ExtVarNames.UserAppInsightsConnectionStringEnv, "InstrumentationKey=value1;key2=value2;key3=value3")]
        [InlineData("14", ImageTestHelperConstants.OsTypeDebianBullseye, "disabled", ExtVarNames.UserAppInsightsConnectionStringEnv, "InstrumentationKey=value1;key2=value2;key3=value3")]
        //With New IPA bits of appInsights, UserAppInsightsExtensionVersion value will be '~3'
        // and that will disable oryx's appInsight attach logic
        public async Task CanBuildAndRun_NodeApp_AppInsights_With_NewIPA_ConfigurationAsync(
            string nodeVersion,
            string osType,
            string agentExtensionVersionEnvValue,
            string appInsightKeyOrConnectionString,
            string envVarValue)
        {
            // Arrange
            var appName = "linxnodeexpress-appinsights";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var spcifyNodeVersionCommand = $"--platform {NodeConstants.PlatformName} --platform-version=" + nodeVersion;
            var aiConnectionString = appInsightKeyOrConnectionString;
            var aIEnabled = ExtVarNames.UserAppInsightsAgentExtensionVersion;
            var OryxAppInsightsAttachString = "--require /usr/local/lib/node_modules/applicationinsights/out/Bootstrap/Oryx.js";

            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} {spcifyNodeVersionCommand} --log-file {appOutputDir}/1.log")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules").ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"export {aIEnabled}={agentExtensionVersionEnvValue}")
                .AddCommand($"export {aiConnectionString}={envVarValue}")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .AddFileExistsCheck($"{FilePaths.NodeGlobalModulesPath}/{FilePaths.NodeAppInsightsLoaderFileName}")
                .AddStringDoesNotExistInFileCheck(OryxAppInsightsAttachString, $"{appOutputDir}/run.sh")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { volume, appOutputDirVolume },
                Settings.BuildImageName,
                "/bin/bash",
                 new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                new List<EnvironmentVariable> { new EnvironmentVariable(aiConnectionString, envVarValue), new EnvironmentVariable(aIEnabled, "~2") },
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