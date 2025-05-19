// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class VersionInformationTest
    {
        private const string Python27VersionInfo = "Python " + PythonVersions.Python27Version;
        private const string Python36VersionInfo = "Python " + PythonVersions.Python36Version;
        private const string Python37VersionInfo = "Python " + PythonVersions.Python37Version;
        private const string Python38VersionInfo = "Python " + PythonVersions.Python38Version;

        private readonly ITestOutputHelper _output;
        private readonly DockerCli _dockerCli;

        public VersionInformationTest(ITestOutputHelper output)
        {
            _output = output;
            _dockerCli = new DockerCli();
        }

        [SkippableFact, Trait("category", "latest")]
        public void PipelineTestInvocationLatest()
        {
            var imageTestHelper = new ImageTestHelper();
            PhpAlias_UsesPhpLatestVersion_ByDefault_WhenNoExplicitVersionIsProvided(
                imageTestHelper.GetBuildImage());
            Python3Alias_UsesPythonLatestVersion_ByDefault_WhenNoExplicitVersionIsProvided(
                Settings.BuildImageName);
            Node_UsesLTSVersion_ByDefault_WhenNoExplicitVersionIsProvided(
                Settings.BuildImageName);
            DotNetAlias_UsesLtsVersion_ByDefault(
                Settings.BuildImageName);
            OryxBuildImage_Contains_VersionAndCommit_Information(Settings.BuildImageName);
        }

        [SkippableFact, Trait("category", "jamstack")]
        public void PipelineTestInvocationJamstack()
        {
            var imageTestHelper = new ImageTestHelper();
            OryxBuildImage_Contains_VersionAndCommit_Information(
                imageTestHelper.GetAzureFunctionsJamStackBuildImage());
        }

        [SkippableFact, Trait("category", "githubactions")]
        public void PipelineTestInvocationGithubActions()
        {
            var imageTestHelper = new ImageTestHelper();
            OryxBuildImage_Contains_VersionAndCommit_Information(
                imageTestHelper.GetGitHubActionsBuildImage());
        }

        private void OryxBuildImage_Contains_VersionAndCommit_Information(string buildImageName)
        {
            // Please note:
            // This test method has at least 1 wrapper function that pases the imageName parameter.

            // we cant always rely on gitcommitid as env variable in case build context is not correctly passed
            // so we should check agent_os environment variable to know if the test is happening in azure devops agent 
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
                ImageId = buildImageName,
                CommandToExecuteOnRun = "oryx",
                CommandArguments = new[] { "--info" }
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

        [Theory]
        [InlineData(Settings.BuildImageName)]
        [InlineData(Settings.LtsVersionsBuildImageName)]
        public void DotNetAlias_UsesLtsVersion_ByDefault(string buildImageName)
        {
            // Arrange
            var expectedOutput = FinalStretchVersions.FinalStretchDotNetCore31SdkVersion;

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = buildImageName,
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

        [Theory, Trait("category", "latest")]
        [InlineData(DotNetCoreSdkVersions.DotNetCore22SdkVersion)]
        [InlineData(DotNetCoreSdkVersions.DotNetCore30SdkVersion)]
        [InlineData(FinalStretchVersions.FinalStretchDotNetCore31SdkVersion)]
        public void DotNetAlias_UsesVersion_SetOnBenv(string expectedSdkVersion)
        {
            // Arrange
            var script = new ShellScriptBuilder()
                .Source($"benv dotnet={expectedSdkVersion}")
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

        [Theory]
        [InlineData(Settings.BuildImageName)]
        [InlineData(Settings.LtsVersionsBuildImageName)]
        public void Node_UsesLTSVersion_ByDefault_WhenNoExplicitVersionIsProvided(string buildImageName)
        {
            // Arrange
            var expectedOutput = "v" + (buildImageName.Contains("stretch") 
                ? FinalStretchVersions.FinalStretchNode14Version 
                : NodeConstants.NodeLtsVersion);

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = buildImageName,
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

        [Theory]
        [InlineData(Settings.BuildImageName)]
        [InlineData(Settings.LtsVersionsBuildImageName)]
        public void Python3Alias_UsesPythonLatestVersion_ByDefault_WhenNoExplicitVersionIsProvided(
            string buildImageName)
        {
            // Arrange
            var expectedOutput = $"Python {PythonVersions.Python38Version}";

            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = buildImageName,
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
        [Theory, Trait("category", "latest")]
        [InlineData("8.1.4", "v8.1.4")]
        [InlineData("8.11", "v8.11.4")]
        [InlineData("8.11.4", "v8.11.4")]
        [InlineData("8.12.0", "v8.12.0")]
        [InlineData("8.15", "v8.15.1")]
        [InlineData("9", "v9.4.0")]
        [InlineData("9.4", "v9.4.0")]
        [InlineData("9.4.0", "v9.4.0")]
        [InlineData("10.1", "v10.1.0")]
        [InlineData("10.1.0", "v10.1.0")]
        [InlineData("10.10.0", "v10.10.0")]
        [InlineData("10.14.2", "v10.14.2")]
        [InlineData("8", "v" + NodeVersions.Node8Version)]
        [InlineData("10", "v" + NodeVersions.Node10Version)]
        [InlineData("12", "v" + NodeVersions.Node12Version)]
        [InlineData(NodeVersions.Node8Version, "v" + NodeVersions.Node8Version)]
        [InlineData(NodeVersions.Node10Version, "v" + NodeVersions.Node10Version)]
        [InlineData(NodeVersions.Node12Version, "v" + NodeVersions.Node12Version)]
        [InlineData(FinalStretchVersions.FinalStretchNode14Version, "v" + FinalStretchVersions.FinalStretchNode14Version)]
        [InlineData("lts", "v" + FinalStretchVersions.FinalStretchNode14Version)]
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
        [Theory, Trait("category", "latest")]
        // Only version 6 of npm is upgraded, so the following should remain unchanged.
        [InlineData("10.1", "5.6.0")]
        // Make sure the we get the upgraded version of npm in the following cases
        [InlineData("10.10.0", "6.9.0")]
        [InlineData("10.14.2", "6.9.0")]
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

        [Fact, Trait("category", "latest")]
        public void NpmVersion_IsNotUpgraded_To_6_9_0()
        {
            // Arrange
            var nodeVersion = NodeVersions.Node12Version;
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
                    Assert.DoesNotContain("6.9.0", actualOutput);
                },
                result.GetDebugInfo());
        }

        [Trait("platform", "python")]
        [Theory, Trait("category", "latest")]
        [InlineData("2", Python27VersionInfo)]
        [InlineData("2.7", Python27VersionInfo)]
        [InlineData(PythonVersions.Python27Version, Python27VersionInfo)]
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
        [Theory, Trait("category", "latest")]
        [InlineData("2", Python27VersionInfo)]
        [InlineData("2.7", Python27VersionInfo)]
        [InlineData(PythonVersions.Python27Version, Python27VersionInfo)]
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
        [Theory, Trait("category", "latest")]
        [InlineData("latest", Python38VersionInfo)]
        [InlineData("stable", Python38VersionInfo)]
        [InlineData("3", Python38VersionInfo)]
        [InlineData("3.6", Python36VersionInfo)]
        [InlineData(PythonVersions.Python36Version, Python36VersionInfo)]
        [InlineData("3.7", Python37VersionInfo)]
        [InlineData(PythonVersions.Python37Version, Python37VersionInfo)]
        [InlineData("3.8", Python38VersionInfo)]
        [InlineData(PythonVersions.Python38Version, Python38VersionInfo)]
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

        [Trait("platform", "dotnet")]
        [Theory, Trait("category", "latest")]
        [InlineData("DotNet", "dotnet")]
        [InlineData("dotnet", "dotNet")]
        [InlineData("DOTNET_VERSION", "DOTNET_VERSION")]
        [InlineData("dotnet_version", "dotnet_version")]
        public void DotNetAlias_UsesVersionSetOnBenvArgument_OverVersionSetInEnvironmentVariable(
            string environmentVariableName,
            string argumentName)
        {
            // Arrange
            var expectedOutput = DotNetCoreSdkVersions.DotNetCore30SdkVersion;
            var script = new ShellScriptBuilder()
                //.SetEnvironmentVariable("ENABLE_DYNAMIC_INSTALL", "true")
                .SetEnvironmentVariable(environmentVariableName, FinalStretchVersions.FinalStretchDotNetCore31SdkVersion)
                .Source($"benv {argumentName}={DotNetCoreSdkVersions.DotNetCore30SdkVersion}")
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
                    Assert.Equal(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Trait("platform", "dotnet")]
        [Fact, Trait("category", "latest")]
        public void RunningBenvMultipleTimes_HonorsLastRunArguments()
        {
            // Arrange
            var expectedOutput = DotNetCoreSdkVersions.DotNetCore30SdkVersion;
            var script = new ShellScriptBuilder()
                .Source($"benv dotnet={FinalStretchVersions.FinalStretchDotNetCore31SdkVersion}")
                .Source($"benv dotnet_version={DotNetCoreSdkVersions.DotNetCore30SdkVersion}")
                // benv should update the PATH environment in such a way that we should version 1
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
                    Assert.Equal(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }

        private void PhpAlias_UsesPhpLatestVersion_ByDefault_WhenNoExplicitVersionIsProvided(string buildImageName)
        {
            // Please note:
            // This test method has at least 1 wrapper function that pases the imageName parameter.

            // Arrange
            var phpVersion = PhpVersions.Php73Version;

            var expectedOutput = $"PHP {phpVersion} (cli) ";

            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = buildImageName,
                CommandToExecuteOnRun = "php",
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

        [Theory, Trait("category", "latest")]
        [InlineData(PhpVersions.Php73Version, PhpVersions.Php73Version)]
        [InlineData("7.3", PhpVersions.Php73Version)]
        [InlineData("7", PhpVersions.Php73Version)]
        public void Php_UsesVersion_SetOnBenv(string specifiedVersion, string expectedVersion)
        {
            // Arrange
            var expectedOutput = $"PHP {expectedVersion} (cli) ";
            var script = new ShellScriptBuilder()
                .Source($"benv php={specifiedVersion}")
                .AddCommand("php --version")
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
                    Assert.Contains(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "latest")]
        public void BenvShouldSetUpEnviroment_WhenMultiplePlatforms_AreSuppliedAsArguments()
        {
            // Arrange
            var expectedDotNetVersion = DotNetCoreSdkVersions.DotNetCore30SdkVersion;
            var expectedPythonVersion = Python36VersionInfo;
            var script = new ShellScriptBuilder()
                .Source($"benv dotnet={DotNetCoreSdkVersions.DotNetCore30SdkVersion} python=3.6")
                .AddCommand("dotnet --version")
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
                    Assert.Contains(expectedDotNetVersion, actualOutput);
                    Assert.Contains(expectedPythonVersion, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "latest")]
        public void BenvShouldSetUpEnviroment_UsingExactNames()
        {
            // Arrange
            var expectedDotNetVersion = FinalStretchVersions.FinalStretchDotNetCore31SdkVersion;
            var script = new ShellScriptBuilder()
                .Source("benv dotnet_foo=1")
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
                    Assert.Contains(expectedDotNetVersion, actualOutput);
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