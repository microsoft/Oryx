// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.RuntimeImage.Tests
{
    public class DotnetCoreImageVersionsTest
    {
        private readonly ITestOutputHelper _output;
        private readonly DockerCli _dockerCli;

        public DotnetCoreImageVersionsTest(ITestOutputHelper output)
        {
            _output = output;
            _dockerCli = new DockerCli();
        }

        [Theory]
        [InlineData("1.0", "Version  : 1.0.1")]
        [InlineData("1.1", "Version  : 1.1.10")]
        public void DotnetVersionMatchesImageName_NetCoreApp1Versions(string version, string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(
                "oryxdevms/dotnetcore-" + version + ":latest",
                commandToExecuteOnRun: "dotnet",
                commandArguments: new[] { "--version" });

            // Assert
            var actualOutput = result.Output.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.Contains(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("2.0", "Version  : 2.0.9")]
        [InlineData("2.1", "Version: 2.1.6")]
        [InlineData("2.2", "Version: 2.2.0")]
        public void DotnetVersionMatchesImageName(string version, string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(
                "oryxdevms/dotnetcore-" + version + ":latest",
                commandToExecuteOnRun: "dotnet",
                commandArguments: new[] { "--info" });

            // Assert
            var actualOutput = result.Output.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedOutput, actualOutput);
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