// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;
using ScriptGenerator = Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "dotnetcore")]
    public class DotNetCoreEndToEndTests : PlatformEndToEndTestsBase
    {
        private const int HostPort = Constants.DotNetCoreEndToEndTestsPort;
        private const int ContainerPort = 3000;
        private const string NetCoreApp11WebApp = "NetCoreApp11WebApp";
        private const string NetCoreApp21WebApp = "NetCoreApp21WebApp";
        private const string NetCoreApp22WebApp = "NetCoreApp22WebApp";
        private const string NetCoreApp30WebApp = "NetCoreApp30WebApp";
        private const string NetCoreApp21MultiProjectApp = "NetCoreApp21MultiProjectApp";
        private const string DefaultStartupFilePath = "./run.sh";

        private readonly ITestOutputHelper _output;
        private readonly string _hostSamplesDir;
        private readonly string _tempRootDir;

        public DotNetCoreEndToEndTests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
        {
            _output = output;
            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
            _tempRootDir = testTempDirTestFixture.RootDirPath;
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore11WebApp()
        {
            // Arrange
            var dotnetcoreVersion = "1.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp11WebApp);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -l dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp11WebApp,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                $"oryxdevms/dotnetcore-{dotnetcoreVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore11WebApp_HavingExplicitAssemblyName()
        {
            // Arrange
            var appName = "NetCoreApp11WithExplicitAssemblyName";
            var dotnetcoreVersion = "1.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -l dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
                .AddStringExistsInFileCheck(
                $"{DotnetCoreConstants.StartupFileName}=\"Yoyo.dll\"",
                $"{appOutputDir}/{ScriptGenerator.Constants.ManifestFileName}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                $"oryxdevms/dotnetcore-{dotnetcoreVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore21WebApp_UsingDefaultPublishOutputDirectory()
        {
            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var appOutputDir = $"{appDir}/{DotnetCoreConstants.OryxOutputPublishDirectory}";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -l dotnet --language-version {dotnetcoreVersion}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                $"oryxdevms/dotnetcore-{dotnetcoreVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanRunApp_WithoutBuildManifestFile()
        {
            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -o {appOutputDir} -l dotnet --language-version {dotnetcoreVersion}")
                .AddFileExistsCheck($"{appOutputDir}/{ScriptGenerator.Constants.ManifestFileName}")
                // NOTE: Delete the manifest file explicitly
                .AddCommand($"rm -f {appOutputDir}/{ScriptGenerator.Constants.ManifestFileName}")
                .AddFileDoesNotExistCheck($"{appOutputDir}/{ScriptGenerator.Constants.ManifestFileName}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -sourcePath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                $"oryxdevms/dotnetcore-{dotnetcoreVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanRunApp_NetCore21WebApp_NetCoreApp21WithExplicitAssemblyName_AndNoBuildManifestFile()
        {
            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", "NetCoreApp21WithExplicitAssemblyName");
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -o {appOutputDir} -l dotnet --language-version {dotnetcoreVersion}")
                .AddFileExistsCheck($"{appOutputDir}/{ScriptGenerator.Constants.ManifestFileName}")
                // NOTE: Delete the manifest file explicitly
                .AddCommand($"rm -f {appOutputDir}/{ScriptGenerator.Constants.ManifestFileName}")
                .AddFileDoesNotExistCheck($"{appOutputDir}/{ScriptGenerator.Constants.ManifestFileName}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -sourcePath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                $"oryxdevms/dotnetcore-{dotnetcoreVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });

        }
        [Fact]
        public async Task CanBuildAndRun_NetCore21WebApp_WhenOutputDirIsCurrentDirectory()
        {
            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -l dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                // NOTE: Make sure the current directory is the output directory
                .AddCommand($"cd {appOutputDir}")
                .AddCommand($"oryx -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                $"oryxdevms/dotnetcore-{dotnetcoreVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore21WebApp_HavingExplicitAssemblyName()
        {
            // Arrange
            var appName = "NetCoreApp21WithExplicitAssemblyName";
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -l dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
               .AddStringExistsInFileCheck(
                $"{DotnetCoreConstants.StartupFileName}=\"Yoyo.dll\"",
                $"{appOutputDir}/{ScriptGenerator.Constants.ManifestFileName}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                $"oryxdevms/dotnetcore-{dotnetcoreVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore22WebApp()
        {
            // Arrange
            var dotnetcoreVersion = "2.2";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp22WebApp);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -l dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp22WebApp,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                $"oryxdevms/dotnetcore-{dotnetcoreVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore30WebApp()
        {
            // Arrange
            var dotnetcoreVersion = "3.0";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp30WebApp);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -l dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp22WebApp,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                $"oryxdevms/dotnetcore-{dotnetcoreVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore30WebApp_HavingExplicitAssemblyName()
        {
            // Arrange
            var appName = "NetCoreApp30WebAppWithExplicitAssemblyName";
            var dotnetcoreVersion = "3.0";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -l dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
               .AddStringExistsInFileCheck(
                $"{DotnetCoreConstants.StartupFileName}=\"Yoyo.dll\"",
                $"{appOutputDir}/{ScriptGenerator.Constants.ManifestFileName}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                $"oryxdevms/dotnetcore-{dotnetcoreVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task DotNetCoreStartupScript_UsesPortEnvironmentVariableValue()
        {
            // Arrange
            var dotnetcoreVersion = "2.2";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp22WebApp);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -l dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"export PORT={ContainerPort}")
                .AddCommand(
                $"oryx -appPath {appOutputDir}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp22WebApp,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                $"oryxdevms/dotnetcore-{dotnetcoreVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task StartupScript_UsesSuppliedBindingPort_EvenIfPortEnvironmentVariableValue_IsPresent()
        {
            // Arrange
            var dotnetcoreVersion = "2.2";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp22WebApp);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -l dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"export PORT=9095")
                .AddCommand(
                $"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp22WebApp,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                $"oryxdevms/dotnetcore-{dotnetcoreVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore22WebApp_HavingExplicitAssemblyName()
        {
            // Arrange
            var appName = "NetCoreApp22WithExplicitAssemblyName";
            var dotnetcoreVersion = "2.2";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -l dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                $"oryxdevms/dotnetcore-{dotnetcoreVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore21WebApp_UsingExplicitStartupCommand()
        {
            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var startupFilePath = "/tmp/run.sh";
            var startupCommand = "\"dotnet foo.dll\"";
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -l dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"cp {appOutputDir}/{NetCoreApp21WebApp}.dll {appOutputDir}/foo.dll")
                .AddCommand($"rm -f {appOutputDir}/{NetCoreApp21WebApp}.dll")
                .AddCommand(
                $"cp {appOutputDir}/{NetCoreApp21WebApp}.runtimeconfig.json {appOutputDir}/foo.runtimeconfig.json")
                .AddCommand($"rm -f {appOutputDir}/{NetCoreApp21WebApp}.runtimeconfig.json")
                .AddCommand(
                // NOTE: Do NOT specify the source path as the startup command should be used as it is
                $"oryx -appPath {appOutputDir} -output {startupFilePath} " +
                $"-userStartupCommand {startupCommand} -bindPort {ContainerPort}")
                .AddCommand(startupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                $"oryxdevms/dotnetcore-{dotnetcoreVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore21WebApp_HavingNestedProjectDirectory_AndNoLanguageVersionSwitch()
        {
            // Arrange
            var appName = "MultiWebAppRepo";
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.Create(hostDir);
            var repoDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var setProjectEnvVariable = "export PROJECT=src/WebApp1/WebApp1.csproj";
            var appOutputDir = $"{repoDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(setProjectEnvVariable)
                .AddCommand($"oryx build {repoDir} -o {appOutputDir}") // Do not specify language and version
                .AddStringExistsInFileCheck(
                $"{DotnetCoreConstants.StartupFileName}=\"MyWebApp.dll\"",
                $"{appOutputDir}/{ScriptGenerator.Constants.ManifestFileName}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(setProjectEnvVariable)
                .AddCommand(
                $"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                $"oryxdevms/dotnetcore-{dotnetcoreVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World! from WebApp1", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore21WebApp_HavingMultipleProjects()
        {
            // Arrange
            var appName = "MultiWebAppRepo";
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.Create(hostDir);
            var repoDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var appOutputDir = $"{repoDir}/myoutputdir";
            var setProjectEnvVariable = "export PROJECT=src/WebApp1/WebApp1.csproj";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(setProjectEnvVariable)
                .AddCommand($"oryx build {repoDir} -o {appOutputDir} -l dotnet --language-version {dotnetcoreVersion}")
                .AddStringExistsInFileCheck(
                $"{DotnetCoreConstants.StartupFileName}=\"MyWebApp.dll\"",
                $"{appOutputDir}/{ScriptGenerator.Constants.ManifestFileName}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                // NOTE: Need not explicitly set the PROJECT environment variable as it is part of the build manifest
                // file
                .AddCommand($"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                $"oryxdevms/dotnetcore-{dotnetcoreVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World! from WebApp1", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRunApp_WhenOutputIsZipped_AndIntermediateDir_IsNotUsed()
        {
            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -l dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}" +
                $" -p {ScriptGenerator.Constants.ZipAllOutputBuildPropertyKey}=true")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                $"oryxdevms/dotnetcore-{dotnetcoreVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRunApp_WhenOutputIsZipped_AndIntermediateDir_IsUsed()
        {
            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:{ContainerPort}";
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -l dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}" +
                $" -p {ScriptGenerator.Constants.ZipAllOutputBuildPropertyKey}=true")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx -appPath {appOutputDir} -runFromPath /tmp/output -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                $"oryxdevms/dotnetcore-{dotnetcoreVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }
    }
}