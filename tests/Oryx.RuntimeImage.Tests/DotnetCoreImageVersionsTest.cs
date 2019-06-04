// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
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
        [InlineData("1.0")]
        [InlineData("1.1")]
        [InlineData("2.0")]
        [InlineData("2.1")]
        [InlineData("2.2")]
        [InlineData("3.0")]
        public void DotNetCoreRuntimeImage_Contains_VersionAndCommit_Information(string version)
        {
            var agentOS = Environment.GetEnvironmentVariable("AGENT_OS");
            var gitCommitID = Environment.GetEnvironmentVariable("BUILD_SOURCEVERSION");
            var buildNumber = Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");
            var expectedOryxVersion = string.Concat(Settings.OryxVersion, buildNumber);

            // we cant always rely on gitcommitid as env variable in case build context is not correctly passed
            // so we should check agent_os environment variable to know if the build is happening in azure devops agent 
            // or locally, locally we need to skip this test
            Skip.If(string.IsNullOrEmpty(agentOS));

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = $"oryxdevms/dotnetcore-{version}:latest",
                CommandToExecuteOnRun = "oryx",
                CommandArguments = new[] { " " }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.NotNull(result.StdErr);
                    Assert.DoesNotContain(".unspecified, Commit: unspecified", result.StdErr);
                    Assert.Contains(gitCommitID, result.StdErr);
                    Assert.Contains(expectedOryxVersion, result.StdErr);

                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("1.0", "Version  : 1.0.1")]
        [InlineData("1.1", "Version  : 1.1.13")]
        public void RuntimeImage_HasExecptedDotNetVersion_NetCoreApp10Versions(string version, string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = $"oryxdevms/dotnetcore-{version}:latest",
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
        [InlineData("2.0", "Version  : 2.0.9")]
        [InlineData("2.1", "Version: 2.1.11")]
        [InlineData("2.2", "Version: 2.2.5")]
        [InlineData("3.0", "Version: 3.0.0-preview5-27626-15")]
        public void RuntimeImage_HasExecptedDotNetVersion(string version, string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = $"oryxdevms/dotnetcore-{version}:latest",
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