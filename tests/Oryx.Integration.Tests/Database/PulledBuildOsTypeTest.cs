// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "db")]
    public class PulledBuildOsTypeTest
    {
        private readonly DockerCli _dockerCli;
        private readonly ITestOutputHelper _output;

        public PulledBuildOsTypeTest(ITestOutputHelper output)
        {
            _output = output;
            _dockerCli = new DockerCli();
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

        [Theory]
        [InlineData(Settings.WithRootAccessBuildImageName, "stretch")]
        [InlineData(Settings.WithRootAccessLtsVersionsBuildImageName, "stretch")]
        [InlineData(Settings.LtsVerionsBusterBuildImageName, "buster")]
        [InlineData(Settings.GitHubActionsBuildImageName, "stretch")]
        [InlineData(Settings.GitHubActionsBusterBuildImageName, "buster")]
        [InlineData(Settings.GitHubActionsBullseyeBuildImageName, "bullseye")]
        [InlineData(Settings.CliBuildImageName, "stretch")]
        [InlineData(Settings.CliBusterBuildImageName, "buster")]
        [InlineData(Settings.JamStackBuildImageName, "stretch")]
        [InlineData(Settings.JamStackBullseyeBuildImageName, "bullseye")]
        [InlineData(Settings.JamStackBusterBuildImageName, "buster")]
        [InlineData(Settings.VsoUbuntuBuildImageName, "focal-scm")]
        public void PulledBuildImages_Contains_BUILDOS_TYPE_Info(string buildImageName, string expectedBuildOsType)
        {
            // Arrange and Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = buildImageName,
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", "cat /opt/oryx/.ostype" }
            });

            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedBuildOsType, actualOutput);
                },
                result.GetDebugInfo());
        }
    }
}
