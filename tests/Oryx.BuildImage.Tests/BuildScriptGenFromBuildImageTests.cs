// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
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
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                CommandToExecuteOnRun = "oryx",
            });

            // Assert
            RunAsserts(() =>
            {
                Assert.True(result.IsSuccess);
                // Help text must be shown
                Assert.Contains("Generates and runs build scripts for multiple languages.", result.StdOut);
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