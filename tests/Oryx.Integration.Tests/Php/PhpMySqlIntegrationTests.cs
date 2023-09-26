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
    [Trait("db", "mysql")]
    public class PhpMySqlIntegrationTests : DatabaseTestsBase, IClassFixture<Fixtures.MySqlDbContainerFixture>
    {
        public PhpMySqlIntegrationTests(ITestOutputHelper output, Fixtures.MySqlDbContainerFixture dbFixture)
            : base(output, dbFixture)
        {
        }

        [Fact]
        [Trait("category", "php-7.4")]
        [Trait("build-image", "debian-stretch")]
        public async Task Php74App_UsingMysqli_WithLatestStretchBuildImageAsync()
        {
            await PhpApp_UsingMysqliAsync("7.4", ImageTestHelperConstants.OsTypeDebianBullseye, ImageTestHelperConstants.LatestStretchTag);
        }

        [Fact]
        [Trait("category", "php-7.4")]
        [Trait("build-image", "github-actions-debian-buster")]
        public async Task Php74App_UsingMysqli_WithGitHubActionsBusterBuildImageAsync()
        {
            await PhpApp_UsingMysqliAsync("7.4", ImageTestHelperConstants.OsTypeDebianBullseye, ImageTestHelperConstants.GitHubActionsBuster);
        }

        private async Task PhpApp_UsingMysqliAsync(string phpVersion, string osType, string buildImageTag)
        {
            await RunTestAsync(
                "php",
                phpVersion,
                osType,
                Path.Combine(HostSamplesDir, "php", "mysqli-example"),
                8080,
                specifyBindPortFlag: false,
                buildImageName: _imageHelper.GetBuildImage(buildImageTag));
        }
    }
}