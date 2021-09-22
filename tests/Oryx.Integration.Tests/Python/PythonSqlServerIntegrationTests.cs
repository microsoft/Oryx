// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
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
    [Trait("category", "python")]
    [Trait("db", "sqlserver")]
    public class PythonSqlServerIntegrationTests : PlatformEndToEndTestsBase
    {
        private const int ContainerPort = 3000;
        private const string DefaultStartupFilePath = "./run.sh";

        public PythonSqlServerIntegrationTests(ITestOutputHelper output) : base(output, null)
        {
        }

        [Theory(Skip = "Bug #1274414")]
        [InlineData("github-actions")]
        [InlineData("github-actions-buster")]
        [InlineData("latest")]
        public async Task Python37App_MicrosoftSqlServerDB(string imageTag)
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
                _imageHelper.GetBuildImage(imageTag),
                "oryx",
                new[] { "build", appDir, "--platform", "python", "--platform-version", "3.7" },
                _imageHelper.GetRuntimeImage("python", "3.7"),
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