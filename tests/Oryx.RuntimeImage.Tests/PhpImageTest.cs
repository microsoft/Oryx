// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    public class PhpImageTest : TestBase
    {
        public PhpImageTest(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData("7.3", PhpVersions.Php73Version)]
        [InlineData("7.2", PhpVersions.Php72Version)]
        [InlineData("7.0", PhpVersions.Php70Version)]
        [InlineData("5.6", PhpVersions.Php56Version)]
        public void VersionMatchesImageName(string imageTag, string expectedPhpVersion)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = $"oryxdevms/php-{imageTag}:latest",
                CommandToExecuteOnRun = "php",
                CommandArguments = new[] { "--version" }
            });

            // Assert
            RunAsserts(() =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("PHP " + expectedPhpVersion, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("7.3")]
        [InlineData("7.2")]
        [InlineData("7.0")]
        [InlineData("5.6")]
        public void GraphicsExtension_Gd_IsInstalled(string imageTag)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = $"oryxdevms/php-{imageTag}:latest",
                CommandToExecuteOnRun = "php",
                CommandArguments = new[] { "-r", "echo json_encode(gd_info());" }
            });

            // Assert
            JObject gdInfo = JsonConvert.DeserializeObject<JObject>(result.StdOut);
            Assert.True((bool)((JValue)gdInfo.GetValue("GIF Read Support")).Value);
            Assert.True((bool)((JValue)gdInfo.GetValue("GIF Create Support")).Value);
            Assert.True((bool)((JValue)gdInfo.GetValue("JPEG Support")).Value);
            Assert.True((bool)((JValue)gdInfo.GetValue("PNG Support")).Value);
        }
    }
}
