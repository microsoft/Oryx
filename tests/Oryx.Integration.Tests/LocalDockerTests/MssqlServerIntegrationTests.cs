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
    public class MssqlServerIntegrationTests : DatabaseTestsBase, IClassFixture<Fixtures.MSSqlServerDatabaseSetupFixture>
    {
        private const int hostPort = 8085;
        private readonly Fixtures.MSSqlServerDatabaseSetupFixture _msSqlServerDatabaseSetupFixture;

        public MssqlServerIntegrationTests(
            ITestOutputHelper output,
            Fixtures.MSSqlServerDatabaseSetupFixture msSqlServerDatabaseSetupFixture)
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
                _msSqlServerDatabaseSetupFixture.DatabaseServerContainerName);
        }

        [Fact]
        public async Task Python37App_MicrosoftSqlServerDB()
        {
            await RunTestAsync(
                "python",
                "3.7",
                Path.Combine(HostSamplesDir, "python", "mssqlserver-sample"),
                _msSqlServerDatabaseSetupFixture.DatabaseServerContainerName);
        }
    }
}