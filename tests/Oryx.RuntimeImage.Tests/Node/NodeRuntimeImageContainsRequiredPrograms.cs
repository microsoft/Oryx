// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    public class NodeRuntimeImageContainsRequiredPrograms : NodeRuntimeImageTestBase
    {
        public NodeRuntimeImageContainsRequiredPrograms(
            ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Theory]
        [MemberData(nameof(TestValueGenerator.GetNodeVersions), MemberType = typeof(TestValueGenerator))]
        public void NodeImage_Contains_RequiredPrograms(string nodeTag)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", nodeTag),
                CommandToExecuteOnRun = "/bin/sh",
                CommandArguments = new[]
                {
                    "-c",
                    "which tar && which unzip && which pm2 && /opt/node-wrapper/node --version"
                }
            });

            // Assert
            RunAsserts(() => Assert.True(result.IsSuccess), result.GetDebugInfo());
        }

        [Theory]
        [MemberData(nameof(TestValueGenerator.GetNodeVersions), MemberType = typeof(TestValueGenerator))]
        public void NodeImage_Contains_ApplicationInsights(string nodeTag)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", nodeTag),
                CommandToExecuteOnRun = "/bin/sh",
                CommandArguments = new[]
                {
                    "-c",
                    "npm list -g applicationinsights"
                }
            });

            var actualOutput = result.StdOut.ReplaceNewLine();

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("applicationinsights@1.4.1", actualOutput);
                    Assert.Contains("/usr/local/lib", actualOutput);
                },
                result.GetDebugInfo());
        }
    }
}
