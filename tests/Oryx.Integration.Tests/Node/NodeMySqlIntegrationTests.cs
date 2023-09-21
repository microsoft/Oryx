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
    [Trait("db", "mysql")]
    public class NodeMySqlIntegrationTests : DatabaseTestsBase, IClassFixture<Fixtures.MySqlDbContainerFixture>
    {
        public NodeMySqlIntegrationTests(ITestOutputHelper output, Fixtures.MySqlDbContainerFixture dbFixture)
            : base(output, dbFixture)
        {
        }

        [Fact(Skip = "Bug #1505700 may be intermittent")]        
        [Trait("build-image", "debian-stretch")]
        public async Task Node14App_MySqlDB_WithLatestStretchBuildImageAsync()
        {
            await NodeApp_MySqlDBAsync(ImageTestHelperConstants.LatestStretchTag);
        }

        [Fact(Skip = "Bug #1505700 may be intermittent")]
        [Trait("build-image", "github-actions-debian-buster")]
        public async Task Node14App_MySqlDB_WithGitHubActionsBusterBuildImageAsync()
        {
            await NodeApp_MySqlDBAsync(ImageTestHelperConstants.GitHubActionsBuster);
        }

        private async Task NodeApp_MySqlDBAsync(string buildImageTag)
        {
            await RunTestAsync(
                "nodejs",
                "14",
                ImageTestHelperConstants.OsTypeDebianBullseye,
                Path.Combine(HostSamplesDir, "nodejs", "node-mysql"),
                buildImageName: _imageHelper.GetBuildImage(buildImageTag));
        }

    }
}