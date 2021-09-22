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

        [Theory]
        [InlineData("github-actions")]
        [InlineData("github-actions-buster")]
        [InlineData("latest")]
        public async Task NodeApp_PostgreSqlDB(string imageTag)
        {
            await RunTestAsync(
                "nodejs",
                "10.14",
                Path.Combine(HostSamplesDir, "nodejs", "node-postgres"),
                buildImageName: _imageHelper.GetBuildImage(imageTag));
        }

        [Theory]
        [InlineData("github-actions")]
        [InlineData("github-actions-buster")]
        [InlineData("latest")]
        public async Task Python37App_PostgreSqlDB(string imageTag)
        {
            await RunTestAsync(
                "python",
                "3.7",
                Path.Combine(HostSamplesDir, "python", "postgres-sample"),
                buildImageName: _imageHelper.GetBuildImage(imageTag));
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