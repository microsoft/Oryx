// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Collection("Php integration")]
    [Trait("db", "postgres")]
    public class PhpPostgreSqlIntegrationTests : DatabaseTestsBase, IClassFixture<Fixtures.PostgreSqlDbContainerFixture>
    {
        public PhpPostgreSqlIntegrationTests(ITestOutputHelper output, Fixtures.PostgreSqlDbContainerFixture dbFixture)
            : base(output, dbFixture)
        {
        }

        [Fact(Skip = "Bug #1410367")]
        [Trait("category", "php-7.4")]
        [Trait("build-image", "debian-stretch")]
        public async Task Php74AppAsync()
        {
            await PhpAppAsync("7.4", ImageTestHelperConstants.OsTypeDebianBullseye);
        }

        private async Task PhpAppAsync(string phpVersion, string osType)
        {
            await RunTestAsync(
                "php",
                phpVersion,
                osType,
                Path.Combine(HostSamplesDir, "php", "pgsql-example"),
                8080,
                specifyBindPortFlag: false);
        }
    }
}