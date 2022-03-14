// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public class NodeDynamicInstallationTest : NodeEndToEndTestsBase
    {
        private readonly string DefaultSdksRootDir = "/tmp/oryx/nodejs";

        public NodeDynamicInstallationTest(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// IMPORTANT:
        /// New tests should be included in a corresponding
        /// method with attribute:
        ///     [Fact, Trait("category", "<platform>-<version>")]
        ///
        /// The pipeline will invoke these integration tests 
        /// on the matching category attribute.
        /// </summary>
        [Fact, Trait("category", "node-12")]
        public void PipelineTestInvocationsNode12()
        {
            string nodeVersion = "12";
            CanBuildAndRunAppUsingDynamicInstallationOfRuntimeInRuntimeImage(nodeVersion);
            CanBuildAndRunApp_UsingScriptCommand(nodeVersion);
        }

        [Fact, Trait("category", "node-14")]
        public void PipelineTestInvocationsNode14()
        {
            string nodeVersion = "14";
            CanBuildAndRunAppUsingDynamicInstallationOfRuntimeInRuntimeImage(nodeVersion);
            CanBuildAndRunApp_UsingScriptCommand(nodeVersion);
        }

        [Theory]
        [InlineData(NodeVersions.Node12Version)]
        [InlineData(NodeVersions.Node14Version)]
        public async Task CanBuildAndRunAppUsingDynamicInstallationOfRuntimeInRuntimeImage(string nodeVersion)
        {
            // Arrange
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddDefaultTestEnvironmentVariables()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetLtsVersionsBuildImage(),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("node", "dynamic"),
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
        [InlineData(NodeVersions.Node12Version)]
        [InlineData(NodeVersions.Node14Version)]
        public async Task CanBuildAndRunApp_UsingScriptCommand(string nodeVersion)
        {
            // Arrange
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddDefaultTestEnvironmentVariables()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .SetEnvironmentVariable(SettingsKeys.EnableDynamicInstall, true.ToString())
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetLtsVersionsBuildImage(),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("node", "dynamic"),
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

        private string GetSnippetToCleanUpExistingInstallation()
        {
            return $"rm -rf {DefaultSdksRootDir}; mkdir -p {DefaultSdksRootDir}";
        }
    }
}