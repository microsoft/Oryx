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

        [Theory]
        [Trait("category", "runtime-buster")]
        [InlineData("3.9")]
        public void PythonBusterRuntimeImage_Contains_VersionAndCommit_Information(string version)
        {
            // we cant always rely on gitcommitid as env variable in case build context is not correctly passed
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
                ImageId = _imageHelper.GetRuntimeImage("python", version, ImageTestHelperConstants.OsTypeDebianBuster),
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
        [InlineData("3.8")]
        [InlineData("3.9")]
        [InlineData("3.10")]
        [InlineData("3.11")]
        [InlineData("3.12")]
        public void PythonBullseyeRuntimeImage_Contains_VersionAndCommit_Information(string version)
        {
            // we cant always rely on gitcommitid as env variable in case build context is not correctly passed
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
                ImageId = _imageHelper.GetRuntimeImage("python", version, ImageTestHelperConstants.OsTypeDebianBullseye),
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
        [InlineData("3.11")]
        [InlineData("3.12")]
        public void PythonBookwormRuntimeImage_Contains_VersionAndCommit_Information(string version)
        {
            // we cant always rely on gitcommitid as env variable in case build context is not correctly passed
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
                ImageId = _imageHelper.GetRuntimeImage("python", version, ImageTestHelperConstants.OsTypeDebianBookworm),
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
        [Trait("category", "runtime-buster")]
        [InlineData("3.9")]
        public void JamSpell_CanBe_InstalledInBusterRunTimeImage(string version)
        {
            // Arrange
            var expectedPackage = "jamspell";
            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("python", version, ImageTestHelperConstants.OsTypeDebianBuster),
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
        [Trait("category", "runtime-bullseye")]
        [InlineData("3.8")]
        [InlineData("3.9")]
        [InlineData("3.10")]
        [InlineData("3.11")]
        [InlineData("3.12")]
        public void JamSpell_CanBe_InstalledInBullseyeRunTimeImage(string version)
        {
            // Arrange
            var expectedPackage = "jamspell";
            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("python", version, ImageTestHelperConstants.OsTypeDebianBullseye),
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
        [Trait("category", "runtime-bookworm")]
        [InlineData("3.11")]
        [InlineData("3.12")]
        public void JamSpell_CanBe_InstalledInBookwormRunTimeImage(string version)
        {
            // Arrange
            var expectedPackage = "jamspell";
            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("python", version, ImageTestHelperConstants.OsTypeDebianBookworm),
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
        [Trait("category", "runtime-bookworm")]
        [InlineData("3.11", "Python " + PythonVersions.Python311Version)]
        [InlineData("3.12", "Python " + PythonVersions.Python312Version)]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void PythonVersionMatchesBookwormImageName(string pythonVersion, string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("python", pythonVersion, ImageTestHelperConstants.OsTypeDebianBookworm),
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

        [Theory]
        [Trait("category", "runtime-bullseye")]
        [InlineData("3.8", "Python " + PythonVersions.Python38Version)]
        [InlineData("3.9", "Python " + PythonVersions.Python39Version)]
        [InlineData("3.10", "Python " + PythonVersions.Python310Version)]
        [InlineData("3.11", "Python " + PythonVersions.Python311Version)]
        [InlineData("3.12", "Python " + PythonVersions.Python312Version)]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void PythonVersionMatchesBullseyeImageName(string pythonVersion, string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("python", pythonVersion, ImageTestHelperConstants.OsTypeDebianBullseye),
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

        [Theory]
        [Trait("category", "runtime-buster")]
        [InlineData("3.9", "Python " + PythonVersions.Python39Version)]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void PythonVersionMatchesBusterImageName(string pythonVersion, string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("python", pythonVersion, ImageTestHelperConstants.OsTypeDebianBuster),
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
        [Trait("category", "runtime-bullseye")]
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
                ImageId = _imageHelper.GetRuntimeImage("python", "3.11", ImageTestHelperConstants.OsTypeDebianBullseye),
                CommandToExecuteOnRun = "/bin/sh",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(() => Assert.Equal(exitCodeSentinel, result.ExitCode), result.GetDebugInfo());
        }
    }
}