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
    [Trait("category", "node-14-skipped")]
    [Trait("db", "postgres")]
    public class NodePostgreSqlIntegrationTests : DatabaseTestsBase, IClassFixture<Fixtures.PostgreSqlDbContainerFixture>
    {
        public NodePostgreSqlIntegrationTests(ITestOutputHelper output, Fixtures.PostgreSqlDbContainerFixture dbFixture)
            : base(output, dbFixture)
        {
        }

        [Fact(Skip = "Bug #1410367")]
        [Trait("build-image", "debian-stretch")]
        public async Task Node14App_PostgreSqlDB_WithLatestStretchBuildImageAsync()
        {
            await NodeApp_PostgreSqlDBAsync(ImageTestHelperConstants.LatestStretchTag);
        }

        [Fact(Skip = "Bug #1410367")]
        [Trait("build-image", "github-actions-debian-buster")]
        public async Task Node14App_PostgreSqlDB_WithGitHubActionsBusterBuildImageAsync()
        {
            await NodeApp_PostgreSqlDBAsync(ImageTestHelperConstants.GitHubActionsBuster);
        }

        private async Task NodeApp_PostgreSqlDBAsync(string buildImageTag)
        {
            await RunTestAsync(
                "nodejs",
                "14",
                ImageTestHelperConstants.OsTypeDebianBullseye,
                Path.Combine(HostSamplesDir, "nodejs", "node-postgres"),
                buildImageName: _imageHelper.GetBuildImage(buildImageTag));
        }
    }
}