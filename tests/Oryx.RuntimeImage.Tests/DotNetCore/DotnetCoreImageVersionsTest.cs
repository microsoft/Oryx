﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    [Trait("platform", "dotnet")]
    public class DotNetCoreImageVersionsTest : TestBase
    {
        public DotNetCoreImageVersionsTest(ITestOutputHelper output) : base(output)
        {
        }

        [SkippableTheory]
        [InlineData("3.1")]
        public void DotNetCoreBullseyeRuntimeImage_Contains_VersionAndCommit_Information(string version)
        {
            // we cant always rely on gitcommitid as env variable in case build context is not correctly passed
            // so we should check agent_os environment variable to know if the build is happening in azure devops agent
            // or locally, locally we need to skip this test
            var agentOS = Environment.GetEnvironmentVariable("AGENT_OS");
            Skip.If(string.IsNullOrEmpty(agentOS));

            // Arrange
            var buildNumber = Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");
            var expectedOryxVersion = string.Concat(Settings.OryxVersion, buildNumber);
            var gitCommitID = GitHelper.GetCommitID();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("dotnetcore", version),
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
        [InlineData("3.0")]
        [InlineData("5.0")]
        [InlineData("6.0")]
        public void DotNetCoreBusterRuntimeImage_Contains_VersionAndCommit_Information(string version)
        {
            // we cant always rely on gitcommitid as env variable in case build context is not correctly passed
            // so we should check agent_os environment variable to know if the build is happening in azure devops agent
            // or locally, locally we need to skip this test
            var agentOS = Environment.GetEnvironmentVariable("AGENT_OS");
            Skip.If(string.IsNullOrEmpty(agentOS));

            // Arrange
            var buildNumber = Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");
            var expectedOryxVersion = string.Concat(Settings.OryxVersion, buildNumber);
            var gitCommitID = GitHelper.GetCommitID();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("dotnetcore", version),
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
        [InlineData("1.0")]
        [InlineData("1.1")]
        [InlineData("2.0")]
        [InlineData("2.1")]
        [InlineData("2.2")]
        public void DotNetCoreStretchRuntimeImage_Contains_VersionAndCommit_Information(string version)
        {
            // we cant always rely on gitcommitid as env variable in case build context is not correctly passed
            // so we should check agent_os environment variable to know if the build is happening in azure devops agent
            // or locally, locally we need to skip this test
            var agentOS = Environment.GetEnvironmentVariable("AGENT_OS");
            Skip.If(string.IsNullOrEmpty(agentOS));

            // Arrange
            var buildNumber = Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");
            var expectedOryxVersion = string.Concat(Settings.OryxVersion, buildNumber);
            var gitCommitID = GitHelper.GetCommitID();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("dotnetcore", version),
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
        [InlineData("1.0", "Version  : 1.0.1")]
        [InlineData("1.1", "Version  : 1.1.13")]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void RuntimeImage_HasExecptedDotNetVersion_NetCoreApp10Versions(string version, string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("dotnetcore", version),
                CommandToExecuteOnRun = "dotnet",
                CommandArguments = new[] { "--version" }
            });

            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.Contains(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("2.0", "Version  : " + DotNetCoreRunTimeVersions.NetCoreApp20)]
        [InlineData("2.1", "Version: " + DotNetCoreRunTimeVersions.NetCoreApp21)]
        [InlineData("2.2", "Version: " + DotNetCoreRunTimeVersions.NetCoreApp22)]
        [InlineData("3.0", "Version: " + DotNetCoreRunTimeVersions.NetCoreApp30)]
        [InlineData("3.1", "Version: " + DotNetCoreRunTimeVersions.NetCoreApp31)]
        [InlineData("5.0", "Version: " + DotNetCoreRunTimeVersions.NetCoreApp50)]
        [InlineData("6.0", "Version: " + DotNetCoreRunTimeVersions.NetCoreApp60)]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void RuntimeImage_HasExecptedDotNetVersion(string version, string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("dotnetcore", version),
                CommandToExecuteOnRun = "dotnet",
                CommandArguments = new[] { "--info" }
            });

            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }
    }
}