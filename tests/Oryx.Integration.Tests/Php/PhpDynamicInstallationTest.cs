// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public class PhpDynamicInstallationTest : PhpEndToEndTestsBase
    {
        public PhpDynamicInstallationTest(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        // Unique category traits are needed to run each
        // platform-version in it's own pipeline agent. This is
        // because our agents currently a space limit of 10GB.
        [Fact, Trait("category", "php-8.2")]
        [Trait("build-image", "github-actions-debian-buster")]
        public async Task PipelineTestInvocationsPhp82Async()
        {
            await CanBuildAndRunAppAsync("8.2", ImageTestHelperConstants.OsTypeDebianBullseye);
        }

        [Fact, Trait("category", "php-8.1")]
        [Trait("build-image", "github-actions-debian-buster")]
        public async Task PipelineTestInvocationsPhp81Async()
        {   
            await CanBuildAndRunAppAsync("8.1", ImageTestHelperConstants.OsTypeDebianBullseye);
        }

        [Fact, Trait("category", "php-8.0")]
        [Trait("build-image", "github-actions-debian-buster")]
        public async Task PipelineTestInvocationsPhp80Async()
        {   
            await CanBuildAndRunAppAsync("8.0", ImageTestHelperConstants.OsTypeDebianBullseye);
        }

        [Fact, Trait("category", "php-7.4")]
        [Trait("build-image", "github-actions-debian-buster")]
        public async Task PipelineTestInvocationsPhp74Async()
        {
            await CanBuildAndRunAppAsync("7.4", ImageTestHelperConstants.OsTypeDebianBullseye);
        }

        private async Task CanBuildAndRunAppAsync(string phpVersion, string osType)
        {
            // Arrange
            var exifImageTypePng = "3";
            var appName = "exif-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PhpConstants.PlatformName} --platform-version {phpVersion}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -output {RunScriptPath}")
                .AddCommand(RunScriptPath)
                .ToString();

            // Act & Assert
            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBuster),
                "/bin/sh", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("php", phpVersion, osType),
                ContainerPort,
                "/bin/sh", new[] { "-c", runScript },
                async (hostPort) =>
                {
                    string exifOutput = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    // The test app: `echo exif_imagetype('64x64.png')`
                    Assert.Equal(exifImageTypePng, exifOutput);
                });
        }
    }
}