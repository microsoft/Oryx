// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    public class NodeRuntimeImageContainsVersionAndCommitInfo : NodeRuntimeImageTestBase
    {
        public NodeRuntimeImageContainsVersionAndCommitInfo(
            ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        // [SkippableTheory]
        // [Trait("category", "runtime-buster")]
        // [MemberData(nameof(TestValueGenerator.GetBusterNodeVersions), MemberType = typeof(TestValueGenerator))]
        // public void NodeBusterImage_Contains_VersionAndCommit_Information(string version, string osType)
        // {
        //     // We can't always rely on git commit ID as env variable in case build context is not correctly passed
        //     // so we should check agent_os environment variable to know if the build is happening in azure devops agent
        //     // or locally, locally we need to skip this test
        //     var agentOS = Environment.GetEnvironmentVariable("AGENT_OS");
        //     Skip.If(string.IsNullOrEmpty(agentOS));

        //     // Arrange
        //     var gitCommitID = GitHelper.GetCommitID();
        //     var buildNumber = Environment.GetEnvironmentVariable("IMAGE_BUILDNUMBER");
        //     var expectedOryxVersion = string.Concat(Settings.OryxVersion, buildNumber);

        //     // Act
        //     var result = _dockerCli.Run(new DockerRunArguments
        //     {
        //         ImageId = _imageHelper.GetRuntimeImage("node", version, osType),
        //         CommandToExecuteOnRun = "oryx",
        //         CommandArguments = new[] { "version" }
        //     });

        //     // Assert
        //     RunAsserts(
        //         () =>
        //         {
        //             Assert.True(result.IsSuccess);
        //             Assert.NotNull(result.StdErr);
        //             Assert.DoesNotContain(".unspecified, Commit: unspecified", result.StdOut);
        //             Assert.Contains(gitCommitID, result.StdOut);
        //             Assert.Contains(expectedOryxVersion, result.StdOut);
        //         },
        //         result.GetDebugInfo());
        // }

        [SkippableTheory]
        [Trait("category", "runtime-bullseye")]
        [MemberData(nameof(TestValueGenerator.GetBullseyeNodeVersions), MemberType = typeof(TestValueGenerator))]
        public void NodeBullseyeImage_Contains_VersionAndCommit_Information(string version, string osType)
        {
            // We can't always rely on git commit ID as env variable in case build context is not correctly passed
            // so we should check agent_os environment variable to know if the build is happening in azure devops agent
            // or locally, locally we need to skip this test
            var agentOS = Environment.GetEnvironmentVariable("AGENT_OS");
            Skip.If(string.IsNullOrEmpty(agentOS));

            // Arrange
            var gitCommitID = GitHelper.GetCommitID();
            var buildNumber = Environment.GetEnvironmentVariable("IMAGE_BUILDNUMBER");
            var expectedOryxVersion = string.Concat(Settings.OryxVersion, buildNumber);

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", version, osType),
                CommandToExecuteOnRun = "oryx",
                CommandArguments = new[] { "version" }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.NotNull(result.StdErr);
                    Assert.DoesNotContain(".unspecified, Commit: unspecified", result.StdOut);
                    Assert.Contains(gitCommitID, result.StdOut);
                    Assert.Contains(expectedOryxVersion, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [SkippableTheory]
        [Trait("category", "runtime-bookworm")]
        [MemberData(nameof(TestValueGenerator.GetBookwormNodeVersions), MemberType = typeof(TestValueGenerator))]
        public void NodeBookwormImage_Contains_VersionAndCommit_Information(string version, string osType)
        {
            // We can't always rely on git commit ID as env variable in case build context is not correctly passed
            // so we should check agent_os environment variable to know if the build is happening in azure devops agent
            // or locally, locally we need to skip this test
            var agentOS = Environment.GetEnvironmentVariable("AGENT_OS");
            Skip.If(string.IsNullOrEmpty(agentOS));

            // Arrange
            var gitCommitID = GitHelper.GetCommitID();
            var buildNumber = Environment.GetEnvironmentVariable("IMAGE_BUILDNUMBER");
            var expectedOryxVersion = string.Concat(Settings.OryxVersion, buildNumber);

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", version, osType),
                CommandToExecuteOnRun = "oryx",
                CommandArguments = new[] { "version" }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.NotNull(result.StdErr);
                    Assert.DoesNotContain(".unspecified, Commit: unspecified", result.StdOut);
                    Assert.Contains(gitCommitID, result.StdOut);
                    Assert.Contains(expectedOryxVersion, result.StdOut);
                },
                result.GetDebugInfo());
        }

    }
}
