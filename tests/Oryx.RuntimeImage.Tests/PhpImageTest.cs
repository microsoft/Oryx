using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.RuntimeImage.Tests
{
    public class PhpImageTest
    {
        private readonly ITestOutputHelper _output;
        private readonly DockerCli _dockerCli;

        public PhpImageTest(ITestOutputHelper output)
        {
            _output = output;
            _dockerCli = new DockerCli();
        }

        [Theory]
        [InlineData("7.3", PhpVersions.Php73Version)]
        [InlineData("7.2", PhpVersions.Php72Version)]
        [InlineData("7.0", PhpVersions.Php70Version)]
        [InlineData("5.6", PhpVersions.Php56Version)]
        public void VersionMatchesImageName(string imageTag, string expectedPhpVersion)
        {
            // Arrange & Act
            var result = _dockerCli.Run("oryxdevms/php-" + imageTag + ":latest", "php", new[] { "--version" });

            // Assert
            RunAsserts(() =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("PHP " + expectedPhpVersion, result.Output);
                },
                result.GetDebugInfo());
        }

        private void RunAsserts(Action action, string message)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                _output.WriteLine(message);
                throw;
            }
        }
    }
}
