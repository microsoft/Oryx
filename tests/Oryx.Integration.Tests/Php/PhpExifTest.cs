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
    public class PhpExifTest : PhpEndToEndTestsBase
    {
        private const string ExifImageTypePng = "3";

        public PhpExifTest(ITestOutputHelper output, TestTempDirTestFixture fixture)
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
            string phpVersion83 = "8.3";
            await Task.WhenAll(
                ExifExampleAsync(phpVersion83, ImageTestHelperConstants.OsTypeDebianBullseye),
                PhpFpmExifExampleAsync(phpVersion83, ImageTestHelperConstants.OsTypeDebianBullseye));
        }

        [Fact, Trait("category", "php-8.2")]
        [Trait("build-image", "debian-stretch")]
        public async Task PipelineTestInvocationsPhp82Async()
        {
            string phpVersion82 = "8.2";
            await Task.WhenAll(
                ExifExampleAsync(phpVersion82, ImageTestHelperConstants.OsTypeDebianBullseye),
                PhpFpmExifExampleAsync(phpVersion82, ImageTestHelperConstants.OsTypeDebianBullseye));
        }

        [Fact, Trait("category", "php-8.1")]
        [Trait("build-image", "debian-stretch")]
        public async Task PipelineTestInvocationsPhp81Async()
        {   
            string phpVersion81 = "8.1";
            await Task.WhenAll(
                ExifExampleAsync(phpVersion81, ImageTestHelperConstants.OsTypeDebianBullseye),
                PhpFpmExifExampleAsync(phpVersion81, ImageTestHelperConstants.OsTypeDebianBullseye));
        }

        [Fact, Trait("category", "php-8.0")]
        [Trait("build-image", "debian-stretch")]
        public async Task PipelineTestInvocationsPhp80Async()
        {   
            string phpVersion80 = "8.0";
            await Task.WhenAll(
                ExifExampleAsync(phpVersion80, ImageTestHelperConstants.OsTypeDebianBullseye),
                PhpFpmExifExampleAsync(phpVersion80, ImageTestHelperConstants.OsTypeDebianBullseye));
        }

        [Fact, Trait("category", "php-7.4")]
        [Trait("build-image", "debian-stretch")]
        public async Task PipelineTestInvocationsPhp74Async()
        {
            string phpVersion74 = "7.4";
            await Task.WhenAll(
                ExifExampleAsync(phpVersion74, ImageTestHelperConstants.OsTypeDebianBullseye),
                PhpFpmExifExampleAsync(phpVersion74, ImageTestHelperConstants.OsTypeDebianBullseye));
        }

        private async Task ExifExampleAsync(string phpVersion, string osType)
        {
            // Arrange
            var appName = "exif-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
               $"--platform {PhpConstants.PlatformName} --platform-version {phpVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -output {RunScriptPath}")
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
                    string exifOutput = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    // The test app: `echo exif_imagetype('64x64.png')`
                    Assert.Equal(ExifImageTypePng, exifOutput);
                });
        }

        private async Task PhpFpmExifExampleAsync(string phpVersion, string osType)
        {
            // Arrange
            var appName = "exif-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var phpimageVersion = string.Concat(phpVersion, "-", "fpm");
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
               $"--platform {PhpConstants.PlatformName} --platform-version {phpVersion}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand("mkdir -p /home/site/wwwroot")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -output {RunScriptPath}")
                .AddCommand($"cp -rf {appOutputDir}/* /home/site/wwwroot")
                .AddCommand(RunScriptPath)
                .ToString();

            // Act & Assert
            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName, _output, new[] { volume, appOutputDirVolume },
                "/bin/sh", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("php", phpimageVersion, osType),
                ContainerPort,
                "/bin/sh", new[] { "-c", runScript },
                async (hostPort) =>
                {
                    string exifOutput = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    // The test app: `echo exif_imagetype('64x64.png')`
                    Assert.Equal(ExifImageTypePng, exifOutput);
                });
        }
    }
}
