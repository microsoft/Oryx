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
    public class PhpWordPressTest : PhpEndToEndTestsBase
    {
        public PhpWordPressTest(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        // Unique category traits are needed to run each
        // platform-version in it's own pipeline agent. This is
        // because our agents currently a space limit of 10GB.
        [Fact, Trait("category", "php-8.2")]
        [Trait("build-image", "debian-stretch")]
        public async Task PipelineTestInvocationsPhp82Async()
        {
            string phpVersion82 = "8.2";
            await CanBuildAndRun_Wordpress_SampleAppAsync(phpVersion82, ImageTestHelperConstants.OsTypeDebianBullseye);
        }

        [Fact, Trait("category", "php-8.1")]
        [Trait("build-image", "debian-stretch")]
        public async Task PipelineTestInvocationsPhp81Async()
        {
            string phpVersion81 = "8.1";
            await CanBuildAndRun_Wordpress_SampleAppAsync(phpVersion81, ImageTestHelperConstants.OsTypeDebianBullseye);
        }

        [Fact, Trait("category", "php-8.0")]
        [Trait("build-image", "debian-stretch")]
        public async Task PipelineTestInvocationsPhp80Async()
        {
            string phpVersion80 = "8.0";
            await CanBuildAndRun_Wordpress_SampleAppAsync(phpVersion80, ImageTestHelperConstants.OsTypeDebianBullseye);
        }

        [Fact, Trait("category", "php-7.4")]
        [Trait("build-image", "debian-stretch")]
        public async Task PipelineTestInvocationsPhp74Async()
        {
            string phpVersion74 = "7.4";
            await Task.WhenAll(
                PhpWithWordPress51Async(phpVersion74, ImageTestHelperConstants.OsTypeDebianBullseye),
                CanBuildAndRun_Wordpress_SampleAppAsync(phpVersion74, ImageTestHelperConstants.OsTypeDebianBullseye));
        }

        private async Task PhpWithWordPress51Async(string phpVersion, string osType)
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
                    webClient.DownloadFile("https://wordpress.org/wordpress-5.1.zip", wpZipPath);
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
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort} -output {RunScriptPath}")
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

        private async Task CanBuildAndRun_Wordpress_SampleAppAsync(string phpVersion, string osType)
        {
            // Arrange
            var appName = "wordpress-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            //var phpimageVersion = phpVersion.Split("-");

            // build-script to download wordpress cli and build
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"curl -O https://wordpress.org/latest.tar.gz")
                .AddCommand($"tar -xvf latest.tar.gz")
                .AddCommand("cd wordpress")
                .AddCommand("mv * ..")
                .AddCommand("cd ..")
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PhpConstants.PlatformName} --platform-version {phpVersion}")
                .ToString();

            // run script to finish wordpress configuration and run the app
            var runScript = new ShellScriptBuilder()
                .AddCommand($"cd {appOutputDir}")
                .AddCommand($"chmod +x create_wordpress_db.sh && ./create_wordpress_db.sh > /dev/null 2>&1")
                .AddCommand($"chmod +x configure_wordpress.sh && ./configure_wordpress.sh > /dev/null 2>&1")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort} -output {RunScriptPath}")
                .AddCommand($"{RunScriptPath} 2>&1 > {appOutputDir}/runlog.txt")
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
                    // this is to test wordpress and mysql connection is working
                    var testdbconnectiondata = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/testdb.php");
                    Assert.DoesNotContain("Unable to connect to MySQL", testdbconnectiondata);
                    Assert.Contains("Connected successfully", testdbconnectiondata);

                    // this is to test regular wordpress site is working
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/wp-login.php");
                    Assert.Contains("Powered by WordPress", data);
                    Assert.Contains("Remember Me", data);
                    Assert.Contains("Lost your password?", data);
                });
        }
    }
}
