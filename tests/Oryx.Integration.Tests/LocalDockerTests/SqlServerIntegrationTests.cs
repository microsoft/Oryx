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

        public SqlServerIntegrationTests(ITestOutputHelper output, Fixtures.SqlServerDbContainerFixture dbFixture) : base(output, dbFixture, hostPort)
        {
        }

        [Fact(Skip = "#823483: Skip failing DB tests for now")]
        public async Task NodeApp_MicrosoftSqlServerDB()
        {
            await RunTestAsync(
                "nodejs",
                "10.14",
                Path.Combine(HostSamplesDir, "nodejs", "node-mssql"),
                _dbFixture.DbServerContainerName);
        }

        [Fact(Skip = "#823483: Skip failing DB tests for now")]
        public async Task Python37App_MicrosoftSqlServerDB()
        {
            await RunTestAsync(
                "python",
                "3.7",
                Path.Combine(HostSamplesDir, "python", "mssqlserver-sample"),
                _dbFixture.DbServerContainerName);
        }
    }
}