// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.Common;
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

        [Fact]
        public async Task NodeApp_MicrosoftSqlServerDB()
        {
            // Arrange
            var appName = "node-mssql";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { volume },
                Settings.BuildImageName,
                "oryx",
                new[] { "build", appDir, "-l", "nodejs", "--language-version", "10.14" },
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

        [Fact]
        public async Task Python37App_MicrosoftSqlServerDB()
        {
            // Arrange
            var appName = "mssqlserver-sample";
            var hostDir = Path.Combine(_hostSamplesDir, "python", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { volume },
                Settings.BuildImageName,
                "oryx",
                new[] { "build", appDir, "-l", "python", "--language-version", "3.7" },
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
        [InlineData("7.3")]
        [InlineData("7.2")]
        // pdo_sqlsrv only supports PHP >= 7.1
        public async Task PhpApp_UsingPdo(string phpVersion)
        {
            // Arrange
            var appName = "sqlsrv-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { volume },
                Settings.BuildImageName,
                "oryx",
                new[] { "build", appDir, "-l", "php", "--language-version", phpVersion },
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