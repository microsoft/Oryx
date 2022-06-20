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
        [InlineData(Settings.WithRootAccessBuildImageName, "DEBIAN|STRETCH")]
        [InlineData(Settings.WithRootAccessLtsVersionsBuildImageName, "DEBIAN|STRETCH")]
        [InlineData(Settings.LtsVerionsBusterBuildImageName, "DEBIAN|BUSTER")]
        [InlineData(Settings.GitHubActionsBuildImageName, "DEBIAN|STRETCH")]
        [InlineData(Settings.GitHubActionsBusterBuildImageName, "DEBIAN|BUSTER")]
        [InlineData(Settings.GitHubActionsBullseyeBuildImageName, "DEBIAN|BULLSEYE")]
        [InlineData(Settings.CliBuildImageName, "DEBIAN|STRETCH")]
        [InlineData(Settings.CliBusterBuildImageName, "DEBIAN|BUSTER")]
        [InlineData(Settings.JamStackBuildImageName, "DEBIAN|STRETCH")]
        [InlineData(Settings.JamStackBullseyeBuildImageName, "DEBIAN|BULLSEYE")]
        [InlineData(Settings.JamStackBusterBuildImageName, "DEBIAN|BUSTER")]
        [InlineData(Settings.VsoUbuntuBuildImageName, "DEBIAN|FOCAL-SCM")]
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
