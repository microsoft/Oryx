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
    [Trait("category", "php")]
    public class PhpBuildpackTest : PhpEndToEndTestsBase
    {
        public PhpBuildpackTest(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Theory]
        // [InlineData(Constants.OryxBuildpackBuilderImageName)] // AB#896178
        [InlineData(Constants.HerokuBuildpackBuilderImageName)]
        // Twig does not support PHP < 7
        public async Task TwigExample_WithBuildpack(string builder)
        {
            // Arrange
            var appName = "twig-example";
            var appVolume = DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "php", appName));

            // Act & Assert
            await EndToEndTestHelper.RunPackAndAssertAppAsync(
                _output,
                appName,
                appVolume,
                "test-phpapp",
                builder,
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("<h1>Hello World!</h1>", data);
                });
        }
    }
}
