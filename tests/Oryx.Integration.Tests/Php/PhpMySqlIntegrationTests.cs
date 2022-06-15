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
        [Fact, Trait("category", "php-7.4")]
        public void PipelineTestInvocationsPhp74()
        {
            string phpVersion74 = "7.4";
            Task.WaitAll(
                PhpApp_UsingMysqli(phpVersion74, "latest"),
                PhpApp_UsingMysqli(phpVersion74, "github-actions"));
        }

        [Theory]
        [InlineData("7.4", "latest")]
        [InlineData("7.4", "github-actions")]
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