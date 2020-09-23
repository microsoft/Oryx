// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public class PythonDynamicInstallationTest : PythonEndToEndTestsBase
    {
        private readonly string DefaultSdksRootDir = "/opt/python";

        public PythonDynamicInstallationTest(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Theory]
        [InlineData("2.7")]
        [InlineData("3")]
        [InlineData("3.6")]
        [InlineData("3.7")]
        public async Task CanBuildAndRunPythonApp(string pythonVersion)
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand(GetSnippetToCleanUpExistingInstallation())
               .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
               .AddCommand(
                $"oryx build {appDir} --platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
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
                _imageHelper.GetLtsVersionsBuildImage(),
                "/bin/bash", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("python", "dynamic"),
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
        [InlineData(PythonVersions.Python27Version)]
        [InlineData("3")]
        [InlineData(PythonVersions.Python37Version)]
        public async Task CanBuildAndRunPythonApp_UsingGitHubActionsBuildImage(string pythonVersion)
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand(GetSnippetToCleanUpExistingInstallation())
               .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
               .AddCommand(
                $"oryx build {appDir} --platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
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
                _imageHelper.GetGitHubActionsBuildImage(),
                "/bin/bash", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("python", "dynamic"),
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
        public async Task CanBuildAndRunPythonApp_UsingScriptCommandAndSetEnvSwitch()
        {
            // Arrange
            var pythonVersion = "3.7";
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand(GetSnippetToCleanUpExistingInstallation())
               .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
               .AddCommand(
                $"oryx build {appDir} --platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(SettingsKeys.EnableDynamicInstall, true.ToString())
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume },
                _imageHelper.GetLtsVersionsBuildImage(),
                "/bin/bash", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("python", "dynamic"),
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
        public async Task CanBuildAndRunPythonAppWhenUsingPackageDirSwitch()
        {
            // Arrange
            var pythonVersion = "3.7";
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var packagesDir = ".python_packages/lib/python3.7/site-packages";
            var buildScript = new ShellScriptBuilder()
               .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
               .AddCommand(
                $"oryx build {appDir} --platform {PythonConstants.PlatformName} --platform-version {pythonVersion} " +
                $"-p packagedir={packagesDir}")
               .AddDirectoryExistsCheck($"{appDir}/{packagesDir}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume },
                _imageHelper.GetGitHubActionsBuildImage(),
                "/bin/bash", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("python", "3.7"),
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
