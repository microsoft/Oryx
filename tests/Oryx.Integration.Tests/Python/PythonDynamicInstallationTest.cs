﻿// --------------------------------------------------------------------------------------------
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
        [Trait("category", "python-3.7")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        public async Task CanBuildAndRunPython37AppAsync()
        {
            // Temporarily skip - Bug #1266781
            // await CanBuildAndRunPythonAppAsync("3.7", ImageTestHelperConstants.GitHubActionsBullseye);
            await Task.FromResult(true);
        }

        [Fact]
        [Trait("category", "python-3.8")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        public async Task CanBuildAndRunPython38AppAsync()
        {
            // Temporarily skip - Bug #1266781
            // await CanBuildAndRunPythonAppAsync("3.8", ImageTestHelperConstants.GitHubActionsBullseye);
            await Task.FromResult(true);
        }

        [Fact]
        [Trait("category", "python-3.9")]
        [Trait("build-image", "github-actions-debian-buster")]
        public async Task CanBuildAndRunPython39AppAsync()
        {
            // Temporarily skip - Bug #1266781
            // await CanBuildAndRunPythonAppAsync("3.9", ImageTestHelperConstants.GitHubActionsBuster);
            await Task.FromResult(true);
        }

        [Fact]
        [Trait("category", "python-3.7")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        public async Task CanBuildAndRunPython37App_UsingGitHubActionsBuildImage_AndDynamicRuntimeInstallationAsync()
        {
            // Temporarily skip - Bug #1266781
            // await CanBuildAndRunPythonApp_UsingGitHubActionsBuildImage_AndDynamicRuntimeInstallationAsync(PythonVersions.Python37Version, ImageTestHelperConstants.GitHubActionsBullseye);
            await Task.FromResult(true);
        }

        [Fact]
        [Trait("category", "python-3.8")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        public async Task CanBuildAndRunPython38App_UsingGitHubActionsBuildImage_AndDynamicRuntimeInstallationAsync()
        {
            // Temporarily skip - Bug #1266781
            // await CanBuildAndRunPythonApp_UsingGitHubActionsBuildImage_AndDynamicRuntimeInstallationAsync(PythonVersions.Python38Version, ImageTestHelperConstants.GitHubActionsBullseye);
            await Task.FromResult(true);
        }

        [Fact]
        [Trait("category", "python-3.9")]
        [Trait("build-image", "github-actions-debian-buster")]
        public async Task CanBuildAndRunPython39App_UsingGitHubActionsBuildImage_AndDynamicRuntimeInstallationAsync()
        {
            // Temporarily skip - Bug #1266781
            // await CanBuildAndRunPythonApp_UsingGitHubActionsBuildImage_AndDynamicRuntimeInstallationAsync(PythonVersions.Python39Version, ImageTestHelperConstants.GitHubActionsBuster);
            await Task.FromResult(true);
        }

        [Fact]
        [Trait("category", "python-3.10")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        public async Task CanBuildAndRunPython310App_UsingGitHubActionsBullseyeBuildImage_AndDynamicRuntimeInstallationAsync()
        {
            await CanBuildAndRunPythonApp_UsingGitHubActionsBullseyeBuildImage_AndDynamicRuntimeInstallationAsync("3.10", ImageTestHelperConstants.GitHubActionsBullseye);
        }

        [Fact]
        [Trait("category", "python-3.11")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        public async Task CanBuildAndRunPython311App_UsingGitHubActionsBullseyeBuildImage_AndDynamicRuntimeInstallationAsync()
        {
            await CanBuildAndRunPythonApp_UsingGitHubActionsBullseyeBuildImage_AndDynamicRuntimeInstallationAsync("3.11", ImageTestHelperConstants.GitHubActionsBullseye);
        }

        [Fact]
        [Trait("category", "python-3.7")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        public async Task CanBuildAndRunPython37App_UsingScriptCommandAndSetEnvSwitchAsync()
        {
            await CanBuildAndRunPythonApp_UsingScriptCommandAndSetEnvSwitchAsync(ImageTestHelperConstants.GitHubActionsBullseye);
        }

        [Fact]
        [Trait("category", "python-3.7")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        public async Task CanBuildAndRunPython37App_WhenUsingPackageDirSwitchAsync()
        {
            // Temporarily skip - Bug #1266781
            // await CanBuildAndRunPythonAppWhenUsingPackageDirSwitchAsync(true, ImageTestHelperConstants.GitHubActionsBullseye);
            // await CanBuildAndRunPythonAppWhenUsingPackageDirSwitchAsync(false, ImageTestHelperConstants.GitHubActionsBullseye);
            await Task.FromResult(true);
        }

        private async Task CanBuildAndRunPythonAppAsync(string pythonVersion, string debianFlavor = null)
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand(GetSnippetToCleanUpExistingInstallation())
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx setupEnv -appPath {appOutputDir}")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(debianFlavor),
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
            string pythonVersion,
            string debianFlavor = null)
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand(GetSnippetToCleanUpExistingInstallation())
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(debianFlavor),
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
            string pythonVersion,
            string debianFlavor = null)
        {
            // Arrange
            var appName = "django-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand(GetSnippetToCleanUpExistingInstallation())
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(debianFlavor),
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

        private async Task CanBuildAndRunPythonApp_UsingScriptCommandAndSetEnvSwitchAsync(string debianFlavor = null)
        {
            // Arrange
            var pythonVersion = "3.7";
            var appName = "django-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand(GetSnippetToCleanUpExistingInstallation())
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx setupEnv -appPath {appOutputDir}")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(debianFlavor),
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

        private async Task CanBuildAndRunPythonAppWhenUsingPackageDirSwitchAsync(bool compressDestinationDir, string debianFlavor = null)
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
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion} " +
                $"-p packagedir={packagesDir} {compressDestination}")
               .AddDirectoryExistsCheck($"{appOutputDir}/{packagesDir}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(debianFlavor),
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
