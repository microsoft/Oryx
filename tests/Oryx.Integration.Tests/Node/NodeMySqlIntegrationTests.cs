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
    [Trait("category", "node-12-2")]
    [Trait("db", "mysql")]
    public class NodeMySqlIntegrationTests : DatabaseTestsBase, IClassFixture<Fixtures.MySqlDbContainerFixture>
    {
        public NodeMySqlIntegrationTests(ITestOutputHelper output, Fixtures.MySqlDbContainerFixture dbFixture)
            : base(output, dbFixture)
        {
        }

        [Theory(Skip = "bug: 1505700 may be intermittent")]
        [InlineData("latest")]
        [InlineData("github-actions")]
        public async Task NodeApp_MySqlDB(string imageTag)
        {
            await RunTestAsync(
                "nodejs",
                "12",
                Path.Combine(HostSamplesDir, "nodejs", "node-mysql"),
                buildImageName: _imageHelper.GetBuildImage(imageTag));
        }

    }
}