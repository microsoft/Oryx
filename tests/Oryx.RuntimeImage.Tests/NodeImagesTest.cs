// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using Oryx.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.RuntimeImage.Tests
{
    public class NodeImagesTest
    {
        private readonly ITestOutputHelper _output;
        private readonly DockerCli _dockerCli;

        public NodeImagesTest(ITestOutputHelper output)
        {
            _output = output;
            _dockerCli = new DockerCli();
        }

        [Theory]
        [InlineData("4.4.7")]
        [InlineData("4.5.0")]
        [InlineData("4.8.3")]
        [InlineData("4.8.4")]
        [InlineData("6.2.2")]
        [InlineData("6.6.0")]
        [InlineData("6.9.3")]
        [InlineData("6.10.3")]
        [InlineData("6.11.0")]
        [InlineData("6.11.1")]
        [InlineData("6.11.5")]
        [InlineData("8.0.0")]
        [InlineData("8.1.2")]
        [InlineData("8.1.3")]
        [InlineData("8.1.4")]
        [InlineData("8.2.1")]
        [InlineData("8.8.0")]
        [InlineData("8.8.1")]
        [InlineData("8.9.4")]
        [InlineData("8.11.2")]
        [InlineData("9.4.0")]
        [InlineData("10.1.0")]
        public void NodeVersionMatchesImageName(string nodeVersion)
        {
            // Arrange & Act
            var expectedNodeVersion = "v" + nodeVersion;
            var result = _dockerCli.Run(
                "oryxdevms/node-" + nodeVersion + ":latest",
                commandToExecuteOnRun: "node",
                commandArguments: new[] { "--version" });

            // Assert
            var actualOutput = result.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedNodeVersion, actualOutput);
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
