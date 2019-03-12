// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests.LocalDockerTests
{
    public class PhpEndToEndTests : IDisposable
    {
        private const int HostPort = 8000;
        private const string RunScriptPath = "/tmp/startup.sh";

        private readonly ITestOutputHelper _output;
        private readonly string _hostSamplesDir;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly IList<string> _downloadedPaths = new List<string>();

        public PhpEndToEndTests(ITestOutputHelper output)
        {
            _output = output;
            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
        }

        public void Dispose()
        {
            foreach (string path in _downloadedPaths)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
        }

        [Theory]
        [InlineData(PhpVersions.Php73Version)]
        [InlineData(PhpVersions.Php72Version)]
        [InlineData(PhpVersions.Php70Version)]
        // Twig does not support PHP < 7
        public async Task TwigExample(string phpVersion)
        {
            // Arrange
            phpVersion = RemovePatchVersion(phpVersion);
            var appName = "twig-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:80";
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
                portMapping,
                "/bin/sh", new[] { "-c", script },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("<h1>Hello World!</h1>", data);
                });
        }

        [Theory]
        [InlineData(PhpVersions.Php73Version)]
        [InlineData(PhpVersions.Php72Version)]
        [InlineData(PhpVersions.Php70Version)]
        [InlineData(PhpVersions.Php56Version)]
        public async Task WordPress51(string phpVersion)
        {
            // Arrange
            phpVersion = RemovePatchVersion(phpVersion);

            var wpDir = Path.Combine(Path.GetTempPath(), "wordpress-5.1");
            Directory.CreateDirectory(wpDir);
            _downloadedPaths.Add(wpDir);

            using (var webClient = new WebClient())
            {
                var wpZipPath = Path.Combine(wpDir, "wp.zip");
                webClient.DownloadFile("https://wordpress.org/wordpress-5.1.zip", wpZipPath);
                ZipFile.ExtractToDirectory(wpZipPath, wpDir); // The ZIP already contains a `wordpress` folder
            }

            var appName = "wordpress";
            var hostDir = Path.Combine(Path.GetTempPath(), "wordpress");
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:80";
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
                portMapping,
                "/bin/sh", new[] { "-c", runScript },
                async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("<title>WordPress &rsaquo; Setup Configuration File</title>", data);
                });
        }

        private static string RemovePatchVersion(string fullVersion)
        {
            var lastDot = fullVersion.LastIndexOf('.');
            return fullVersion.Substring(0, lastDot);
        }
    }
}
