// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.DotnetCore;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests.LocalDockerTests
{
    public class DotnetCoreEndToEndTests
    {
        private const int HostPort = 8081;
        private const string startupFilePath = "/tmp/startup.sh";
        private const string NetCoreApp11WebApp = "NetCoreApp11WebApp";
        private const string NetCoreApp21WebApp = "NetCoreApp21WebApp";
        private const string NetCoreApp22WebApp = "NetCoreApp22WebApp";
        private const string NetCoreApp21MultiProjectApp = "NetCoreApp21MultiProjectApp";

        private readonly ITestOutputHelper _output;
        private readonly string _hostSamplesDir;
        private readonly HttpClient _httpClient;

        public DotnetCoreEndToEndTests(ITestOutputHelper output)
        {
            _output = output;
            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
            _httpClient = new HttpClient();
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore11WebApp()
        {
            // Arrange
            var dotnetcoreVersion = "1.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp11WebApp);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var containerPort = "9095";
            var portMapping = $"{HostPort}:{containerPort}";
            var startupFilePath = "/tmp/run.sh";
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx -sourcePath {appDir} -output {startupFilePath}")
                .AddCommand($"export ASPNETCORE_URLS=http://*:{containerPort}")
                .AddCommand(startupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp11WebApp,
                _output,
                volume,
                "oryx",
                new[] { "build", appDir, "-l", "dotnet", "--language-version", dotnetcoreVersion },
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
            var containerPort = "9095";
            var portMapping = $"{HostPort}:{containerPort}";
            var startupFilePath = "/tmp/run.sh";
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx -sourcePath {appDir} -output {startupFilePath}")
                .AddCommand($"export ASPNETCORE_URLS=http://*:{containerPort}")
                .AddCommand(startupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "oryx",
                new[] { "build", appDir, "-l", "dotnet", "--language-version", dotnetcoreVersion },
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
        public async Task CanBuildAndRun_NetCore21WebApp()
        {
            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var containerPort = "9095";
            var portMapping = $"{HostPort}:{containerPort}";
            var startupFilePath = "/tmp/run.sh";
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx -sourcePath {appDir} -output {startupFilePath}")
                .AddCommand($"export ASPNETCORE_URLS=http://*:{containerPort}")
                .AddCommand(startupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
                "oryx",
                new[] { "build", appDir, "-l", "dotnet", "--language-version", dotnetcoreVersion },
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
            var containerPort = "9095";
            var portMapping = $"{HostPort}:{containerPort}";
            var startupFilePath = "/tmp/run.sh";
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx -sourcePath {appDir} -output {startupFilePath}")
                .AddCommand($"export ASPNETCORE_URLS=http://*:{containerPort}")
                .AddCommand(startupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "oryx",
                new[] { "build", appDir, "-l", "dotnet", "--language-version", dotnetcoreVersion },
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
        public async Task CanBuildAndRun_NetCore21WebApp_WhenUsingExplicitPublishOutputDirectory()
        {
            // Arrange
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp21WebApp);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var containerPort = "9095";
            var portMapping = $"{HostPort}:{containerPort}";
            var startupFilePath = "/tmp/run.sh";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -l dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx -sourcePath {appDir} -publishedOutputPath {appOutputDir} -output {startupFilePath}")
                .AddCommand($"export ASPNETCORE_URLS=http://*:{containerPort}")
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
        public async Task CanBuildAndRun_NetCore22WebApp()
        {
            // Arrange
            var dotnetcoreVersion = "2.2";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp22WebApp);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var containerPort = "9095";
            var portMapping = $"{HostPort}:{containerPort}";
            var startupFilePath = "/tmp/run.sh";
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx -sourcePath {appDir} -output {startupFilePath}")
                .AddCommand($"export ASPNETCORE_URLS=http://*:{containerPort}")
                .AddCommand(startupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp22WebApp,
                _output,
                volume,
                "oryx",
                new[] { "build", appDir, "-l", "dotnet", "--language-version", dotnetcoreVersion },
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
            var containerPort = "9095";
            var portMapping = $"{HostPort}:{containerPort}";
            var startupFilePath = "/tmp/run.sh";
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"oryx -sourcePath {appDir} -output {startupFilePath}")
                .AddCommand($"export ASPNETCORE_URLS=http://*:{containerPort}")
                .AddCommand(startupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "oryx",
                new[] { "build", appDir, "-l", "dotnet", "--language-version", dotnetcoreVersion },
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
            var containerPort = "9095";
            var portMapping = $"{HostPort}:{containerPort}";
            var startupFilePath = "/tmp/run.sh";
            var startupCommand = "\"dotnet foo.dll\"";
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"cp {appDir}/{DotnetCoreConstants.OryxOutputPublishDirectory}/{NetCoreApp21WebApp}.dll " +
                $"{appDir}/{DotnetCoreConstants.OryxOutputPublishDirectory}/foo.dll")
                .AddCommand(
                $"cp {appDir}/{DotnetCoreConstants.OryxOutputPublishDirectory}/{NetCoreApp21WebApp}.runtimeconfig.json " +
                $"{appDir}/{DotnetCoreConstants.OryxOutputPublishDirectory}/foo.runtimeconfig.json")
                .AddCommand($"oryx -sourcePath {appDir} -output {startupFilePath} -userStartupCommand {startupCommand}")
                .AddCommand($"export ASPNETCORE_URLS=http://*:{containerPort}")
                .AddCommand(startupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                volume,
                "oryx",
                new[] { "build", appDir, "-l", "dotnet", "--language-version", dotnetcoreVersion },
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
        public async Task CanBuildAndRun_NetCore21WebApp_HavingNestProjectDirectory_WhenNotSpecifyingLanguageVersion()
        {
            // Arrange
            var appName = "MultiWebAppRepo";
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.Create(hostDir);
            var repoDir = volume.ContainerDir;
            var containerPort = "9095";
            var portMapping = $"{HostPort}:{containerPort}";
            var startupFilePath = "/tmp/run.sh";
            var setProjectEnvVariable = "export PROJECT=src/WebApp1/WebApp1.csproj";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(setProjectEnvVariable)
                .AddCommand($"oryx build {repoDir}") // Do not specify language and version
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(setProjectEnvVariable)
                .AddCommand($"oryx -sourcePath {repoDir} -output {startupFilePath}")
                .AddCommand($"export ASPNETCORE_URLS=http://*:{containerPort}")
                .AddCommand(startupFilePath)
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
        public async Task CanBuildAndRun_NetCore21WebApp_HavingNestProjectDirectory()
        {
            // Arrange
            var appName = "MultiWebAppRepo";
            var dotnetcoreVersion = "2.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.Create(hostDir);
            var repoDir = volume.ContainerDir;
            var containerPort = "9095";
            var portMapping = $"{HostPort}:{containerPort}";
            var startupFilePath = "/tmp/run.sh";
            var setProjectEnvVariable = "export PROJECT=src/WebApp1/WebApp1.csproj";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(setProjectEnvVariable)
                .AddCommand($"oryx build {repoDir} -l dotnet --language-version {dotnetcoreVersion}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(setProjectEnvVariable)
                .AddCommand($"oryx -sourcePath {repoDir} -output {startupFilePath}")
                .AddCommand($"export ASPNETCORE_URLS=http://*:{containerPort}")
                .AddCommand(startupFilePath)
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
    }
}