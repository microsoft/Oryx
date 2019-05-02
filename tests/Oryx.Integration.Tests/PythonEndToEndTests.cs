// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "python")]
    public class PythonEndToEndTests : PlatformEndToEndTestsBase
    {
        private const int HostPort = Constants.PythonEndToEndTestsPort;
        private const int ContainerPort = 3000;
        private const string DefaultStartupFilePath = "./run.sh";

        private readonly ITestOutputHelper _output;
        private readonly string _hostSamplesDir;
        private readonly string _tempRootDir;

        public PythonEndToEndTests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
        {
            _output = output;
            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
            _tempRootDir = testTempDirTestFixture.RootDirPath;
        }

        [Fact]
        public async Task CanBuildAndRunPythonApp_UsingPython27_AndExplicitOutputStartupFile()
        {
            // Arrange
            var appName = "python2-flask-app";
            var hostDir = Path.Combine(_hostSamplesDir, "python", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var startupFile = "/tmp/startup.sh";
            var portMapping = $"{HostPort}:{ContainerPort}";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"oryx -appPath {appDir} -output {startupFile} -bindPort {ContainerPort}")
                .AddCommand(startupFile)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "oryx",
                new[] { "build", appDir, "-l", "python", "--language-version", "2.7" },
                "oryxdevms/python-2.7",
                portMapping,
                "/bin/bash",
                new[]
                {
                    "-c",
                    script
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_Python27App_UsingVirtualEnv()
        {
            // Arrange
            var appName = "python2-flask-app";
            var hostDir = Path.Combine(_hostSamplesDir, "python", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            const string virtualEnvName = "antenv2.7";
            var portMapping = $"{HostPort}:{ContainerPort}";
            var buildScript = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -l python --language-version 2.7 -p virtualenv_name={virtualEnvName}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                // Mimic the commands ran by app service in their derived image.
                .AddCommand("pip install gunicorn")
                .AddCommand("pip install flask")
                .AddCommand($"cd {appDir}")
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort} -virtualEnvName={virtualEnvName}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                "oryxdevms/python-2.7",
                portMapping,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRunPythonApp_UsingPython37()
        {
            // Arrange
            var appName = "flask-app";
            var hostDir = Path.Combine(_hostSamplesDir, "python", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "oryx",
                new[] { "build", appDir },
                "oryxdevms/python-3.7",
                portMapping,
                "/bin/bash",
                new[]
                {
                    "-c",
                    script
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRunPythonApp_UsingPython36()
        {
            // Arrange
            var appName = "flask-app";
            var hostDir = Path.Combine(_hostSamplesDir, "python", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "oryx",
                new[] { "build", appDir, "-l", "python", "--language-version", "3.6" },
                "oryxdevms/python-3.6",
                portMapping,
                "/bin/bash",
                new[]
                {
                    "-c",
                    script
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRunPythonApp_UsingPython37_AndVirtualEnv()
        {
            // Arrange
            var appName = "flask-app";
            var virtualEnvName = "antenv";
            var hostDir = Path.Combine(_hostSamplesDir, "python", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort} -virtualEnvName {virtualEnvName}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "oryx",
                new[] { "build", appDir, "-p", $"virtualenv_name={virtualEnvName}" },
                "oryxdevms/python-3.7",
                portMapping,
                "/bin/bash",
                new[]
                {
                    "-c",
                    script
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Theory]
        [InlineData("tar-gz", "tar.gz")]
        [InlineData("zip", "zip")]
        public async Task CanBuildAndRunPythonApp_UsingPython37_AndCompressedVirtualEnv(
            string compressOption,
            string expectedCompressFileNameExtension)
        {
            // Arrange
            var appName = "flask-app";
            var virtualEnvName = "antenv";
            var hostDir = Path.Combine(_hostSamplesDir, "python", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var appOutputDirPath = Directory.CreateDirectory(
                Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N"))).FullName;
            var appOutputDirVolume = DockerVolume.Create(appOutputDirPath);
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var tempOutputDir = "/tmp/output";
            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {tempOutputDir}" +
                $" -p virtualenv_name={virtualEnvName} -p compress_virtualenv={compressOption}")
                .AddDirectoryDoesNotExistCheck($"{tempOutputDir}/{virtualEnvName}")
                .AddFileExistsCheck($"{tempOutputDir}/{virtualEnvName}.{expectedCompressFileNameExtension}")
                .AddCommand($"cp -rf {tempOutputDir}/* {appOutputDir}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"oryx -appPath {appOutputDir} -bindPort {ContainerPort} -virtualEnvName {virtualEnvName}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { appOutputDirVolume, volume },
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                "oryxdevms/python-3.7",
                portMapping,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_DjangoApp_DoingCollectStaticByDefault()
        {
            // Arrange
            var appName = "django-app";
            var hostDir = Path.Combine(_hostSamplesDir, "python", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "oryx",
                new[] { "build", appDir },
                "oryxdevms/python-3.7",
                portMapping,
                "/bin/bash",
                new[]
                {
                    "-c",
                    script
                },
                async () =>
                {
                    var data = await GetResponseDataAsync($"http://localhost:{HostPort}/staticfiles/css/boards.css");
                    Assert.Contains("CSS file from Boards app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{HostPort}/staticfiles/css/uservoice.css");
                    Assert.Contains("CSS file from UserVoice app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{HostPort}/boards/");
                    Assert.Contains("Hello, World! from Boards app", data);

                    data = await GetResponseDataAsync($"http://localhost:{HostPort}/uservoice/");
                    Assert.Contains("Hello, World! from Uservoice app", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_DjangoPython37App_UsingVirtualEnv()
        {
            // Arrange
            var appName = "django-app";
            var hostDir = Path.Combine(_hostSamplesDir, "python", appName);
            var volume = DockerVolume.Create(hostDir);
            var appOutputDirPath = Directory.CreateDirectory(
                Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N"))).FullName;
            var appOutputDirVolume = DockerVolume.Create(appOutputDirPath);
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            const string virtualEnvName = "antenv";

            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o /tmp/out -l python --language-version 3.7 " +
                $"-p virtualenv_name={virtualEnvName}")
                .AddCommand($"cp -rf /tmp/out/* {appOutputDir}")
                .ToString();

            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"oryx -appPath {appOutputDir} -bindPort {ContainerPort} -virtualEnvName {virtualEnvName}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { volume, appOutputDirVolume },
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                "oryxdevms/python-3.7",
                portMapping,
                "/bin/bash",
                new[]
                {
                    "-c",
                    script
                },
                async () =>
                {
                    var data = await GetResponseDataAsync($"http://localhost:{HostPort}/staticfiles/css/boards.css");
                    Assert.Contains("CSS file from Boards app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{HostPort}/staticfiles/css/uservoice.css");
                    Assert.Contains("CSS file from UserVoice app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{HostPort}/boards/");
                    Assert.Contains("Hello, World! from Boards app", data);

                    data = await GetResponseDataAsync($"http://localhost:{HostPort}/uservoice/");
                    Assert.Contains("Hello, World! from Uservoice app", data);
                });
        }

        [Theory]
        [InlineData("3.6")]
        [InlineData("3.7")]
        public async Task BuildWithVirtualEnv_RemovesOryxPackagesDir_FromOlderBuild(string pythonVersion)
        {
            // Arrange
            var appName = "django-app";
            var hostDir = Path.Combine(_hostSamplesDir, "python", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            const string virtualEnvName = "antenv";

            // Simulate apps that were built using package directory, and then virtual env
            var buildScript = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -l python --language-version {pythonVersion}")
                .AddBuildCommand(
                $"{appDir} -p virtualenv_name={virtualEnvName} " +
                $"-l python --language-version {pythonVersion}")
                .ToString();

            var runScript = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddDirectoryDoesNotExistCheck("__oryx_packages__")
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort} -virtualEnvName={virtualEnvName}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/bash",
                new[] { "-c", buildScript },
                $"oryxdevms/python-{pythonVersion}",
                portMapping,
                "/bin/bash",
                new[] { "-c", runScript },
                async () =>
                {
                    var data = await GetResponseDataAsync($"http://localhost:{HostPort}/staticfiles/css/boards.css");
                    Assert.Contains("CSS file from Boards app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{HostPort}/staticfiles/css/uservoice.css");
                    Assert.Contains("CSS file from UserVoice app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{HostPort}/boards/");
                    Assert.Contains("Hello, World! from Boards app", data);

                    data = await GetResponseDataAsync($"http://localhost:{HostPort}/uservoice/");
                    Assert.Contains("Hello, World! from Uservoice app", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_DjangoPython36App_UsingVirtualEnv()
        {
            // Arrange
            var appName = "django-app";
            var hostDir = Path.Combine(_hostSamplesDir, "python", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            const string virtualEnvName = "antenv3.6";
            var buildScript = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -l python --language-version 3.6 -p virtualenv_name={virtualEnvName}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                "oryxdevms/python-3.6",
                portMapping,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
                async () =>
                {
                    var data = await GetResponseDataAsync($"http://localhost:{HostPort}/staticfiles/css/boards.css");
                    Assert.Contains("CSS file from Boards app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{HostPort}/staticfiles/css/uservoice.css");
                    Assert.Contains("CSS file from UserVoice app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{HostPort}/boards/");
                    Assert.Contains("Hello, World! from Boards app", data);

                    data = await GetResponseDataAsync($"http://localhost:{HostPort}/uservoice/");
                    Assert.Contains("Hello, World! from Uservoice app", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_DjangoApp_UsingPython36()
        {
            // Arrange
            var appName = "django-app";
            var hostDir = Path.Combine(_hostSamplesDir, "python", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "oryx",
                new[] { "build", appDir, "-l", "python", "--language-version", "3.6" },
                "oryxdevms/python-3.6",
                portMapping,
                "/bin/bash",
                new[]
                {
                    "-c",
                    script
                },
                async () =>
                {
                    var data = await GetResponseDataAsync($"http://localhost:{HostPort}/staticfiles/css/boards.css");
                    Assert.Contains("CSS file from Boards app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{HostPort}/staticfiles/css/uservoice.css");
                    Assert.Contains("CSS file from UserVoice app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{HostPort}/boards/");
                    Assert.Contains("Hello, World! from Boards app", data);

                    data = await GetResponseDataAsync($"http://localhost:{HostPort}/uservoice/");
                    Assert.Contains("Hello, World! from Uservoice app", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_Tweeter3App()
        {
            // Arrange
            var appName = "tweeter3";
            var hostDir = Path.Combine(_hostSamplesDir, "python", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "oryx",
                new[] { "build", appDir },
                "oryxdevms/python-3.7",
                portMapping,
                "/bin/bash",
                new[]
                {
                    "-c",
                    script
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("logged in as: bob", data);
                });
        }

        [Theory]
        [MemberData(nameof(TestValueGenerator.GetPythonVersions), MemberType = typeof(TestValueGenerator))]
        public async Task CanBuildAndRun_ShapelyFlaskApp_UsingVirtualEnv(string pythonVersion)
        {
            // Arrange
            var appName = "shapely-flask-app";
            var hostDir = Path.Combine(_hostSamplesDir, "python", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var imageVersion = "oryxdevms/python-" + pythonVersion;

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "oryx",
                new[] { "build", appDir, "-l", "python", "--language-version", pythonVersion },
                imageVersion,
                portMapping,
                "/bin/bash",
                new[]
                {
                    "-c",
                    script
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello Shapely, Area is: 314", data);
                });
        }

        [Theory]
        [MemberData(nameof(TestValueGenerator.GetPythonVersions), MemberType = typeof(TestValueGenerator))]
        public async Task CanBuildAndRun_ShapelyFlaskApp_PackageDir(string pythonVersion)
        {
            // Arrange
            const string packageDir = "orx_packages";
            var appName = "shapely-flask-app";
            var hostDir = Path.Combine(_hostSamplesDir, "python", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort} -packagedir {packageDir}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var imageVersion = "oryxdevms/python-" + pythonVersion;

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "oryx",
                new[] { "build", appDir, "-l", "python", "--language-version", pythonVersion, "-p", $"packagedir={packageDir}" },
                imageVersion,
                portMapping,
                "/bin/bash",
                new[]
                {
                    "-c",
                    script
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello Shapely, Area is: 314", data);
                });
        }

        [Fact]
        public async Task PythonStartupScript_UsesPortEnvironmentVariableValue()
        {
            // Arrange
            var appName = "flask-app";
            var hostDir = Path.Combine(_hostSamplesDir, "python", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"export PORT={ContainerPort}")
                .AddCommand($"oryx -appPath {appDir}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "oryx",
                new[] { "build", appDir },
                "oryxdevms/python-3.7",
                portMapping,
                "/bin/bash",
                new[]
                {
                    "-c",
                    script
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task PythonStartupScript_UsesSuppliedBindingPort_EvenIfPortEnvironmentVariableValue_IsPresent()
        {
            // Arrange
            var appName = "flask-app";
            var hostDir = Path.Combine(_hostSamplesDir, "python", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"export PORT=9095")
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "oryx",
                new[] { "build", appDir },
                "oryxdevms/python-3.7",
                portMapping,
                "/bin/bash",
                new[]
                {
                    "-c",
                    script
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_MultiPlatformApp_HavingReactAndDjango()
        {
            // Arrange
            var appName = "reactdjango";
            var hostDir = Path.Combine(_hostSamplesDir, "multilanguage", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";

            var buildScript = new ShellScriptBuilder()
                .AddCommand("export ENABLE_MULTIPLATFORM_BUILD=true")
                .AddCommand($"cd {appDir}")
                .AddBuildCommand($"{appDir} -l python --language-version 3.7")
                .ToString();

            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                // User would do this through app settings
                .AddCommand("export ENABLE_MULTIPLATFORM_BUILD=true")
                .AddCommand("export DJANGO_SETTINGS_MODULE=\"reactdjango.settings.local_base\"")
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                "oryxdevms/python-3.7",
                portMapping,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runAppScript
                },
                async () =>
                {
                    var data = await GetResponseDataAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("<h1>it works! (Django Template)</h1>", data);

                    // Looks for the link to the webpack-generated file
                    var linkStartIdx = data.IndexOf("src=\"/static/webpack_bundles/main-");
                    Assert.NotEqual(-1, linkStartIdx);

                    var linkdEndIx = data.IndexOf(".js", linkStartIdx);
                    Assert.NotEqual(-1, linkdEndIx);

                    // We remove 5 chars for `src="` and add 2 since we get the first char of ".js"
                    // but we want to include ".js in the string
                    int length = linkdEndIx - linkStartIdx - 2;
                    var link = data.Substring(linkStartIdx + 5, length);

                    data = await GetResponseDataAsync($"http://localhost:{HostPort}{link}");
                    Assert.Contains("!function(e){var t={};function n(r){if(t[r])return t[r].exports", data);
                });
        }
    }
}