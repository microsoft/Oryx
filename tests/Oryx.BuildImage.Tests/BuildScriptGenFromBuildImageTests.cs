// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using Oryx.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.BuildImage.Tests
{
    public class BuildScriptGenFromBuildImageTests
    {
        private readonly ITestOutputHelper _output;
        private readonly DockerCli _dockerCli;

        public BuildScriptGenFromBuildImageTests(ITestOutputHelper output)
        {
            _output = output;
            _dockerCli = new DockerCli();
        }

        [Fact]
        public void BuildScriptGenIsIncludedInBuildImage()
        {
            // Arrange & Act
            var result = _dockerCli.Run(
                imageId: Settings.BuildImageName,
                commandToExecuteOnRun: "oryx",
                commandArguments: null);

            // Assert
            RunAsserts(() =>
            {
                Assert.True(result.IsSuccess);
                // Help text must be shown
                Assert.Contains("Generates and runs build scripts for multiple languages.", result.Output);
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