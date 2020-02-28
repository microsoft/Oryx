// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "node")]
    public class NodeDynamicInstallationTest : NodeEndToEndTestsBase
    {
        private readonly string DefaultSdksRootDir = "/tmp/oryx/nodejs";

        public NodeDynamicInstallationTest(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Theory]
        [InlineData(NodeVersions.Node10Version)]
        [InlineData(NodeVersions.Node12Version)]
        public async Task CanBuildAndRunApp_AfterSettingUpEnvironmentExplicitly(string nodeVersion)
        {
            // Arrange
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
               .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
               .AddCommand(
                $"oryx build {appDir} --platform nodejs --language-version {nodeVersion} --enable-dynamic-install")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddCommand($"oryx setupEnv -appPath {appDir}")
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume },
                _imageHelper.GetTestSlimBuildImage(),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetTestRuntimeImage("node", "dynamic"),
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
        [InlineData(NodeVersions.Node10Version)]
        [InlineData(NodeVersions.Node12Version)]
        public async Task CanBuildAndRunApp_UsingScriptCommand(string nodeVersion)
        {
            // Arrange
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
               .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
               .AddCommand(
                $"oryx build {appDir} --platform nodejs --language-version {nodeVersion} --enable-dynamic-install")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort} -enableDynamicInstall")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume },
                _imageHelper.GetTestSlimBuildImage(),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetTestRuntimeImage("node", "dynamic"),
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