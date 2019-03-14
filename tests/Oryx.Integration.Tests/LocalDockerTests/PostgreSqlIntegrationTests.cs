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
    public class PostgreSqlIntegrationTests : DatabaseTestsBase, IClassFixture<Fixtures.PostgreSqlDbContainerFixture>
    {
        private const int hostPort = 8087;
        private readonly Fixtures.PostgreSqlDbContainerFixture _postgresDatabaseSetupFixture;

        public PostgreSqlIntegrationTests(
            ITestOutputHelper output,
            Fixtures.PostgreSqlDbContainerFixture postgresDatabaseSetupFixture)
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
                _postgresDatabaseSetupFixture.DbServerContainerName);
        }

        [Fact]
        public async Task Python37App_PostgreSqlDB()
        {
            await RunTestAsync(
                "python",
                "3.7",
                Path.Combine(HostSamplesDir, "python", "postgres-sample"),
                _postgresDatabaseSetupFixture.DbServerContainerName);
        }
    }
}