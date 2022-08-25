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

        // Unique category traits are needed to run each
        // platform-version in it's own pipeline agent. This is
        // because our agents currently a space limit of 10GB.
        [Fact, Trait("category", "php-74")]
        public async Task PipelineTestInvocationsPhp74Async()
        {
            string phpVersion74 = "7.4";
            await Task.WhenAll(
                PhpApp_UsingMysqliAsync(phpVersion74, ImageTestHelperConstants.LatestStretchTag),
                PhpApp_UsingMysqliAsync(phpVersion74, ImageTestHelperConstants.GitHubActionsStretch));
        }

        [Theory]
        [InlineData("7.4", ImageTestHelperConstants.LatestStretchTag)]
        [InlineData("7.4", ImageTestHelperConstants.GitHubActionsStretch)]
        public async Task PhpApp_UsingMysqliAsync(string phpVersion, string imageTag)
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