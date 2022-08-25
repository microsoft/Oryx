﻿// --------------------------------------------------------------------------------------------
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
    [Trait("db", "sqlserver")]
    public class PhpSqlServerIntegrationTests : PlatformEndToEndTestsBase
    {
        private const int ContainerPort = 3000;
        private const string DefaultStartupFilePath = "./run.sh";

        public PhpSqlServerIntegrationTests(ITestOutputHelper output) : base(output, null)
        {
        }

        // Unique category traits are needed to run each
        // platform-version in it's own pipeline agent. This is
        // because our agents currently a space limit of 10GB.

        [Fact, Trait("category", "php-80")]
        public async Task PipelineTestInvocationsPhp80Async()
        {   
            string phpVersion80 = "8.0";
            await Task.WhenAll(
                PhpApp_UsingPdoAsync(phpVersion80, ImageTestHelperConstants.GitHubActionsStretch),
                PhpApp_UsingPdoAsync(phpVersion80, ImageTestHelperConstants.GitHubActionsBuster),
                PhpApp_UsingPdoAsync(phpVersion80, ImageTestHelperConstants.LatestStretchTag));
        }

        [Fact, Trait("category", "php-74")]
        public async Task PipelineTestInvocationsPhp74Async()
        {
            string phpVersion74 = "7.4";
            await Task.WhenAll(
                PhpApp_UsingPdoAsync(phpVersion74, ImageTestHelperConstants.GitHubActionsStretch),
                PhpApp_UsingPdoAsync(phpVersion74, ImageTestHelperConstants.GitHubActionsBuster),
                PhpApp_UsingPdoAsync(phpVersion74, ImageTestHelperConstants.LatestStretchTag));
        }

        [Theory]
        [InlineData("7.4", ImageTestHelperConstants.GitHubActionsStretch)]
        [InlineData("7.4", ImageTestHelperConstants.GitHubActionsBuster)]
        [InlineData("7.4", ImageTestHelperConstants.LatestStretchTag)]
        [InlineData("8.0", ImageTestHelperConstants.GitHubActionsStretch)]
        [InlineData("8.0", ImageTestHelperConstants.GitHubActionsBuster)]
        [InlineData("8.0", ImageTestHelperConstants.LatestStretchTag)]
        public async Task PhpApp_UsingPdoAsync(string phpVersion, string imageTag)
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
                _imageHelper.GetBuildImage(imageTag),
                "oryx",
                new[] { "build", appDir, "--platform", "php", "--platform-version", phpVersion },
                _imageHelper.GetRuntimeImage("php", phpVersion),
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