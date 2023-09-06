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
    public class PythonSqlServerIntegrationTests : PlatformEndToEndTestsBase
    {
        private const int ContainerPort = 3000;
        private const string DefaultStartupFilePath = "./run.sh";

        public PythonSqlServerIntegrationTests(ITestOutputHelper output) : base(output, null)
        {
        }

        [Fact(Skip = "Bug #1274414")]
        [Trait("category", "python-3.7")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        public async Task Python37App_MicrosoftSqlServerDB_WithGitHubActionsBullseyeBuildImageAsync()
        {
            await PythonApp_MicrosoftSqlServerDBAsync("3.7", ImageTestHelperConstants.OsTypeDebianBullseye, ImageTestHelperConstants.GitHubActionsBullseye);
        }

        [Fact(Skip = "Bug #1274414")]
        [Trait("category", "python-3.7")]
        [Trait("build-image", "debian-stretch")]
        public async Task Python37App_MicrosoftSqlServerDB_WithLatestStretchBuildImageAsync()
        {
            await PythonApp_MicrosoftSqlServerDBAsync("3.7", ImageTestHelperConstants.OsTypeDebianBullseye, ImageTestHelperConstants.LatestStretchTag);
        }

        private async Task PythonApp_MicrosoftSqlServerDBAsync(string pythonVersion, string osType, string buildImageTag)
        {
            // Arrange
            var appName = "mssqlserver-sample";
            var hostDir = Path.Combine(_hostSamplesDir, "python", appName);
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
                new[] { "build", appDir, "--platform", "python", "--platform-version", pythonVersion },
                _imageHelper.GetRuntimeImage("python", pythonVersion, osType),
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