// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Python;
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

        [Fact]
        [Trait("category", "python-37")]
        public async Task RunPython37IntegrationTests()
        {
            // Temporarily skip - Bug #1266781
            // await CanBuildAndRunPythonAppAsync("3.7");

            // Temporarily skip - Bug #1410367
            // await CanBuildAndRunPythonApp_UsingGitHubActionsBuildImage_AndDynamicRuntimeInstallationAsync("PythonVersions.Python37Version");

            await CanBuildAndRunPythonApp_UsingScriptCommandAndSetEnvSwitchAsync();

            // Temporarily skip - Bug #1266781
            // await CanBuildAndRunPythonAppWhenUsingPackageDirSwitchAsync(true);
            // await CanBuildAndRunPythonAppWhenUsingPackageDirSwitchAsync(false);
        }

        [Fact]
        [Trait("category", "python-38")]
        public async Task RunPython38IntegrationTests()
        {
            // Temporarily skip - Bug #1266781
            // await CanBuildAndRunPythonAppAsync("3.8");

            // Temporarily skip - Bug #1410367
            // await CanBuildAndRunPythonApp_UsingGitHubActionsBuildImage_AndDynamicRuntimeInstallationAsync("PythonVersions.Python38Version");
        }

        [Fact]
        [Trait("category", "python-39")]
        public async Task RunPython39IntegrationTests()
        {
            // Temporarily skip - Bug #1266781
            // await CanBuildAndRunPythonAppAsync("3.9");

            // Temporarily skip - Bug #1410367
            // await CanBuildAndRunPythonApp_UsingGitHubActionsBuildImage_AndDynamicRuntimeInstallationAsync("PythonVersions.Python39Version");
        }

        [Fact]
        [Trait("category", "python-310")]
        public async Task RunPython310IntegrationTests()
        {
            await CanBuildAndRunPythonApp_UsingGitHubActionsBullseyeBuildImage_AndDynamicRuntimeInstallationAsync("3.10");
        }

        [Fact]
        [Trait("category", "python-311")]
        public async Task RunPython311IntegrationTests()
        {
            await CanBuildAndRunPythonApp_UsingGitHubActionsBullseyeBuildImage_AndDynamicRuntimeInstallationAsync("3.11");
        }

        private async Task CanBuildAndRunPythonAppAsync(string pythonVersion)
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddDefaultTestEnvironmentVariables()
               .AddCommand(GetSnippetToCleanUpExistingInstallation())
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
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
                _imageHelper.GetRuntimeImage("python", pythonVersion),
                ContainerPort,
                "/bin/bash",
                new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        private async Task CanBuildAndRunPythonApp_UsingGitHubActionsBuildImage_AndDynamicRuntimeInstallationAsync(
            string pythonVersion)
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddDefaultTestEnvironmentVariables()
               .AddCommand(GetSnippetToCleanUpExistingInstallation())
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
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

        private async Task CanBuildAndRunPythonApp_UsingGitHubActionsBullseyeBuildImage_AndDynamicRuntimeInstallationAsync(
            string pythonVersion)
        {
            // Arrange
            var appName = "django-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddDefaultTestEnvironmentVariables()
               .AddCommand(GetSnippetToCleanUpExistingInstallation())
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
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
                _imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye),
                "/bin/bash", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("python", pythonVersion),
                ContainerPort,
                "/bin/bash",
                new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await GetResponseDataAsync($"http://localhost:{hostPort}/staticfiles/css/boards.css");
                    Assert.Contains("CSS file from Boards app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}/staticfiles/css/uservoice.css");
                    Assert.Contains("CSS file from UserVoice app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}/boards/");
                    Assert.Contains("Hello, World! from Boards app", data);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}/uservoice/");
                    Assert.Contains("Hello, World! from Uservoice app", data);
                });
        }

        private async Task CanBuildAndRunPythonApp_UsingScriptCommandAndSetEnvSwitchAsync()
        {
            // Arrange
            var pythonVersion = "3.7";
            var appName = "django-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddDefaultTestEnvironmentVariables()
               .AddCommand(GetSnippetToCleanUpExistingInstallation())
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
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
                _imageHelper.GetRuntimeImage("python", "dynamic"),
                ContainerPort,
                "/bin/bash",
                new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await GetResponseDataAsync($"http://localhost:{hostPort}/staticfiles/css/boards.css");
                    Assert.Contains("CSS file from Boards app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}/staticfiles/css/uservoice.css");
                    Assert.Contains("CSS file from UserVoice app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}/boards/");
                    Assert.Contains("Hello, World! from Boards app", data);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}/uservoice/");
                    Assert.Contains("Hello, World! from Uservoice app", data);
                });
        }

        private async Task CanBuildAndRunPythonAppWhenUsingPackageDirSwitchAsync(bool compressDestinationDir)
        {
            // Arrange
            var pythonVersion = "3.7";
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var packagesDir = ".python_packages/lib/python3.7/site-packages";
            var compressDestination = compressDestinationDir ? "--compress-destination-dir" : string.Empty;
            var buildScript = new ShellScriptBuilder()
               .AddDefaultTestEnvironmentVariables()
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion} " +
                $"-p packagedir={packagesDir} {compressDestination}")
               .AddDirectoryExistsCheck($"{appOutputDir}/{packagesDir}")
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
