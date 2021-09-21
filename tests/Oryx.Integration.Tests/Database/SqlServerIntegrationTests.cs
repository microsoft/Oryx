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
    [Trait("category", "db")]
    [Trait("db", "sqlserver")]
    public class SqlServerIntegrationTests : PlatformEndToEndTestsBase
    {
        private const string DbServerHostnameEnvVarName = "SQLSERVER_DATABASE_HOST";
        private const string DbServerUsernameEnvVarName = "SQLSERVER_DATABASE_USERNAME";
        private const string DbServerPasswordEnvVarName = "SQLSERVER_DATABASE_PASSWORD";
        private const string DbServerDatabaseEnvVarName = "SQLSERVER_DATABASE_NAME";

        private const int ContainerPort = 3000;
        private const string DefaultStartupFilePath = "./run.sh";

        public SqlServerIntegrationTests(ITestOutputHelper output) : base(output, null)
        {
        }

        [Theory]
        [InlineData("github-actions")]
        [InlineData("github-actions-buster")]
        [InlineData("latest")]
        public async Task NodeApp_MicrosoftSqlServerDB(string imageTag)
        {
            // Arrange
            var appName = "node-mssql";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
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
                new[] { "build", appDir, "--platform", "nodejs", "--platform-version", "10.14" },
                _imageHelper.GetRuntimeImage("node", "10.14"),
                GetEnvironmentVariables(),
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
                GetEnvironmentVariables(),
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

        [Theory]
        [InlineData("7.3", "github-actions")]
        [InlineData("7.3", "github-actions-buster")]
        [InlineData("7.3", "latest")]
        [InlineData("7.4", "github-actions")]
        [InlineData("7.4", "github-actions-buster")]
        [InlineData("7.4", "latest"]
        [InlineData("8.0", "github-actions")]
        [InlineData("8.0", "github-actions-buster")]
        [InlineData("8.0", "latest")]
        // pdo_sqlsrv only supports PHP >= 7.3
        public async Task PhpApp_UsingPdo(string phpVersion, string imageTag)
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
                GetEnvironmentVariables(),
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

        private List<EnvironmentVariable> GetEnvironmentVariables()
        {
            return new List<EnvironmentVariable>
            {
                new EnvironmentVariable(
                    DbServerHostnameEnvVarName, Environment.GetEnvironmentVariable(DbServerHostnameEnvVarName)),
                new EnvironmentVariable(
                    DbServerDatabaseEnvVarName, Environment.GetEnvironmentVariable(DbServerDatabaseEnvVarName)),
                new EnvironmentVariable(
                    DbServerUsernameEnvVarName, Environment.GetEnvironmentVariable(DbServerUsernameEnvVarName)),
                new EnvironmentVariable(
                    DbServerPasswordEnvVarName, Environment.GetEnvironmentVariable(DbServerPasswordEnvVarName)),
            };
        }
    }
}
