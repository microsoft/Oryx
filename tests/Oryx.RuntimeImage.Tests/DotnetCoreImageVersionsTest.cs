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
    public class DotnetCoreImageVersionsTest : TestBase
    {
        public DotnetCoreImageVersionsTest(ITestOutputHelper output) : base(output)
        {
        }

        [SkippableTheory]
        [InlineData("1.0")]
        [InlineData("1.1")]
        [InlineData("2.0")]
        [InlineData("2.1")]
        [InlineData("2.2")]
        public void DotnetCoreRuntimeImage_Contains_VersionAndCommit_Information(string version)
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
            var result = _dockerCli.Run(
                "oryxdevms/dotnetcore-" + version + ":latest",
                commandToExecuteOnRun: "oryx",
                commandArguments: new[] { "--version" });
            // Assert
            RunAsserts(
                () =>
                {
                    Assert.False(result.IsSuccess);
                    Assert.NotNull(result.StdErr);
                    Assert.DoesNotContain(".unspecified, Commit: unspecified", result.StdErr);
                    Assert.Contains(gitCommitID, result.StdErr);
                    Assert.Contains(expectedOryxVersion, result.StdErr);
                
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("1.0", "Version  : 1.0.1")]
        [InlineData("1.1", "Version  : 1.1.12")]
        public void DotnetVersionMatchesImageName_NetCoreApp1Versions(string version, string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(
                "oryxdevms/dotnetcore-" + version + ":latest",
                commandToExecuteOnRun: "dotnet",
                commandArguments: new[] { "--version" });

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
        [InlineData("2.1", "Version: 2.1.9")]
        [InlineData("2.2", "Version: 2.2.3")]
        public void DotnetVersionMatchesImageName(string version, string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(
                "oryxdevms/dotnetcore-" + version + ":latest",
                commandToExecuteOnRun: "dotnet",
                commandArguments: new[] { "--info" });

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