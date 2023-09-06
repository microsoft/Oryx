// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    public class RubyImagesTest : TestBase
    {
        public RubyImagesTest(ITestOutputHelper output) : base(output)
        {
        }

        [SkippableFact]
        public void RubyRuntimeImage_Contains_VersionAndCommit_Information()
        {
            // we cant always rely on gitcommitid as env variable in case build context is not correctly passed
            // so we should check agent_os environment variable to know if the build is happening in azure devops agent
            // or locally, locally we need to skip this test
            var agentOS = Environment.GetEnvironmentVariable("AGENT_OS");
            Skip.If(string.IsNullOrEmpty(agentOS));

            // Arrange
            var gitCommitID = GitHelper.GetCommitID();
            var buildNumber = Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");
            var expectedOryxVersion = string.Concat(Settings.OryxVersion, buildNumber);

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("ruby", "dynamic", ImageTestHelperConstants.OsTypeDebianBuster),
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

        [Fact]
        public void GeneratedScript_CanRunStartupScriptsFromAppRoot()
        {
            // Arrange
            const int exitCodeSentinel = 222;
            var appPath = "/tmp/app";
            var script = new ShellScriptBuilder()
                .CreateDirectory(appPath)
                .CreateFile(appPath + "/entry.sh", $"exit {exitCodeSentinel}")
                .AddCommand("oryx create-script -userStartupCommand entry.sh -appPath " + appPath)
                .AddCommand(". ./run.sh") // Source the default output path
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("ruby", "dynamic", ImageTestHelperConstants.OsTypeDebianBuster),
                CommandToExecuteOnRun = "/bin/sh",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(() => Assert.Equal(exitCodeSentinel, result.ExitCode), result.GetDebugInfo());
        }

        [Theory]
        [InlineData("2.5", ImageTestHelperConstants.OsTypeDebianBuster, "ruby " + RubyVersions.Ruby25Version)]
        [InlineData("2.5", ImageTestHelperConstants.OsTypeDebianBullseye, "ruby " + RubyVersions.Ruby25Version)]
        [InlineData("2.6", ImageTestHelperConstants.OsTypeDebianBuster, "ruby " + RubyVersions.Ruby26Version)]
        [InlineData("2.6", ImageTestHelperConstants.OsTypeDebianBullseye, "ruby " + RubyVersions.Ruby26Version)]
        [InlineData("2.7", ImageTestHelperConstants.OsTypeDebianBuster, "ruby " + RubyVersions.Ruby27Version)]
        [InlineData("2.7", ImageTestHelperConstants.OsTypeDebianBullseye, "ruby " + RubyVersions.Ruby27Version)]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void RubyVersionMatchesImageName(string rubyVersion, string osType, string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("ruby", rubyVersion, osType),
                CommandToExecuteOnRun = "ruby",
                CommandArguments = new[] { "--version" }
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