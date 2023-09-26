// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Integration.Tests.Fixtures;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("db", "sqlserver")]
    public class PhpSqlServerIntegrationTests : PlatformEndToEndTestsBase
    {
        private const int ContainerPort = 3000;
        private const string DefaultStartupFilePath = "./run.sh";

        public PhpSqlServerIntegrationTests(ITestOutputHelper output) : base(output, null)
        {
        }

        [Fact]
        [Trait("category", "7.4")]
        [Trait("build-image", "debian-stretch")]
        public async Task Php74App_UsingPdo_WithLatestStretchBuildImageAsync()
        {
            await PhpApp_UsingPdoAsync("7.4", ImageTestHelperConstants.OsTypeDebianBullseye, ImageTestHelperConstants.LatestStretchTag);
        }

        [Fact]
        [Trait("category", "7.4")]
        [Trait("build-image", "github-actions-debian-buster")]
        public async Task Php74App_UsingPdo_WithGitHubActionsBusterBuildImageAsync()
        {
            await PhpApp_UsingPdoAsync("7.4", ImageTestHelperConstants.OsTypeDebianBullseye, ImageTestHelperConstants.GitHubActionsBuster);
        }

        [Fact]
        [Trait("category", "8.0")]
        [Trait("build-image", "debian-stretch")]
        public async Task Php80App_UsingPdo_WithLatestStretchBuildImageAsync()
        {
            await PhpApp_UsingPdoAsync("8.0", ImageTestHelperConstants.OsTypeDebianBullseye, ImageTestHelperConstants.LatestStretchTag);
        }

        [Fact]
        [Trait("category", "8.0")]
        [Trait("build-image", "github-actions-debian-buster")]
        public async Task Php80App_UsingPdo_WithGitHubActionsBusterBuildImageAsync()
        {
            await PhpApp_UsingPdoAsync("8.0", ImageTestHelperConstants.OsTypeDebianBullseye, ImageTestHelperConstants.GitHubActionsBuster);
        }

        private async Task PhpApp_UsingPdoAsync(string phpVersion, string osType, string buildImageTag)
        {
            // Arrange
            var appName = "sqlsrv-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { volume },
                _imageHelper.GetBuildImage(buildImageTag),
                "oryx",
                new[] { "build", appDir, "--platform", "php", "--platform-version", phpVersion },
                _imageHelper.GetRuntimeImage("php", phpVersion, osType),
                SqlServerDbTestHelper.GetEnvironmentVariables(),
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    script
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Equal(
                        DbContainerFixtureBase.GetSampleDataAsJson(),
                        data.Trim(),
                        ignoreLineEndingDifferences: true,
                        ignoreWhiteSpaceDifferences: true);
                });
        }

    }
}