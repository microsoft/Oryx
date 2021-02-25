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
    [Trait("category", "php")]
    public class PhpWordPressTest : PhpEndToEndTestsBase
    {
        public PhpWordPressTest(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Theory]
        //skipping php-8 test as it is not compatible with composer 1.9
        //[InlineData("8.0-fpm")]
        [InlineData("7.4-fpm")]
        [InlineData("7.3")]
        [InlineData("7.2")]
        [InlineData("7.0")]
        public async Task PhpWithWordPress51(string phpVersion)
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
                _imageHelper.GetRuntimeImage("php", phpimageVersion[0]),
                ContainerPort,
                "/bin/sh", new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("<title>WordPress &rsaquo; Setup Configuration File</title>", data);
                });
        }

        [Theory(Skip = "#1101128, investigate why  this tests are only failing in agent machines")]
        [InlineData("8.0")]
        [InlineData("7.4")]
        [InlineData("7.3-fpm")]
        [InlineData("7.2-fpm")]
        [InlineData("5.6")]
        public async Task CanBuildAndRun_Wordpress_SampleApp(string phpVersion)
        {
            // Arrange
            var appName = "wordpress-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var phpimageVersion = phpVersion.Split("-");

            // build-script to download wordpress cli and build
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"./wp-cli.phar core download")
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PhpConstants.PlatformName} --platform-version {phpimageVersion[0]}")
                .ToString();

            // run script to finish wordpress configuration and run the app
            var runScript = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"chmod +x create_wordpress_db.sh && ./create_wordpress_db.sh")
                .AddCommand($"chmod +x configure_wordpress.sh && ./configure_wordpress.sh")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort} -output {RunScriptPath}")
                .AddCommand("mkdir -p /home/site/wwwroot")
                .AddCommand($"cp -a {appOutputDir}/. /home/site/wwwroot")
                .AddCommand(RunScriptPath)
                .ToString();

            // Act & Assert
            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName, _output, new[] { volume, appOutputDirVolume },
                "/bin/sh", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("php", phpimageVersion[0]),
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
                    Assert.Contains("Back to localsite", data);
                });
        }
    }
}
