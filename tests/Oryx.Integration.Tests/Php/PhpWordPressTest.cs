// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Php;
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
        [InlineData("7.4")]
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
               .AddCommand($"oryx build {appDir} --platform {PhpConstants.PlatformName} --language-version {phpVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appDir} -output {RunScriptPath}")
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

        [Theory]
        [InlineData("7.4")]
        [InlineData("7.3")]
        [InlineData("7.2")]
        [InlineData("7.0")]
        [InlineData("5.6")]
        public async Task CanBuildAndRun_Wordpress_SampleApp(string phpVersion)
        {
            // Arrange
            var appName = "wordpress-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;

            // build-script to download wordpress cli and build
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand("curl -O https://raw.githubusercontent.com/wp-cli/builds/gh-pages/phar/wp-cli.phar")
                .AddCommand("chmod +x wp-cli.phar")
                .AddCommand("./wp-cli.phar core download")
                .AddCommand($"oryx build {appDir} --platform {PhpConstants.PlatformName} --language-version {phpVersion}")
                .AddDirectoryExistsCheck("wp-admin")
                .AddFileDoesNotExistCheck("wp-config.php")
                .ToString();

            // run script to finish wordpress configuration and run the app
            var runScript = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddFileExistsCheck("oryx-manifest.toml")
                .AddFileExistsCheck("wp-cli.phar")
                .AddFileExistsCheck("create_wordpress_db.sh")
                .AddFileExistsCheck("configure_wordpress.sh")
                .AddCommand($"chmod +x create_wordpress_db.sh && ./create_wordpress_db.sh")
                .AddCommand($"chmod +x configure_wordpress.sh && ./configure_wordpress.sh")
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort} -output {RunScriptPath}")
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
                    // this is to test wordpress and mysql connection is working
                    var testdbconnectiondata = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/testdb.php");
                    Assert.DoesNotContain("Unable to connect to MySQL",testdbconnectiondata);
                    Assert.Contains("Connected successfully", testdbconnectiondata);

                    // this is to test regular wordpress site is working
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/wp-login.php");
                    Assert.Contains("Powered by WordPress", data);
                    Assert.Contains("Remember Me", data);
                    Assert.Contains("Lost your password?", data);
                    Assert.Contains("Back to localsite", data);
                });
        }

        [Theory]
        [InlineData("7.4")]
        [InlineData("7.3")]
        [InlineData("7.2")]
        public async Task CanBuildAndRun_Wordpress_SampleApp_With_Phpfpm(string phpVersion)
        {
            // Arrange
            var appName = "wordpress-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var phpimageVersion = string.Concat(phpVersion, "-fpm");

            // build-script to download wordpress cli and build
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand("curl -O https://raw.githubusercontent.com/wp-cli/builds/gh-pages/phar/wp-cli.phar")
                .AddCommand("chmod +x wp-cli.phar")
                .AddCommand("./wp-cli.phar core download")
                .AddCommand($"oryx build {appDir} --platform {PhpConstants.PlatformName} --language-version {phpVersion}")
                .AddDirectoryExistsCheck("wp-admin")
                .AddFileDoesNotExistCheck("wp-config.php")
                .ToString();

            // run script to finish wordpress configuration and run the app
            var runScript = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddFileExistsCheck("oryx-manifest.toml")
                .AddFileExistsCheck("wp-cli.phar")
                .AddFileExistsCheck("create_wordpress_db.sh")
                .AddFileExistsCheck("configure_wordpress.sh")
                .AddCommand($"chmod +x create_wordpress_db.sh && ./create_wordpress_db.sh")
                .AddCommand($"chmod +x configure_wordpress.sh && ./configure_wordpress.sh")
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort} -output {RunScriptPath}")
                .AddCommand("mkdir -p /home/site/wwwroot")
                .AddCommand($"cp -a {appDir}/. /home/site/wwwroot")
                .AddCommand(RunScriptPath)
                .ToString();

            // Act & Assert
            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName, _output, volume,
                "/bin/sh", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("php", phpimageVersion),
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
