// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class VersionInformationTest
    {
        private const string Python27VersionInfo = "Python " + Settings.Python27Version;
        private const string Python36VersionInfo = "Python " + Common.PythonVersions.Python36Version;
        private const string Python37VersionInfo = "Python " + Common.PythonVersions.Python37Version;

        private readonly ITestOutputHelper _output;
        private readonly DockerCli _dockerCli;

        public VersionInformationTest(ITestOutputHelper output)
        {
            _output = output;
            _dockerCli = new DockerCli();
        }

        [SkippableFact]
        public void OryxBuildImage_Contains_VersionAndCommit_Information()
        {
            // we cant always rely on gitcommitid as env variable in case build context is not correctly passed
            // so we should check agent_os environment variable to know if the test is happening in azure devops agent 
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
                ImageId = Settings.BuildImageName,
                CommandToExecuteOnRun = "oryx",
                CommandArguments = new[] { "--version" }
            });

            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.DoesNotContain(".unspecified, Commit: unspecified", actualOutput);
                    Assert.Contains(gitCommitID, actualOutput);
                    Assert.Contains(expectedOryxVersion, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Trait("platform", "dotnet")]
        [Fact]
        public void DotNetAlias_UsesLtsVersion_ByDefault()
        {
            // Arrange
            var expectedOutput = DotNetCoreSdkVersions.DotNetCore21SdkVersion;

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                CommandToExecuteOnRun = "dotnet",
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

        [Trait("platform", "dotnet")]
        [Theory]
        [InlineData("1", DotNetCoreSdkVersions.DotNetCore11SdkVersion)]
        [InlineData("1.0", DotNetCoreSdkVersions.DotNetCore11SdkVersion)]
        [InlineData("1.1", DotNetCoreSdkVersions.DotNetCore11SdkVersion)]
        [InlineData("2", DotNetCoreSdkVersions.DotNetCore21SdkVersion)]
        [InlineData("2.0", DotNetCoreSdkVersions.DotNetCore21SdkVersion)]
        [InlineData("2.1", DotNetCoreSdkVersions.DotNetCore21SdkVersion)]
        [InlineData("lts", DotNetCoreSdkVersions.DotNetCore21SdkVersion)]
        [InlineData("2.2", DotNetCoreSdkVersions.DotNetCore22SdkVersion)]
        [InlineData("3", DotNetCoreSdkVersions.DotNetCore30SdkVersionPreviewName)]
        [InlineData("3.0", DotNetCoreSdkVersions.DotNetCore30SdkVersionPreviewName)]
        public void DotNetAlias_UsesVersion_SetOnBenv(string runtimeVersion, string expectedSdkVersion)
        {
            // Arrange
            var script = new ShellScriptBuilder()
                .Source($"benv dotnet={runtimeVersion}")
                .AddCommand("dotnet --version")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedSdkVersion, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Trait("platform", "node")]
        [Fact]
        public void Node_UsesLTSVersion_ByDefault_WhenNoExplicitVersionIsProvided()
        {
            // Arrange
            var expectedOutput = "v" + NodeVersions.Node10Version;

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                CommandToExecuteOnRun = "node",
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

        [Trait("platform", "python")]
        [Fact]
        public void PythonAlias_UsesPython2_ByDefault_WhenNoExplicitVersionIsProvided()
        {
            // Arrange
            var expectedOutput = Python27VersionInfo;

            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                CommandToExecuteOnRun = "python",
                CommandArguments = new[] { "--version" }
            });

            // Assert
            var actualOutput = result.StdErr.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Trait("platform", "python")]
        [Fact]
        public void Python3Alias_UsesPythonLatestVersion_ByDefault_WhenNoExplicitVersionIsProvided()
        {
            // Arrange
            var expectedOutput = $"Python {Common.PythonVersions.Python37Version}";

            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                CommandToExecuteOnRun = "python3",
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

        [Trait("platform", "node")]
        [Theory]
        [InlineData("4", "v4.8.0")]
        [InlineData("4.5", "v4.5.0")]
        [InlineData("4.8", "v4.8.0")]
        [InlineData("4.5.0", "v4.5.0")]
        [InlineData("4.8.0", "v4.8.0")]
        [InlineData("6.11", "v6.11.0")]
        [InlineData("6.11.0", "v6.11.0")]
        [InlineData("8.1.4", "v8.1.4")]
        [InlineData("8.11", "v8.11.2")]
        [InlineData("8.11.2", "v8.11.2")]
        [InlineData("8.12.0", "v8.12.0")]
        [InlineData("8.15", "v8.15.1")]
        [InlineData("9", "v9.4.0")]
        [InlineData("9.4", "v9.4.0")]
        [InlineData("9.4.0", "v9.4.0")]
        [InlineData("10.1", "v10.1.0")]
        [InlineData("10.1.0", "v10.1.0")]
        [InlineData("10.10.0", "v10.10.0")]
        [InlineData("10.14.2", "v10.14.2")]
        [InlineData("6", "v" + NodeVersions.Node6Version)]
        [InlineData("8", "v" + NodeVersions.Node8Version)]
        [InlineData("10", "v" + NodeVersions.Node10Version)]
        [InlineData(NodeVersions.Node6MajorMinorVersion, "v" + NodeVersions.Node6Version)]
        [InlineData(NodeVersions.Node8MajorMinorVersion, "v" + NodeVersions.Node8Version)]
        [InlineData(NodeVersions.Node10MajorMinorVersion, "v" + NodeVersions.Node10Version)]
        [InlineData("lts", "v" + NodeVersions.Node10Version)]
        public void NodeAlias_UsesVersion_SetOnBenv(string specifiedVersion, string expectedOutput)
        {
            // Arrange
            var script = new ShellScriptBuilder()
                .Source($"benv node={specifiedVersion}")
                .AddCommand("node --version")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
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

        [Trait("platform", "node")]
        [Theory]
        // Only version 6 of npm is upgraded, so the following should remain unchanged.
        [InlineData("10.1", "5.6.0")]
        // Make sure the we get the upgraded version of npm in the following cases
        [InlineData("10.10.0", "6.9.0")]
        [InlineData("10.14.2", "6.9.0")]
        [InlineData(NodeVersions.Node10MajorMinorVersion, "6.9.0")]
        public void UsesExpectedNpmVersion(string nodeVersion, string expectedOutput)
        {
            // Arrange
            var script = new ShellScriptBuilder()
                .Source($"benv node={nodeVersion}")
                .AddCommand("npm --version")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
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

        [Trait("platform", "node")]
        [Theory]
        [InlineData("latest", "6.9.0")]
        [InlineData("6", "6.9.0")]
        [InlineData("6.9", "6.9.0")]
        [InlineData("5", "5.6.0")]
        [InlineData("5.6", "5.6.0")]
        [InlineData("5.4", "5.4.2")]
        [InlineData("5.3", "5.3.0")]
        [InlineData("5.0", "5.0.3")]
        [InlineData("3", "3.10.10")]
        [InlineData("3.10", "3.10.10")]
        [InlineData("3.9", "3.9.5")]
        [InlineData("2", "2.15.9")]
        [InlineData("2.15", "2.15.9")]
        public void Npm_UsesVersion_SpecifiedToBenv(string specifiedVersion, string expectedOutput)
        {
            // Arrange
            var script = new ShellScriptBuilder()
                .Source($"benv npm={specifiedVersion}")
                .AddCommand("npm --version")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
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

        [Trait("platform", "python")]
        [Theory]
        [InlineData("2", Python27VersionInfo)]
        [InlineData("2.7", Python27VersionInfo)]
        [InlineData(Settings.Python27Version, Python27VersionInfo)]
        public void PythonAlias_UsesVersion_SetOnBenv(string specifiedVersion, string expectedOutput)
        {
            // Arrange
            var script = new ShellScriptBuilder()
                .Source($"benv python={specifiedVersion}")
                .AddCommand("python --version")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            // NOTE: Python2 version writes out information to StdErr unlike Python3 versions
            var actualOutput = result.StdErr.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Trait("platform", "python")]
        [Theory]
        [InlineData("2", Python27VersionInfo)]
        [InlineData("2.7", Python27VersionInfo)]
        [InlineData(Settings.Python27Version, Python27VersionInfo)]
        public void Python2Alias_UsesVersion_SetOnBenv(string specifiedVersion, string expectedOutput)
        {
            // Arrange
            var script = new ShellScriptBuilder()
                .Source($"benv python={specifiedVersion}")
                .AddCommand("python2 --version")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            var actualOutput = result.StdErr.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Trait("platform", "python")]
        [Theory]
        [InlineData("latest", Python37VersionInfo)]
        [InlineData("3", Python37VersionInfo)]
        [InlineData("3.6", Python36VersionInfo)]
        [InlineData(Common.PythonVersions.Python36Version, Python36VersionInfo)]
        [InlineData("3.7", Python37VersionInfo)]
        [InlineData(Common.PythonVersions.Python37Version, Python37VersionInfo)]
        public void Python3_UsesVersion_SetOnBenv(string specifiedVersion, string expectedOutput)
        {
            // Arrange
            var script = new ShellScriptBuilder()
                .Source($"benv python={specifiedVersion}")
                .AddCommand("python --version")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
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