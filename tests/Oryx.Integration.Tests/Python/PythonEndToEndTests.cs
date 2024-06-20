// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public class PythonEndToEndTests : PythonEndToEndTestsBase
    {
        public PythonEndToEndTests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        [Trait("category", "python-3.7")]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRun_Tweeter3AppAsync()
        {
            // Arrange
            var version = "3.7";
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "tweeter3";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(EnvironmentSettingsKeys.PostBuildCommand, "scripts/postbuild.sh")
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {version}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("python", version, osType),
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("logged in as: bob", data);
                });
        }

        [Fact]
        [Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        public async Task CanBuildAndRun_DjangoRegex_OnBullseyeBuildImage()
        {
            // Arrange
            var version = "3.11";
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "django-regex-example-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {PythonVersions.Python311Version}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye),
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("python", version, osType),
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello world from Django!", data);
                });
        }

        [Fact]
        [Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-debian-bookworm")]
        public async Task CanBuildAndRun_DjangoRegex_OnBookwormBuildImage()
        {
            // Arrange
            var version = "3.11";
            var osType = ImageTestHelperConstants.OsTypeDebianBookworm;
            var appName = "django-regex-example-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {PythonVersions.Python311Version}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBookworm),
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("python", version, osType),
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello world from Django!", data);
                });
        }

        [Fact]
        [Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        public async Task CanBuildAndRun_DjangoRegex_Python12_OnBullseyeBuildImage()
        {
            // Arrange
            var version = "3.12";
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "django-regex-example-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {PythonVersions.Python312Version}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye),
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("python", version, osType),
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello world from Django!", data);
                });
        }

        [Fact]
        [Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-debian-bookworm")]
        public async Task CanBuildAndRun_DjangoRegex_Python12_OnBookwormBuildImage()
        {
            // Arrange
            var version = "3.12";
            var osType = ImageTestHelperConstants.OsTypeDebianBookworm;
            var appName = "django-regex-example-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {PythonVersions.Python312Version}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBookworm),
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("python", version, osType),
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello world from Django!", data);
                });
        }

        [Theory]
        [Trait("category", "python-3.7")]
        [Trait("build-image", "debian-stretch")]
        [InlineData("3.7", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("3.7", ImageTestHelperConstants.OsTypeDebianBullseye)]
        public async Task BuildWithVirtualEnv_RemovesOryxPackagesDir_FromOlderBuildAsync(string pythonVersion, string osType)
        {
            // Arrange
            var appName = "django-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            const string virtualEnvName = "antenv";

            // Simulate apps that were built using package directory, and then virtual env
            var buildScript = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} -p virtualenv_name={virtualEnvName} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
                .ToString();

            var runScript = new ShellScriptBuilder()
                .AddDirectoryDoesNotExistCheck("__oryx_packages__")
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort} -virtualEnvName={virtualEnvName}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/bash",
                new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("python", pythonVersion, osType),
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

        [Fact(Skip = "Bug #1410367")]
        [Trait("category", "python-3.7")]
        [Trait("build-image", "debian-stretch")]
        public async Task BuildWithVirtualEnv_From_File_Requirement_TxtAsync_WithPython37()
        {
            await BuildWithVirtualEnv_From_File_Requirement_TxtAsync("3.7", ImageTestHelperConstants.OsTypeDebianBullseye);
        }

        [Fact(Skip = "Bug #1410367")]
        [Trait("category", "python-3.8")]
        [Trait("build-image", "debian-stretch")]
        public async Task BuildWithVirtualEnv_From_File_Requirement_TxtAsync_WithPython38()
        {
            await BuildWithVirtualEnv_From_File_Requirement_TxtAsync("3.8", ImageTestHelperConstants.OsTypeDebianBullseye);
        }

        private async Task BuildWithVirtualEnv_From_File_Requirement_TxtAsync(string pythonVersion, string osType)
        {
             // This is to test if we can build and run an app when both the files requirement.txt 
             // and setup.py are provided, we tend to prioritize the root level requirement.txt
            
            // Arrange
            var appName = "flask-setup-py-requirement-txt";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion}")
                .ToString();

            var runScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/bash",
                new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("python", pythonVersion, osType),
                ContainerPort,
                "/bin/bash",
                new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact(Skip = "Bug #1410367") ]
        [Trait("category", "python-3.8")]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRunPythonApp_UsingOutputDirectory_NestedUnderSourceDirectoryAsync()
        {
            // Arrange
            var version = "3.8";
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir}" +
               $" --platform {PythonConstants.PlatformName} --platform-version {version}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("python", version, osType),
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact (Skip = "Bug #1410367")]
        [Trait("category", "python-3.8")]
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRunPythonApp_UsingIntermediateDir_AndNestedOutputDirectoryAsync()
        {
            // Arrange
            var version = "3.8";
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {version}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("python", version, osType),
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        [Trait("category", "python-3.11")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        public async Task CanBuildAndRun_FastAPI()
        {
            // Arrange
            var version = "3.11";
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "fastapi-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {PythonVersions.Python311Version}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye),
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("python", version, osType),
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("{\"Hello\":\"World\"}", data);
                });
        }

        [Fact]
        [Trait("category", "python-3.12")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        public async Task CanBuildAndRun_FastAPI_Python12()
        {
            // Arrange
            var version = "3.12";
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "fastapi-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {PythonVersions.Python312Version}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye),
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("python", version, osType),
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("{\"Hello\":\"World\"}", data);
                });
        }
    }
}