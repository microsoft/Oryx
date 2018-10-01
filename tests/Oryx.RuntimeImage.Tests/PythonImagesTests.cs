// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using Oryx.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.RuntimeImage.Tests
{
    public class PythonImagesTest
    {
        private readonly ITestOutputHelper _output;
        private readonly DockerCli _dockerCli;

        public PythonImagesTest(ITestOutputHelper output)
        {
            _output = output;
            _dockerCli = new DockerCli();
        }

        [Theory]
        [InlineData("3.6.6")]
        [InlineData("3.7.0")]
        public void PythonVersionMatchesImageName(string pythonVersion)
        {
            // Arrange & Act
            var expectedPythonVersion = "Python " + pythonVersion;
            var result = _dockerCli.Run(
                "oryxdevms/python-" + pythonVersion + ":latest",
                commandToExecuteOnRun: "python",
                commandArguments: new[] { "--version" });

            // Assert
            var actualOutput = result.Output.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedPythonVersion, actualOutput);
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
