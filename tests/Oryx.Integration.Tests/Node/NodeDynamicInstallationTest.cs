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

        [Fact]
        [Trait("category", "node-14-gh-buster")]
        [Trait("build-image", "github-actions-debian-buster")]
        public async Task CanBuildAndRunNode14AppUsingDynamicInstallationOfRuntimeInRuntimeImageAsync()
        {
            await CanBuildAndRunAppUsingDynamicInstallationOfRuntimeInRuntimeImageAsync(NodeVersions.Node14Version);
        }

        // [Fact]
        // [Trait("category", "node-16")]
        // [Trait("build-image", "github-actions-debian-buster")]
        // public async Task CanBuildAndRunNode16AppUsingDynamicInstallationOfRuntimeInRuntimeImageAsync()
        // {
        //     await CanBuildAndRunAppUsingDynamicInstallationOfRuntimeInRuntimeImageAsync(NodeVersions.Node16Version);
        // }

        private async Task CanBuildAndRunAppUsingDynamicInstallationOfRuntimeInRuntimeImageAsync(string nodeVersion)
        {
            // Arrange
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBuster),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("node", "dynamic", ImageTestHelperConstants.OsTypeDebianBuster),
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
        [Trait("category", "node-14-gh-buster")]
        [Trait("build-image", "github-actions-debian-buster")]
        public async Task CanBuildAndRunNode14App_UsingScriptCommandAsync()
        {
            await CanBuildAndRunApp_UsingScriptCommandAsync(NodeVersions.Node14Version);
        }

        [Fact]
        [Trait("category", "node-16")]
        [Trait("build-image", "github-actions-debian-buster")]
        public async Task CanBuildAndRunNode16App_UsingScriptCommandAsync()
        {
            await CanBuildAndRunApp_UsingScriptCommandAsync(NodeVersions.Node16Version);
        }

        private async Task CanBuildAndRunApp_UsingScriptCommandAsync(string nodeVersion)
        {
            // Arrange
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(SettingsKeys.EnableDynamicInstall, true.ToString())
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBuster),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("node", "dynamic", ImageTestHelperConstants.OsTypeDebianBuster),
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
        [InlineData(NodeVersions.Node18Version), Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        public async Task CanBuildAndRunApp_UsingScriptCommand_WithBullseyeBasedImages(string nodeVersion)
        {
            // Arrange
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(SettingsKeys.EnableDynamicInstall, true.ToString())
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("node", "18", ImageTestHelperConstants.OsTypeDebianBullseye),
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
        [InlineData(NodeVersions.Node20Version), Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        public async Task CanBuildAndRunNode20App_UsingScriptCommand_WithBullseyeBasedImages(string nodeVersion)
        {
            // Arrange
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(SettingsKeys.EnableDynamicInstall, true.ToString())
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("node", "20", ImageTestHelperConstants.OsTypeDebianBullseye),
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
        [InlineData(NodeVersions.Node20Version), Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-debian-bookworm")]
        public async Task CanBuildAndRunNode20App_UsingScriptCommand_WithBookwormBasedImages(string nodeVersion)
        {
            // Arrange
            var appName = "webfrontend";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {NodeConstants.PlatformName} --platform-version {nodeVersion}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(SettingsKeys.EnableDynamicInstall, true.ToString())
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBookworm),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("node", "20", ImageTestHelperConstants.OsTypeDebianBookworm),
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