// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    public class DotNetCoreImageVersionsTest : TestBase
    {
        public DotNetCoreImageVersionsTest(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [Trait("category", "runtime-bullseye")]
        [InlineData("8.0")]
        public void DotNetCoreBullseyeRuntimeImage_Contains_VersionAndCommit_Information(string version)
        {
            // we cant always rely on gitcommitid as env variable in case build context is not correctly passed
            // so we should check agent_os environment variable to know if the build is happening in azure devops agent
            // or locally, locally we need to skip this test
            var agentOS = Environment.GetEnvironmentVariable("AGENT_OS");
            Skip.If(string.IsNullOrEmpty(agentOS));

            // Arrange
            var buildNumber = Environment.GetEnvironmentVariable("IMAGE_BUILDNUMBER");
            var expectedOryxVersion = string.Concat(Settings.OryxVersion, buildNumber);
            var gitCommitID = GitHelper.GetCommitID();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("dotnetcore", version, ImageTestHelperConstants.OsTypeDebianBullseye),
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

        [Theory]
        [Trait("category", "runtime-bookworm")]
        [InlineData("8.0")]
        [InlineData("9.0")]
        [InlineData("10.0")]
        public void DotNetCoreBookwormRuntimeImage_Contains_VersionAndCommit_Information(string version)
        {
            // we cant always rely on gitcommitid as env variable in case build context is not correctly passed
            // so we should check agent_os environment variable to know if the build is happening in azure devops agent
            // or locally, locally we need to skip this test
            var agentOS = Environment.GetEnvironmentVariable("AGENT_OS");
            Skip.If(string.IsNullOrEmpty(agentOS));

            // Arrange
            var buildNumber = Environment.GetEnvironmentVariable("IMAGE_BUILDNUMBER");
            var expectedOryxVersion = string.Concat(Settings.OryxVersion, buildNumber);
            var gitCommitID = GitHelper.GetCommitID();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("dotnetcore", version, ImageTestHelperConstants.OsTypeDebianBookworm),
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

        [Theory]
        [Trait("category", "runtime-bullseye")]
        [InlineData("8.0", "Version: " + DotNetCoreRunTimeVersions.NetCoreApp80)]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void RuntimeImage_Bullseye_HasExecptedDotNetVersion(string version, string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("dotnetcore", version, ImageTestHelperConstants.OsTypeDebianBullseye),
                CommandToExecuteOnRun = "dotnet",
                CommandArguments = new[] { "--info" }
            });

            // Assert
            var actualOutput = string.Join("", result.StdOut.ReplaceNewLine().Where(c => !char.IsWhiteSpace(c)));
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(string.Join("", expectedOutput.Where(c => !char.IsWhiteSpace(c))), actualOutput);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [Trait("category", "runtime-bookworm")]
        [InlineData("8.0", "Version: " + DotNetCoreRunTimeVersions.NetCoreApp80)]
        [InlineData("9.0", "Version: " + DotNetCoreRunTimeVersions.NetCoreApp90)]
        [InlineData("10.0", "Version: " + DotNetCoreRunTimeVersions.NetCoreApp100)]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void RuntimeImage_Bookworm_HasExecptedDotNetVersion(string version, string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("dotnetcore", version, ImageTestHelperConstants.OsTypeDebianBookworm),
                CommandToExecuteOnRun = "dotnet",
                CommandArguments = new[] { "--info" }
            });

            // Assert
            var actualOutput = string.Join("", result.StdOut.ReplaceNewLine().Where(c => !char.IsWhiteSpace(c)));
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(string.Join("", expectedOutput.Where(c => !char.IsWhiteSpace(c))), actualOutput);
                },
                result.GetDebugInfo());
        }
    }
}