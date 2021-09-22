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
    [Trait("category", "php")]
    [Trait("db", "postgres")]
    public class PhpPostgreSqlIntegrationTests : DatabaseTestsBase, IClassFixture<Fixtures.PostgreSqlDbContainerFixture>
    {
        public PhpPostgreSqlIntegrationTests(ITestOutputHelper output, Fixtures.PostgreSqlDbContainerFixture dbFixture)
            : base(output, dbFixture)
        {
        }

        [Theory]
        [InlineData("7.2")]
        [InlineData("7.3")]
        [InlineData("7.4")]
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