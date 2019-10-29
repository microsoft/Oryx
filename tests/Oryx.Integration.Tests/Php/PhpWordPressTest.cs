// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "php")]
    public class PhpWordPressTest : PhpEndToEndTestsBase
    {
        public PhpWordPressTest(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Theory]
        [InlineData("7.3")]
        [InlineData("7.2")]
        [InlineData("7.0")]
        [InlineData("5.6")]
        public async Task WordPress51(string phpVersion)
        {
            // Arrange
            string hostDir = Path.Combine(_tempRootDir, "wordpress");
            if (!Directory.Exists(hostDir))
            {
                using (var webClient = new WebClient())
                {
                    var wpZipPath = Path.Combine(_tempRootDir, "wp.zip");
                    webClient.DownloadFile("https://wordpress.org/wordpress-5.1.zip", wpZipPath);
                    // The ZIP already contains a `wordpress` folder
                    ZipFile.ExtractToDirectory(wpZipPath, _tempRootDir);
                }
            }

            var appName = "wordpress";
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform php --language-version {phpVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -output {RunScriptPath}")
                .AddCommand(RunScriptPath)
                .ToString();

            // Act & Assert
            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName, _output, volume,
                "/bin/sh", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("php", phpVersion),
                ContainerPort,
                "/bin/sh", new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("<title>WordPress &rsaquo; Setup Configuration File</title>", data);
                });
        }
    }
}
