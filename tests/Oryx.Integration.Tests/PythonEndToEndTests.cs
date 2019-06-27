// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public abstract class PythonEndToEndTestsBase : PlatformEndToEndTestsBase
    {
        protected const int ContainerPort = 3000;
        protected const string DefaultStartupFilePath = "./run.sh";

        protected readonly ITestOutputHelper _output;
        protected readonly string _hostSamplesDir;
        protected readonly string _tempRootDir;

        public PythonEndToEndTestsBase(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
        {
            _output = output;
            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
            _tempRootDir = testTempDirTestFixture.RootDirPath;
        }

        protected DockerVolume CreateAppVolume(string appName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "python", appName));
    }

    [Trait("category", "python")]
    public class PythonEndToEndTests_BackCompat : PythonEndToEndTestsBase
    {
        public PythonEndToEndTests_BackCompat(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanRunPythonApp_UsingEarlierBuiltPackagesDirectory()
        {
            // This is AppService's scenario where previously built apps can still run
            // fine.

            // Arrange
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var virtualEnvName = "antenv";
            var buildScript = new ShellScriptBuilder()
                // App should run fine even with manifest file not present
                .AddCommand($"oryx build {appDir} -p packagedir={PythonConstants.DefaultTargetPackageDirectory}")
                .AddCommand($"rm -f {appDir}/{FilePaths.BuildManifestFileName}")
                .AddFileDoesNotExistCheck($"{appDir}/{FilePaths.BuildManifestFileName}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -virtualEnvName {virtualEnvName} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/bash",
                new[] { "-c", buildScript },
                "oryxdevms/python-3.7",
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
        public async Task CanRunPythonApp_WithoutBuildManifestFile()
        {
            // This is AppService's scenario where previously built apps can still run
            // fine.

            // Arrange
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var virtualEnvName = "antenv";
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -p virtualenv_name={virtualEnvName}")
                // App should run fine even with manifest file not present
                .AddCommand($"rm -f {appDir}/{FilePaths.BuildManifestFileName}")
                .AddFileDoesNotExistCheck($"{appDir}/{FilePaths.BuildManifestFileName}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -virtualEnvName {virtualEnvName} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/bash",
                new[] { "-c", buildScript },
                "oryxdevms/python-3.7",
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
    }

    [Trait("category", "python")]
    public class PythonEndToEndTests_Python27 : PythonEndToEndTestsBase
    {
        public PythonEndToEndTests_Python27(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanBuildAndRunPythonApp_UsingPython27_AndExplicitOutputStartupFile()
        {
            // Arrange
            var appName = "python2-flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var startupFile = "/tmp/startup.sh";
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} --platform python --language-version 2.7")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -output {startupFile} -bindPort {ContainerPort}")
                .AddCommand(startupFile)
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
        public async Task CanBuildAndRun_Python27App_UsingVirtualEnv()
        {
            // Arrange
            var appName = "python2-flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            const string virtualEnvName = "antenv2.7";
            var buildScript = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} --platform python --language-version 2.7 -p virtualenv_name={virtualEnvName}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                // Mimic the commands ran by app service in their derived image.
                .AddCommand("pip install gunicorn")
                .AddCommand("pip install flask")
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
                "oryxdevms/python-2.7",
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
    }

    [Trait("category", "python")]
    public class Python37EndToEndTests : PythonEndToEndTestsBase
    {
        public Python37EndToEndTests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Theory]
        [InlineData("3.7")]
        [InlineData("3.8")]
        public async Task CanBuildAndRunPythonApp(string pythonVersion)
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform python --language-version {pythonVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/bash", new[] { "-c", buildScript },
                $"oryxdevms/python-{pythonVersion}",
                ContainerPort,
                "/bin/bash",
                new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact(Skip = "AB#896178")]
        public async Task CanBuildAndRunPythonApp_WithBuildpack()
        {
            var appName = "flask-app";

            await EndToEndTestHelper.RunPackAndAssertAppAsync(
                _output,
                appName,
                CreateAppVolume(appName),
                "test-py37app",
                Constants.OryxBuildpackBuilderImageName,
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRunPythonApp_UsingPython37_AndVirtualEnv()
        {
            // Arrange
            var appName = "flask-app";
            var virtualEnvName = "antenv";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -p virtualenv_name={virtualEnvName}")
               .ToString();
            var runScript = new ShellScriptBuilder()
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
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirPath = Directory.CreateDirectory(
                Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N"))).FullName;
            var appOutputDirVolume = DockerVolume.CreateMirror(appOutputDirPath);
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
                .AddCommand($"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
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
        public async Task CanBuildAndRunPythonApp_UsingCustomManifestFileLocation()
        {
            // Arrange
            var appName = "flask-app";
            var virtualEnvName = "antenv";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirPath = Directory.CreateDirectory(
                Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N"))).FullName;
            var appOutputDirVolume = DockerVolume.CreateMirror(appOutputDirPath);
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var manifestDirPath = Directory.CreateDirectory(
            Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N"))).FullName;
            var manifestDirVolume = DockerVolume.CreateMirror(manifestDirPath);
            var manifestDir = manifestDirVolume.ContainerDir;
            var tempOutputDir = "/tmp/output";
            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {tempOutputDir} --manifest-dir {manifestDir} " +
                $" -p virtualenv_name={virtualEnvName} -p compress_virtualenv=tar-gz")
                .AddDirectoryDoesNotExistCheck($"{tempOutputDir}/{virtualEnvName}")
                .AddFileExistsCheck($"{tempOutputDir}/{virtualEnvName}.tar.gz")
                .AddCommand($"cp -rf {tempOutputDir}/* {appOutputDir}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -manifestDir {manifestDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { appOutputDirVolume, volume, manifestDirVolume },
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                "oryxdevms/python-3.7",
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
        public async Task CanBuildAndRun_DjangoApp_DoingCollectStaticByDefault()
        {
            // Arrange
            var appName = "django-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir}")
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
                "oryxdevms/python-3.7",
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
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

        [Fact]
        public async Task CanBuildAndRun_DjangoPython37App_UsingVirtualEnv()
        {
            // Arrange
            var appName = "django-app";
            var volume = CreateAppVolume(appName);
            var appOutputDirPath = Directory.CreateDirectory(
                Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N"))).FullName;
            var appOutputDirVolume = DockerVolume.CreateMirror(appOutputDirPath);
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var appDir = volume.ContainerDir;
            const string virtualEnvName = "antenv";

            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o /tmp/out --platform python --language-version 3.7 " +
                $"-p virtualenv_name={virtualEnvName}")
                .AddCommand($"cp -rf /tmp/out/* {appOutputDir}")
                .ToString();

            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
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
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
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
    }

    [Trait("category", "python")]
    public class PythonEndToEndTests : PythonEndToEndTestsBase
    {
        public PythonEndToEndTests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanBuildAndRunPythonApp_UsingPython36()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform python --language-version 3.6")
               .ToString();
            var runScript = new ShellScriptBuilder()
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
        public async Task CanBuildAndRun_Tweeter3App()
        {
            // Arrange
            var appName = "tweeter3";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir}")
               .ToString();
            var runScript = new ShellScriptBuilder()
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

        [Theory]
        [InlineData("3.6")]
        [InlineData("3.7")]
        public async Task BuildWithVirtualEnv_RemovesOryxPackagesDir_FromOlderBuild(string pythonVersion)
        {
            // Arrange
            var appName = "django-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            const string virtualEnvName = "antenv";

            // Simulate apps that were built using package directory, and then virtual env
            var buildScript = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} --platform python --language-version {pythonVersion}")
                .AddBuildCommand(
                $"{appDir} -p virtualenv_name={virtualEnvName} " +
                $"--platform python --language-version {pythonVersion}")
                .ToString();

            var runScript = new ShellScriptBuilder()
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
    }

    [Trait("category", "python")]
    public class PythonEndToEndTests_DjangoApp : PythonEndToEndTestsBase
    {
        public PythonEndToEndTests_DjangoApp(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Fact]
        public async Task CanBuildAndRun_DjangoPython36App_UsingVirtualEnv()
        {
            // Arrange
            var appName = "django-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            const string virtualEnvName = "antenv3.6";
            var buildScript = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} --platform python --language-version 3.6 -p virtualenv_name={virtualEnvName}")
                .ToString();
            var runScript = new ShellScriptBuilder()
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
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
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

        [Fact]
        public async Task CanBuildAndRun_DjangoApp_UsingPython36()
        {
            // Arrange
            var appName = "django-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform python --language-version 3.6")
               .ToString();
            var runScript = new ShellScriptBuilder()
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
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
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

        [Fact]
        public async Task CanBuildAndRun_MultiPlatformApp_HavingReactAndDjango()
        {
            // Arrange
            var appName = "reactdjango";
            var hostDir = Path.Combine(_hostSamplesDir, "multilanguage", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;

            var buildScript = new ShellScriptBuilder()
                .AddCommand("export ENABLE_MULTIPLATFORM_BUILD=true")
                .AddBuildCommand($"{appDir} --platform python --language-version 3.7")
                .ToString();

            var runAppScript = new ShellScriptBuilder()
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
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runAppScript
                },
                async (hostPort) =>
                {
                    var data = await GetResponseDataAsync($"http://localhost:{hostPort}/");
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

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}{link}");
                    Assert.Contains("!function(e){var t={};function n(r){if(t[r])return t[r].exports", data);
                });
        }
    }

    [Trait("category", "python")]
    public class PythonEndToEndTests_ShapelyApp : PythonEndToEndTestsBase
    {
        public PythonEndToEndTests_ShapelyApp(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Theory]
        [MemberData(nameof(TestValueGenerator.GetPythonVersions), MemberType = typeof(TestValueGenerator))]
        public async Task CanBuildAndRun_ShapelyFlaskApp_UsingVirtualEnv(string pythonVersion)
        {
            // Arrange
            var appName = "shapely-flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform python --language-version {pythonVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var imageVersion = "oryxdevms/python-" + pythonVersion;

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
                imageVersion,
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
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand(
                $"oryx build {appDir} --platform python --language-version {pythonVersion} -p packagedir={packageDir}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            var imageVersion = "oryxdevms/python-" + pythonVersion;

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
                imageVersion,
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
                    Assert.Contains("Hello Shapely, Area is: 314", data);
                });
        }
    }

    [Trait("category", "python")]
    public class PythonEndToEndTests_PortEnvVariable : PythonEndToEndTestsBase
    {
        public PythonEndToEndTests_PortEnvVariable(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Fact]
        public async Task PythonStartupScript_UsesPortEnvironmentVariableValue()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"export PORT={ContainerPort}")
                .AddCommand($"oryx -appPath {appDir}")
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
        public async Task PythonStartupScript_UsesSuppliedBindingPort_EvenIfPortEnvironmentVariableValue_IsPresent()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"export PORT=9095")
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
    }
}