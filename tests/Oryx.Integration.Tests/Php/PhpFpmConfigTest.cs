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
        [InlineData("1000", "1000")]
        [InlineData("", "5")]
        public async Task PipelineTestInvocationsPhp80(
            string fpmMaxChildren, string expectedFpmMaxChildren)
        {
            await PhpFpmConfigTestAsync("8.0", fpmMaxChildren, expectedFpmMaxChildren);
        }

        [Theory, Trait("category", "php-8.0")]
        [InlineData("false", "pm.max_children must be a positive value")]
        [InlineData("-1", "pm.max_children must be a positive value")]
        public async Task PipelineTestFailInvocationsPhp80(string fpmMaxChildren, string failureOutputText)
        {
            await PhpFpmConfigTestFailuresAsync("8.0", fpmMaxChildren, failureOutputText);
        }

        [Theory, Trait("category", "php-7.4")]
        [InlineData("1000", "1000")]
        [InlineData("", "5")]
        public async Task PipelineTestInvocationsPhp74(string fpmMaxChildren, string expectedFpmMaxChildren)
        {
            await PhpFpmConfigTestAsync("7.4", fpmMaxChildren, expectedFpmMaxChildren);
        }

        [Theory, Trait("category", "php-7.4")]
        [InlineData("false", "pm.max_children must be a positive value")]
        [InlineData("-1", "pm.max_children must be a positive value")]
        public async Task PipelineTestFailInvocationsPhp74(string fpmMaxChildren, string failureOutputText)
        {
            await PhpFpmConfigTestFailuresAsync("7.4", fpmMaxChildren, failureOutputText);
        }

        private async Task PhpFpmConfigTestAsync(
            string phpVersion, 
            string fpmMaxChildren, 
            string expectedFpmMaxChildren)
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
                .AddCommand($"cat {RunScriptPath}")
                .AddCommand(RunScriptPath)
                .ToString();

            var phpimageVersion = string.Concat(phpVersion, "-", "fpm");

            // Act & Assert
            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName, 
                _output, 
                new[] { volume, appOutputDirVolume }, 
                Settings.BuildImageName,
                "/bin/sh", 
                new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("php", phpimageVersion),
                new List<EnvironmentVariable>()
                {
                    new EnvironmentVariable(ExtVarNames.PhpFpmMaxChildrenEnvVarName, fpmMaxChildren),
                },
                ContainerPort,
                "/bin/sh", 
                new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var output = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains($"pm.max_children = {expectedFpmMaxChildren}", output);
                });
        }

        private async Task PhpFpmConfigTestFailuresAsync(
            string phpVersion,
            string fpmMaxChildren,
            string failureOutputText)
        {
            // Arrange
            var appName = "php-fpm-config";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;

            // We expect that the container will exit well before this time limit is up
            // as the startup script should fail. This is a fallback in case this does
            // not happen.
            var waitTimeForContainerExit = TimeSpan.FromSeconds(30);

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

            // Act & Assert
            var debugText = await EndToEndTestHelper.BuildRunAndAssertFailureAsync(
                _output,
                new[] { volume, appOutputDirVolume },
                Settings.BuildImageName,
                "/bin/sh",
                new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("php", phpimageVersion),
                new List<EnvironmentVariable>()
                {
                    new EnvironmentVariable(ExtVarNames.AppServiceAppNameEnvVarName, appName),
                    new EnvironmentVariable(ExtVarNames.PhpFpmMaxChildrenEnvVarName, fpmMaxChildren),
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