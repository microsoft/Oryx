// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public class PhpFpmConfigTest : PhpEndToEndTestsBase
    {

        public PhpFpmConfigTest(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        // Unique category traits are needed to run each
        // platform-version in it's own pipeline agent. This is
        // because our agents currently a space limit of 10GB.
        [Theory, Trait("category", "php-8.0")]
        [Trait("build-image", "debian-stretch")]
        [InlineData("10", "10", "7", "7", "8", "8", "6", "6")]
        [InlineData("", "5", "", "2", "", "3", "", "1")] // defaults
        [InlineData("false", null, "5", null, "5", null, "5", null, "pm.max_children must be a positive value")]
        [InlineData("-1", null, "5", null, "5", null, "5", null, "pm.max_children must be a positive value")]
        [InlineData("10", null, "5", null, "13", null, "12", null, "pm.min_spare_servers(12) and pm.max_spare_servers(13) cannot be greater than pm.max_children(10)")]
        [InlineData("10", null, "9", null, "8", null, "7", null, "pm.start_servers(9) must not be less than pm.min_spare_servers(7) and not greater than pm.max_spare_servers(8)")]
        public async Task PipelineTestInvocationsPhp80Async(
            string fpmMaxChildren, string expectedFpmMaxChildren,
            string fpmStartServers, string expectedFpmStartServers,
            string fpmMaxSpareServers, string expectedFpmMaxSpareServers,
            string fpmMinSpareServers, string expectedFpmMinSpareServers,
            string failureOutputText = default)
        {
            await PhpFpmConfigTestAsync(
                "8.0",
                ImageTestHelperConstants.OsTypeDebianBullseye,
                fpmMaxChildren, expectedFpmMaxChildren,
                fpmStartServers, expectedFpmStartServers,
                fpmMaxSpareServers, expectedFpmMaxSpareServers,
                fpmMinSpareServers, expectedFpmMinSpareServers,
                failureOutputText);
        }

        [Theory, Trait("category", "php-7.4")]
        [Trait("build-image", "debian-stretch")]
        [InlineData("10", "10", "7", "7", "8", "8", "6", "6")]
        [InlineData("", "5", "", "2", "", "3", "", "1")] // defaults
        [InlineData("false", null, "5", null, "5", null, "5", null, "pm.max_children must be a positive value")]
        [InlineData("-1", null, "5", null, "5", null, "5", null, "pm.max_children must be a positive value")]
        [InlineData("10", null, "5", null, "13", null, "12", null, "pm.min_spare_servers(12) and pm.max_spare_servers(13) cannot be greater than pm.max_children(10)")]
        [InlineData("10", null, "9", null, "8", null, "7", null, "pm.start_servers(9) must not be less than pm.min_spare_servers(7) and not greater than pm.max_spare_servers(8)")]
        public async Task PipelineTestInvocationsPhp74Async(
            string fpmMaxChildren, string expectedFpmMaxChildren,
            string fpmStartServers, string expectedFpmStartServers,
            string fpmMaxSpareServers, string expectedFpmMaxSpareServers,
            string fpmMinSpareServers, string expectedFpmMinSpareServers,
            string failureOutputText = default)
        {
            await PhpFpmConfigTestAsync(
                "7.4",
                ImageTestHelperConstants.OsTypeDebianBullseye,
                fpmMaxChildren, expectedFpmMaxChildren,
                fpmStartServers, expectedFpmStartServers,
                fpmMaxSpareServers, expectedFpmMaxSpareServers,
                fpmMinSpareServers, expectedFpmMinSpareServers,
                failureOutputText);
        }

        [Fact, Trait("category", "php-8.0")]
        public async Task PhpFpmNginxCustomizationTestAsync()
        {
            // Arrange
            var appName = "php-fpm-config";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var phpVersion = "8.0";
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var nginxDummyCustomConfigFile = appDir + "/NGINX_DUMMY_CUSTOM_CONFIG_FILE.conf";
            var nginxCustomCommand1 = "cp " + nginxDummyCustomConfigFile + " /etc/nginx/nginx.conf";
            var nginxCustomCommand2 = "service nginx reload";

            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
               $"--platform php --platform-version {phpVersion}")
               .ToString();

            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -output {RunScriptPath} -bindPort {ContainerPort}")
                .AddCommand("mkdir -p /home/site/wwwroot")
                .AddCommand($"cp -rf {appOutputDir}/* /home/site/wwwroot")
                .AddCommand("cat " + RunScriptPath)
                .AddStringExistsInFileCheck(nginxCustomCommand1, RunScriptPath)
                .AddStringExistsInFileCheck(nginxCustomCommand2, RunScriptPath)
                .AddCommand(RunScriptPath)
                .ToString();
            var phpimageVersion = string.Concat(phpVersion, "-", "fpm");
            // Act & Assert success conditions
                await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                    appName,
                    _output,
                    new[] { volume, appOutputDirVolume },
                    Settings.BuildImageName,
                    "/bin/sh",
                    new[] { "-c", buildScript },
                    _imageHelper.GetRuntimeImage("php", phpimageVersion, osType),
                    new List<EnvironmentVariable>()
                    {
                    new EnvironmentVariable(ExtVarNames.NginxConfFile, nginxDummyCustomConfigFile)
                    },
                    ContainerPort,
                    "/bin/sh",
                    new[] { "-c", runScript },
                    async (hostPort) =>
                    {
                        var output = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                        Assert.Contains("Hello World!", output);
                    });
        }

        private async Task PhpFpmConfigTestAsync(
            string phpVersion,
            string osType,
            string fpmMaxChildren, string expectedFpmMaxChildren,
            string fpmStartServers, string expectedFpmStartServers,
            string fpmMaxSpareServers, string expectedFpmMaxSpareServers,
            string fpmMinSpareServers, string expectedFpmMinSpareServers,
            string failureOutputText)
        {
            // Arrange
            var appName = "php-fpm-config";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;

            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
               $"--platform php --platform-version {phpVersion}")
               .ToString();

            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -output {RunScriptPath} -bindPort {ContainerPort}")
                .AddCommand("mkdir -p /home/site/wwwroot")
                .AddCommand($"cp -rf {appOutputDir}/* /home/site/wwwroot")
                .AddCommand(RunScriptPath)
                .ToString();

            var phpimageVersion = string.Concat(phpVersion, "-", "fpm");

            if (string.IsNullOrEmpty(failureOutputText))
            {
                // Act & Assert success conditions
                await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                    appName,
                    _output,
                    new[] { volume, appOutputDirVolume },
                    Settings.BuildImageName,
                    "/bin/sh",
                    new[] { "-c", buildScript },
                    _imageHelper.GetRuntimeImage("php", phpimageVersion, osType),
                    new List<EnvironmentVariable>()
                    {
                    new EnvironmentVariable(ExtVarNames.PhpFpmMaxChildrenEnvVarName, fpmMaxChildren),
                    new EnvironmentVariable(ExtVarNames.PhpFpmStartServersEnvVarName, fpmStartServers),
                    new EnvironmentVariable(ExtVarNames.PhpFpmMaxSpareServersEnvVarName, fpmMaxSpareServers),
                    new EnvironmentVariable(ExtVarNames.PhpFpmMinSpareServersEnvVarName, fpmMinSpareServers),
                    },
                    ContainerPort,
                    "/bin/sh",
                    new[] { "-c", runScript },
                    async (hostPort) =>
                    {
                        var output = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                        Assert.Contains($"pm = dynamic", output);
                        Assert.Contains($"pm.max_children = {expectedFpmMaxChildren}", output);
                        Assert.Contains($"pm.start_servers = {expectedFpmStartServers}", output);
                        Assert.Contains($"pm.max_spare_servers = {expectedFpmMaxSpareServers}", output);
                        Assert.Contains($"pm.min_spare_servers = {expectedFpmMinSpareServers}", output);
                    });
            } else {

                // We expect that docker will be able to pull the image,
                // start the container, and exit well before this time limit is up
                // as the startup script should fail. This is a fallback in case this does
                // not happen.
                var waitTimeForContainerExit = TimeSpan.FromMinutes(5);

                // Act & Assert failure conditions
                var debugText = await EndToEndTestHelper.BuildRunAndAssertFailureAsync(
                    _output,
                    new[] { volume, appOutputDirVolume },
                    Settings.BuildImageName,
                    "/bin/sh",
                    new[] { "-c", buildScript },
                    _imageHelper.GetRuntimeImage("php", phpimageVersion, osType),
                    new List<EnvironmentVariable>()
                    {
                    new EnvironmentVariable(ExtVarNames.AppServiceAppNameEnvVarName, appName),
                    new EnvironmentVariable(ExtVarNames.PhpFpmMaxChildrenEnvVarName, fpmMaxChildren),
                    new EnvironmentVariable(ExtVarNames.PhpFpmStartServersEnvVarName, fpmStartServers),
                    new EnvironmentVariable(ExtVarNames.PhpFpmMaxSpareServersEnvVarName, fpmMaxSpareServers),
                    new EnvironmentVariable(ExtVarNames.PhpFpmMinSpareServersEnvVarName, fpmMinSpareServers),
                    },
                    ContainerPort,
                    link: null,
                    "/bin/sh",
                    new[] { "-c", runScript },
                    waitTimeForContainerExit);

                Assert.Contains("ERROR: failed to post process the configuration", debugText);
                Assert.Contains("ERROR: FPM initialization failed", debugText);
                Assert.Contains(failureOutputText, debugText);
            }
        }
    }
}