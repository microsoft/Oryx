// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Ruby;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "ruby")]
    public class RubyDynamicInstallationTest : RubyEndToEndTestsBase
    {
        private readonly string DefaultSdksRootDir = "/opt/ruby";

        public RubyDynamicInstallationTest(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Theory]
        [InlineData("2.5")]
        [InlineData("2.6")]
        [InlineData("2.7")]
        public async Task CanBuildAndRunRubyApp(string rubyVersion)
        {
            // Arrange
            var appName = "sinatra-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddDefaultTestEnvironmentVariables()
               .AddCommand(GetSnippetToCleanUpExistingInstallation())
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {RubyConstants.PlatformName} --platform-version {rubyVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddCommand($"oryx setupEnv -appPath {appOutputDir}")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetLtsVersionsBuildImage(),
                "/bin/bash", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("ruby", rubyVersion),
                ContainerPort,
                "/bin/bash",
                new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Theory]
        [InlineData(RubyVersions.Ruby25Version)]
        [InlineData(RubyVersions.Ruby26Version)]
        [InlineData(RubyVersions.Ruby27Version)]
        public async Task CanBuildAndRunRubyApp_UsingGitHubActionsBuildImage_AndDynamicRuntimeInstallation(
            string rubyVersion)
        {
            // Arrange
            var appName = "sinatra-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddDefaultTestEnvironmentVariables()
               .AddCommand(GetSnippetToCleanUpExistingInstallation())
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {RubyConstants.PlatformName} --platform-version {rubyVersion}")
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
                _imageHelper.GetGitHubActionsBuildImage(),
                "/bin/bash", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("ruby", "dynamic"),
                ContainerPort,
                "/bin/bash",
                new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRunRubyApp_UsingScriptCommandAndSetEnvSwitch()
        {
            // Arrange
            var rubyVersion = "2.7";
            var appName = "sinatra-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddDefaultTestEnvironmentVariables()
               .AddCommand(GetSnippetToCleanUpExistingInstallation())
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {RubyConstants.PlatformName} --platform-version {rubyVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddCommand($"oryx setupEnv -appPath {appOutputDir}")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetLtsVersionsBuildImage(),
                "/bin/bash", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("ruby", "dynamic"),
                ContainerPort,
                "/bin/bash",
                new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        private string GetSnippetToCleanUpExistingInstallation()
        {
            return $"rm -rf {DefaultSdksRootDir}; mkdir -p {DefaultSdksRootDir}";
        }
    }
}
