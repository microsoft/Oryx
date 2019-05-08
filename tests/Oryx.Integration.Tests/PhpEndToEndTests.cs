// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "php")]
    public class PhpEndToEndTests : PlatformEndToEndTestsBase
    {
        private const int HostPort = Constants.PhpEndToEndTestsPort;
        private const int ContainerPort = 8080;
        private const string RunScriptPath = "/tmp/startup.sh";

        private readonly ITestOutputHelper _output;
        private readonly string _hostSamplesDir;
        private readonly string _hostTempDir;
        private readonly IList<string> _downloadedPaths = new List<string>();

        public PhpEndToEndTests(ITestOutputHelper output, TestTempDirTestFixture fixture)
        {
            _output = output;
            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
            _hostTempDir = fixture.RootDirPath;
        }
        
        [Theory]
        [InlineData("7.3")]
        [InlineData("7.2")]
        [InlineData("7.0")]
        // Twig does not support PHP < 7
        public async Task TwigExample(string phpVersion)
        {
            // Arrange
            var appName = "twig-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"oryx -appPath {appDir} -output {RunScriptPath}")
                .AddCommand(RunScriptPath)
                .ToString();

            // Act & Assert
            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName, _output, volume,
                "oryx", new[] { "build", appDir, "-l", "php", "--language-version", phpVersion },
                $"oryxdevms/php-{phpVersion}",
                $"{HostPort}:{ContainerPort}",
                "/bin/sh", new[] { "-c", script },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("<h1>Hello World!</h1>", data);
                });
        }

        [Theory]
        [InlineData("7.3")]
        [InlineData("7.2")]
        [InlineData("7.0")]
        [InlineData("5.6")]
        public async Task WordPress51(string phpVersion)
        {
            // Arrange
            string hostDir = Path.Combine(_hostTempDir, "wordpress");
            if (!Directory.Exists(hostDir))
            {
                using (var webClient = new WebClient())
                {
                    var wpZipPath = Path.Combine(_hostTempDir, "wp.zip");
                    webClient.DownloadFile("https://wordpress.org/wordpress-5.1.zip", wpZipPath);
                    // The ZIP already contains a `wordpress` folder
                    ZipFile.ExtractToDirectory(wpZipPath, _hostTempDir);
                }
            }

            var appName = "wordpress";
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var runScript = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"oryx -appPath {appDir} -output {RunScriptPath}")
                .AddCommand(RunScriptPath)
                .ToString();

            // Act & Assert
            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName, _output, volume,
                "oryx", new[] { "build", appDir, "-l", "php", "--language-version", phpVersion },
                $"oryxdevms/php-{phpVersion}",
                $"{HostPort}:{ContainerPort}",
                "/bin/sh", new[] { "-c", runScript },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("<title>WordPress &rsaquo; Setup Configuration File</title>", data);
                });
        }

        [Theory]
        [InlineData("7.3")]
        [InlineData("7.2")]
        [InlineData("7.0")]
        [InlineData("5.6")]
        public async Task ImagickExample(string phpVersion)
        {
            // Arrange
            var appName = "imagick-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var runScript = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"oryx -appPath {appDir} -output {RunScriptPath}")
                .AddCommand(RunScriptPath)
                .ToString();

            // Act & Assert
            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName, _output, volume,
                "oryx", new[] { "build", appDir, "-l", "php", "--language-version", phpVersion },
                $"oryxdevms/php-{phpVersion}",
                $"{HostPort}:{ContainerPort}",
                "/bin/sh", new[] { "-c", runScript },
                async () =>
                {
                    string imagickOutput = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Equal("64x64", imagickOutput);
                });
        }
    }
}
