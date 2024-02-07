// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public class PhpWordPressFpmTest : PhpEndToEndTestsBase
    {
        public PhpWordPressFpmTest(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        // Unique category traits are needed to run each
        // platform-version in it's own pipeline agent. This is
        // because our agents currently a space limit of 10GB.
        [Fact, Trait("category", "php-8.3")]
        [Trait("build-image", "debian-stretch")]
        public async Task PipelineTestInvocationsPhp83Async()
        {
            await PhpFpmWithWordPress56Async("8.3-fpm", ImageTestHelperConstants.OsTypeDebianBullseye);
        }

        [Fact, Trait("category", "php-8.2")]
        [Trait("build-image", "debian-stretch")]
        public async Task PipelineTestInvocationsPhp82Async()
        {
            await PhpFpmWithWordPress56Async("8.2-fpm", ImageTestHelperConstants.OsTypeDebianBullseye);
        }

        [Fact, Trait("category", "php-8.1")]
        [Trait("build-image", "debian-stretch")]
        public async Task PipelineTestInvocationsPhp81Async()
        {
            await PhpFpmWithWordPress56Async("8.1-fpm", ImageTestHelperConstants.OsTypeDebianBullseye);
        }

        [Fact, Trait("category", "php-8.0")]
        [Trait("build-image", "debian-stretch")]
        public async Task PipelineTestInvocationsPhp80Async()
        {
            await PhpFpmWithWordPress56Async("8.0-fpm", ImageTestHelperConstants.OsTypeDebianBullseye);
        }

        [Fact, Trait("category", "php-7.4")]
        [Trait("build-image", "debian-stretch")]
        public async Task PipelineTestInvocationsPhp74Async()
        {
            await PhpFpmWithWordPress56Async("7.4-fpm", ImageTestHelperConstants.OsTypeDebianBullseye);
        }

        private async Task PhpFpmWithWordPress56Async(string phpVersion, string osType)
        {
            // Arrange
            string hostDir = Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N"));
            var phpimageVersion = phpVersion.Split("-");

            if (!Directory.Exists(hostDir))
            {
                Directory.CreateDirectory(hostDir);
                using (var webClient = new WebClient())
                {
                    var wpZipPath = Path.Combine(hostDir, "wp.zip");
                    webClient.DownloadFile("https://wordpress.org/wordpress-5.6.zip", wpZipPath);
                    // The ZIP already contains a `wordpress` folder
                    ZipFile.ExtractToDirectory(wpZipPath, hostDir);
                }
            }

            var appName = "wordpress";
            var volume = DockerVolume.CreateMirror(Path.Combine(hostDir, "wordpress"));
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
               $"--platform {PhpConstants.PlatformName} --platform-version {phpimageVersion[0]}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand("mkdir -p /home/site/wwwroot/")
                .AddCommand($"cp -rf {appOutputDir}/* /home/site/wwwroot/")
                .AddCommand("ls -la /home/site/wwwroot/")
                .AddCommand($"oryx create-script -appPath /home/site/wwwroot/ -bindPort {ContainerPort} -output {RunScriptPath}")
                .AddCommand(RunScriptPath)
                .ToString();

            // Act & Assert
            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName, _output, new[] { volume, appOutputDirVolume },
                "/bin/sh", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("php", phpVersion, osType),
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
