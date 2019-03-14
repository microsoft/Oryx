// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests.LocalDockerTests
{
    public class SqlServerIntegrationTests : DatabaseTestsBase, IClassFixture<Fixtures.SqlServerDbContainerFixture>
    {
        private const int hostPort = 8085;
        private readonly Fixtures.SqlServerDbContainerFixture _dbFixture;

        public SqlServerIntegrationTests(ITestOutputHelper output, Fixtures.SqlServerDbContainerFixture dbFixture)
            : base(output, hostPort)
        {
            _dbFixture = dbFixture;
        }

        [Fact]
        public async Task NodeApp_MicrosoftSqlServerDB()
        {
            await RunTestAsync(
                "nodejs",
                "10.14",
                Path.Combine(HostSamplesDir, "nodejs", "node-mssql"),
                _dbFixture.DbServerContainerName);
        }

        [Fact]
        public async Task Python37App_MicrosoftSqlServerDB()
        {
            await RunTestAsync(
                "python",
                "3.7",
                Path.Combine(HostSamplesDir, "python", "mssqlserver-sample"),
                _dbFixture.DbServerContainerName);
        }

        [Theory]
        [InlineData("7.3")]
        [InlineData("7.2")]
        // pdo_sqlsrv only supports PHP >= 7.1
        public async Task PhpApp_UsingPdo(string phpVersion)
        {
            await RunTestAsync("php", phpVersion, Path.Combine(HostSamplesDir, "php", "sqlsrv-example"), _msSqlServerDatabaseSetupFixture.DatabaseServerContainerName, 80);
        }
    }
}