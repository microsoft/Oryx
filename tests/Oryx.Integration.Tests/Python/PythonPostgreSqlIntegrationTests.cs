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
    [Trait("db", "postgres")]
    public class PythonPostgreSqlIntegrationTests : DatabaseTestsBase, IClassFixture<Fixtures.PostgreSqlDbContainerFixture>
    {
        public PythonPostgreSqlIntegrationTests(ITestOutputHelper output, Fixtures.PostgreSqlDbContainerFixture dbFixture)
            : base(output, dbFixture)
        {
        }

        [Fact(Skip = "Bug #1410367")]
        [Trait("category", "python-3.7")]
        [Trait("build-image", "github-actions-debian-stretch")]
        public async Task Python37App_PostgreSqlDB_WithGitHubActionsStretchBuildImageAsync()
        {
            await PythonApp_PostgreSqlDBAsync("3.7", ImageTestHelperConstants.GitHubActionsStretch);
        }

        [Fact(Skip = "Bug #1410367")]
        [Trait("category", "python-3.7")]
        [Trait("build-image", "github-actions-debian-buster")]
        public async Task Python37App_PostgreSqlDB_WithGitHubActionsBusterBuildImageAsync()
        {
            await PythonApp_PostgreSqlDBAsync("3.7", ImageTestHelperConstants.GitHubActionsBuster);
        }

        [Fact(Skip = "Bug #1410367")]
        [Trait("category", "python-3.7")]
        [Trait("build-image", "debian-stretch")]
        public async Task Python37App_PostgreSqlDB_WithLatestStretchBuildImageAsync()
        {
            await PythonApp_PostgreSqlDBAsync("3.7", ImageTestHelperConstants.LatestStretchTag);
        }

        private async Task PythonApp_PostgreSqlDBAsync(string pythonVersion, string imageTag)
        {
            await RunTestAsync(
                "python",
                pythonVersion,
                Path.Combine(HostSamplesDir, "python", "postgres-sample"),
                buildImageName: _imageHelper.GetBuildImage(imageTag));
        }
    }
}