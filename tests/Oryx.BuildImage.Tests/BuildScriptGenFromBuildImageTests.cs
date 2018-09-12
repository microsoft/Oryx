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
                imageId: BuildImageTestSettings.BuildImageName,
                commandToExecuteOnRun: "/opt/buildscriptgen/GenerateBuildScript");

            // Assert
            RunAsserts(() =>
            {
                Assert.Contains("field is required.", result.Output);
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