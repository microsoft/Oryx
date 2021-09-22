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
    [Trait("db", "mysql")]
    public class PhpMySqlIntegrationTests : DatabaseTestsBase, IClassFixture<Fixtures.MySqlDbContainerFixture>
    {
        public PhpMySqlIntegrationTests(ITestOutputHelper output, Fixtures.MySqlDbContainerFixture dbFixture)
            : base(output, dbFixture)
        {
        }

        [Theory]
        [InlineData("7.4", "latest")]
        [InlineData("7.4", "github-actions")]
        [InlineData("7.3", "latest")]
        [InlineData("7.3", "github-actions")]
        [InlineData("7.3", "github-actions-buster")]
        [InlineData("7.2", "latest")]
        [InlineData("7.2", "github-actions")]
        public async Task PhpApp_UsingMysqli(string phpVersion, string imageTag)
        {
            await RunTestAsync(
                "php",
                phpVersion,
                Path.Combine(HostSamplesDir, "php", "mysqli-example"),
                8080,
                specifyBindPortFlag: false,
                buildImageName: _imageHelper.GetBuildImage(imageTag));
        }
    }
}