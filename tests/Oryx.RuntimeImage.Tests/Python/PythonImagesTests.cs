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
    public class PythonImagesTest : TestBase
    {
        public PythonImagesTest(ITestOutputHelper output) : base(output)
        {
        }

        [SkippableTheory]
        [InlineData("2.7")]
        [InlineData("3.6")]
        [InlineData("3.7")]
        [InlineData("3.8")]
        [InlineData("3.9")]
        [InlineData("3.10")]
        public void PythonRuntimeImage_Contains_VersionAndCommit_Information(string version)
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
                ImageId = _imageHelper.GetRuntimeImage("python", version),
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
        [InlineData("2.7")]
        [InlineData("3.6")]
        [InlineData("3.7")]
        [InlineData("3.8")]
        [InlineData("3.9")]
        [InlineData("3.10")]
        public void JamSpell_CanBe_InstalledInTheRunTimeImage(string version)
        {
            // Arrange
            var expectedPackage = "jamspell";
            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("python", version),
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", $"wget -O - https://pypi.org/simple/ | grep -i {expectedPackage}" }
            });
            
            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedPackage, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("3.6", "Python " + PythonVersions.Python36Version)]
        [InlineData("3.7", "Python " + PythonVersions.Python37Version)]
        [InlineData("3.8", "Python " + PythonVersions.Python38Version)]
        [InlineData("3.9", "Python " + PythonVersions.Python39Version)]
        [InlineData("3.10", "Python " + PythonVersions.Python310Version)]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void PythonVersionMatchesImageName(string pythonVersion, string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("python", pythonVersion),
                CommandToExecuteOnRun = "python",
                CommandArguments = new[] { "--version" }
            });

            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Fact]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void Python2MatchesImageName()
        {
            string pythonVersion = "2.7";
            string expectedOutput = "Python " + PythonVersions.Python27Version;

            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("python", pythonVersion),
                CommandToExecuteOnRun = "python",
                CommandArguments = new[] { "--version" }
            });

            // Assert
            var actualOutput = result.StdErr.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    //bugs.python.org >> issue18338 weird but true, earlier than python 3.4
                    // sends python --version output to STDERR
                    Assert.Equal(expectedOutput, actualOutput);
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
                ImageId = _imageHelper.GetRuntimeImage("python", "3.7"),
                CommandToExecuteOnRun = "/bin/sh",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(() => Assert.Equal(result.ExitCode, exitCodeSentinel), result.GetDebugInfo());
        }
    }
}