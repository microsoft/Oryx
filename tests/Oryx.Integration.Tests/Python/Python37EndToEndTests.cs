// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
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
                _imageHelper.GetRuntimeImage("python", "3.7"),
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
                _imageHelper.GetRuntimeImage("python", "3.7"),
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
                _imageHelper.GetRuntimeImage("python", "3.7"),
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
                _imageHelper.GetRuntimeImage("python", "3.7"),
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
                _imageHelper.GetRuntimeImage("python", "3.7"),
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
}