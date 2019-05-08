// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "db")]
    [Trait("db", "postgres")]
    public class PostgreSqlIntegrationTests : DatabaseTestsBase, IClassFixture<Fixtures.PostgreSqlDbContainerFixture>
    {
        public PostgreSqlIntegrationTests(ITestOutputHelper output, Fixtures.PostgreSqlDbContainerFixture dbFixture)
            : base(output, dbFixture)
        {
        }

        [Fact]
        public async Task NodeApp_PostgreSqlDB()
        {
            await RunTestAsync("nodejs", "10.14", Path.Combine(HostSamplesDir, "nodejs", "node-postgres"));
        }

        [Fact]
        public async Task Python37App_PostgreSqlDB()
        {
            await RunTestAsync("python", "3.7", Path.Combine(HostSamplesDir, "python", "postgres-sample"));
        }

        [Theory]
        [InlineData("7.3")]
        [InlineData("7.2")]
        [InlineData("7.0")]
        [InlineData("5.6")]
        public async Task PhpApp(string phpVersion)
        {
            await RunTestAsync(
                "php",
                phpVersion,
                Path.Combine(HostSamplesDir, "php", "pgsql-example"),
                8080,
                specifyBindPortFlag: false);
        }
    }
}