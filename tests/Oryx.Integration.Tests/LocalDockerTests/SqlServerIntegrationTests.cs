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
        private readonly Fixtures.SqlServerDbContainerFixture _msSqlServerDatabaseSetupFixture;

        public SqlServerIntegrationTests(
            ITestOutputHelper output,
            Fixtures.SqlServerDbContainerFixture msSqlServerDatabaseSetupFixture)
            : base(output, hostPort)
        {
            _msSqlServerDatabaseSetupFixture = msSqlServerDatabaseSetupFixture;
        }

        [Fact]
        public async Task NodeApp_MicrosoftSqlServerDB()
        {
            await RunTestAsync(
                "nodejs",
                "10.14",
                Path.Combine(HostSamplesDir, "nodejs", "node-mssql"),
                _msSqlServerDatabaseSetupFixture.DbServerContainerName);
        }

        [Fact]
        public async Task Python37App_MicrosoftSqlServerDB()
        {
            await RunTestAsync(
                "python",
                "3.7",
                Path.Combine(HostSamplesDir, "python", "mssqlserver-sample"),
                _msSqlServerDatabaseSetupFixture.DbServerContainerName);
        }
    }
}