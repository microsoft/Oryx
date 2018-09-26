// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using Oryx.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.BuildImage.Tests
{
    public class VersionInformationTest
    {
        private readonly ITestOutputHelper _output;
        private readonly DockerCli _dockerCli;

        public VersionInformationTest(ITestOutputHelper output)
        {
            _output = output;
            _dockerCli = new DockerCli();
        }

        [Fact]
        public void DotnetAlias_UsesLatestVersion()
        {
            // Arrange
            var expectedOutput = "2.1.400";

            // Act
            var result = _dockerCli.Run(
                BuildImageTestSettings.BuildImageName,
                commandToExecuteOnRun: "dotnet",
                commandArguments: new[] { "--version" });

            // Assert
            var actualOutput = result.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Node_UsesLTSVersion_ByDefault_WhenNoExplicitVersionIsProvided()
        {
            // Arrange
            var expectedOutput = "v8.11.2";

            // Arrange & Act
            var result = _dockerCli.Run(
                BuildImageTestSettings.BuildImageName,
                commandToExecuteOnRun: "node",
                commandArguments: new[] { "--version" });

            // Assert
            var actualOutput = result.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void PythonAlias_UsesPython2_ByDefault_WhenNoExplicitVersionIsProvided()
        {
            // Arrange
            var expectedOutput = "Python 2.7.15";

            // Arrange & Act
            var result = _dockerCli.Run(
                BuildImageTestSettings.BuildImageName,
                commandToExecuteOnRun: "python",
                commandArguments: new[] { "--version" });

            // Assert
            var actualOutput = result.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Python3Alias_UsesPythonLatestVersion_ByDefault_WhenNoExplicitVersionIsProvided()
        {
            // Arrange
            var expectedOutput = "Python 3.7.0";

            // Arrange & Act
            var result = _dockerCli.Run(
                BuildImageTestSettings.BuildImageName,
                commandToExecuteOnRun: "python3",
                commandArguments: new[] { "--version" });

            // Assert
            var actualOutput = result.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("4", "v4.5.0")]
        [InlineData("4.5", "v4.5.0")]
        [InlineData("4.5.0", "v4.5.0")]
        [InlineData("6", "v6.11.0")]
        [InlineData("6.11", "v6.11.0")]
        [InlineData("6.11.0", "v6.11.0")]
        [InlineData("lts", "v8.11.2")]
        [InlineData("8", "v8.11.2")]
        [InlineData("8.11", "v8.11.2")]
        [InlineData("8.11.2", "v8.11.2")]
        [InlineData("9", "v9.4.0")]
        [InlineData("9.4", "v9.4.0")]
        [InlineData("9.4.0", "v9.4.0")]
        [InlineData("latest", "v10.1.0")]
        [InlineData("10", "v10.1.0")]
        [InlineData("10.1", "v10.1.0")]
        [InlineData("10.1.0", "v10.1.0")]
        public void NodeAlias_UsesVersion_SpecifiedAtDockerRun(
            string versionSentToDockerRun,
            string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(
                BuildImageTestSettings.BuildImageName,
                new EnvironmentVariable("node", versionSentToDockerRun),
                commandToExecuteOnRun: "node",
                commandArguments: new[] { "--version" });

            // Assert
            var actualOutput = result.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("2", "Python 2.7.15")]
        [InlineData("2.7", "Python 2.7.15")]
        [InlineData("2.7.15", "Python 2.7.15")]
        public void PythonAlias_UsesVersion_SpecifiedAtDockerRun(
            string versionSentToDockerRun,
            string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(
                BuildImageTestSettings.BuildImageName,
                new EnvironmentVariable("python", versionSentToDockerRun),
                commandToExecuteOnRun: "python",
                commandArguments: new[] { "--version" });

            // Assert
            var actualOutput = result.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("2", "Python 2.7.15")]
        [InlineData("2.7", "Python 2.7.15")]
        [InlineData("2.7.15", "Python 2.7.15")]
        public void Python2Alias_UsesVersion_SpecifiedAtDockerRun(
            string versionSentToDockerRun,
            string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(
                BuildImageTestSettings.BuildImageName,
                new EnvironmentVariable("python", versionSentToDockerRun),
                commandToExecuteOnRun: "python2",
                commandArguments: new[] { "--version" });

            // Assert
            var actualOutput = result.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("latest", "Python 3.7.0")]
        [InlineData("3", "Python 3.7.0")]
        [InlineData("3.5", "Python 3.5.6")]
        [InlineData("3.5.6", "Python 3.5.6")]
        [InlineData("3.6", "Python 3.6.6")]
        [InlineData("3.6.6", "Python 3.6.6")]
        [InlineData("3.7", "Python 3.7.0")]
        [InlineData("3.7.0", "Python 3.7.0")]
        public void Python3Alias_UsesVersion_SpecifiedAtDockerRun(
            string versionSentToDockerRun,
            string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(
                BuildImageTestSettings.BuildImageName,
                new EnvironmentVariable("python", versionSentToDockerRun),
                commandToExecuteOnRun: "python3",
                commandArguments: new[] { "--version" });

            // Assert
            var actualOutput = result.ReplaceNewLine();
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