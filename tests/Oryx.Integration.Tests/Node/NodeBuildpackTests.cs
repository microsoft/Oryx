// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{

    [Trait("category", "node")]
    public class NodeBuildpackTests : NodeEndToEndTestsBase
    {
        public NodeBuildpackTests(ITestOutputHelper output, TestTempDirTestFixture fixture) : base(output, fixture)
        {
        }

        [Theory]
        [InlineData(Constants.OryxBuildpackBuilderImageName)]
        [InlineData(Constants.HerokuBuildpackBuilderImageName)]
        public async Task CanBuildAndRun_NodeApp_WithBuildpack(string builder)
        {
            var appName = "webfrontend";

            await EndToEndTestHelper.RunPackAndAssertAppAsync(
                _output,
                appName,
                CreateAppVolume(appName),
                "test-nodeapp",
                builder,
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Say It Again", data);
                });
        }
    }
}