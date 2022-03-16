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
    [Trait("category", "node-14")]
    [Trait("db", "postgres")]
    public class NodePostgreSqlIntegrationTests : DatabaseTestsBase, IClassFixture<Fixtures.PostgreSqlDbContainerFixture>
    {
        public NodePostgreSqlIntegrationTests(ITestOutputHelper output, Fixtures.PostgreSqlDbContainerFixture dbFixture)
            : base(output, dbFixture)
        {
        }

        [Theory (Skip = "Bug 1410367")]
        [InlineData("github-actions")]
        [InlineData("latest")]
        public async Task NodeApp_PostgreSqlDB(string imageTag)
        {
            await RunTestAsync(
                "nodejs",
                "14",
                Path.Combine(HostSamplesDir, "nodejs", "node-postgres"),
                buildImageName: _imageHelper.GetBuildImage(imageTag));
        }
    }
}