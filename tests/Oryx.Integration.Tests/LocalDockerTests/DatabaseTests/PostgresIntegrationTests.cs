// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.Integration.Tests.LocalDockerTests
{
    public class PostgresIntegrationTests : DatabaseTestsBase, IClassFixture<PostgresDatabaseSetupFixture>
    {
        private const int hostPort = 8087;
        private readonly PostgresDatabaseSetupFixture _postgresDatabaseSetupFixture;

        public PostgresIntegrationTests(
            ITestOutputHelper output,
            PostgresDatabaseSetupFixture postgresDatabaseSetupFixture)
            : base(output, hostPort)
        {
            _postgresDatabaseSetupFixture = postgresDatabaseSetupFixture;
        }

        [Fact]
        public async Task NodeApp_PostgresDB()
        {
            await RunTestAsync(
                "nodejs",
                "10.14",
                Path.Combine(HostSamplesDir, "nodejs", "node-postgres"),
                _postgresDatabaseSetupFixture.DatabaseServerContainerName);
        }

        [Fact]
        public async Task Python37App_PostgreSqlDB()
        {
            await RunTestAsync(
                "python",
                "3.7",
                Path.Combine(HostSamplesDir, "python", "postgres-sample"),
                _postgresDatabaseSetupFixture.DatabaseServerContainerName);
        }
    }
}