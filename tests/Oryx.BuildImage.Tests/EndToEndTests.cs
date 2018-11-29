// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Oryx.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.BuildImage.Tests
{
    public class EndToEndTests
    {
        private const string startupCommand = "/opt/startupcmdgen/startupcmdgen";

        private readonly ITestOutputHelper _output;
        private readonly string _hostSamplesDir;
        private readonly HttpClient _httpClient;

        public EndToEndTests(ITestOutputHelper output)
        {
            _output = output;
            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
            _httpClient = new HttpClient();
        }

        [Theory]
        [InlineData("10.13")]
        public async Task NodeApp(string nodeVersion)
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", "webfrontend");
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var port = 8000;
            var portMapping = $"{port}:80";
            var startupFile = "/tmp/startup.sh";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"echo '#!/bin/sh' > {startupFile}")
                .AddCommand($"{startupCommand} -appPath {appDir} >> {startupFile} 2>&1")
                .AddCommand($"chmod a+x {startupFile}")
                .AddCommand(startupFile)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                _output,
                volume,
                "oryx",
                new[] { "build", appDir, "-l", "nodejs", "--language-version", nodeVersion },
                $"oryxdevms/node-{nodeVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    script
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{port}/");
                    Assert.Contains("Say It Again", data);
                });
        }

        [Fact]
        public async Task NodeApp_WithYarnLock()
        {
            // Arrange
            var nodeVersion = "10.13";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", "webfrontend-yarnlock");
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var port = 8000;
            var portMapping = $"{port}:80";
            var startupFile = "/tmp/startup.sh";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"echo '#!/bin/sh' > {startupFile}")
                .AddCommand($"{startupCommand} -appPath {appDir} >> {startupFile} 2>&1")
                .AddCommand($"chmod a+x {startupFile}")
                .AddCommand(startupFile)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                _output,
                volume,
                "oryx",
                new[] { "build", appDir, "-l", "nodejs", "--language-version", nodeVersion },
                $"oryxdevms/node-{nodeVersion}",
                portMapping,
                "/bin/sh",
                new[]
                {
                    "-c",
                    script
                },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{port}/");
                    Assert.Contains("Say It Again", data);
                });
        }

        [Fact(Skip = "https://devdiv.visualstudio.com/DevDiv/_workitems/edit/736633")]
        public async Task Python27App()
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "python", "python2-flask-app");
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var port = 8000;
            var portMapping = $"{port}:5000";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand("./entryPoint.sh")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
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
                    var data = await _httpClient.GetStringAsync($"http://localhost:{port}/");
                    Assert.Contains("Hello World!!!!", data);
                });
        }

        [Fact]
        public async Task FlaskApp_Python37()
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "python", "flask-app");
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var port = 8000;
            var portMapping = $"{port}:5000";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand("./entryPoint.sh")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
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
                    var data = await _httpClient.GetStringAsync($"http://localhost:{port}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task FlaskApp_Python36()
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "python", "flask-app");
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var port = 8000;
            var portMapping = $"{port}:5000";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand("./entryPoint.sh")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
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
                    var data = await _httpClient.GetStringAsync($"http://localhost:{port}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task DjangoApp_Python37()
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "python", "django-app");
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var port = 8000;
            var portMapping = $"{port}:5000";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand("./entryPoint.sh")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
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
                    var data = await GetResponseDataAsync($"http://localhost:{port}/staticfiles/css/boards.css");
                    Assert.Contains("CSS file from Boards app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{port}/staticfiles/css/uservoice.css");
                    Assert.Contains("CSS file from UserVoice app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{port}/boards/");
                    Assert.Contains("Hello, World! from Boards app", data);

                    data = await GetResponseDataAsync($"http://localhost:{port}/uservoice/");
                    Assert.Contains("Hello, World! from Uservoice app", data);
                });
        }

        [Fact]
        public async Task DjangoApp_Python36()
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "python", "django-app");
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var port = 8000;
            var portMapping = $"{port}:5000";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand("./entryPoint.sh")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
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
                    var data = await GetResponseDataAsync($"http://localhost:{port}/staticfiles/css/boards.css");
                    Assert.Contains("CSS file from Boards app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{port}/staticfiles/css/uservoice.css");
                    Assert.Contains("CSS file from UserVoice app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{port}/boards/");
                    Assert.Contains("Hello, World! from Boards app", data);

                    data = await GetResponseDataAsync($"http://localhost:{port}/uservoice/");
                    Assert.Contains("Hello, World! from Uservoice app", data);
                });
        }

        // The following method is used to avoid following exception from HttpClient when trying to read a response:
        // '"utf-8"' is not a supported encoding name. For information on defining a custom encoding, 
        // see the documentation for the Encoding.RegisterProvider method.
        private async Task<string> GetResponseDataAsync(string url)
        {
            var bytes = await _httpClient.GetByteArrayAsync(url);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
