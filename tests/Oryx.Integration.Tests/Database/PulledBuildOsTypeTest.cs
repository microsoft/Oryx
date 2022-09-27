// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System;
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

        [Fact]
        [Trait("build-image", "debian-stretch")]
        public void PulledLatestStretchBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(Settings.WithRootAccessBuildImageName, "DEBIAN|STRETCH");
        }

        [Fact]
        [Trait("build-image", "lts-versions-debian-stretch")]
        public void PulledLtsVersionsStretchBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(Settings.WithRootAccessLtsVersionsBuildImageName, "DEBIAN|STRETCH");
        }

        [Fact]
        [Trait("build-image", "lts-versions-debian-buster")]
        public void PulledLtsVersionsBusterBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(Settings.LtsVerionsBusterBuildImageName, "DEBIAN|BUSTER");
        }

        [Fact]
        [Trait("build-image", "github-actions-debian-stretch")]
        public void PulledGitHubActionsStretchBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(Settings.GitHubActionsBuildImageName, "DEBIAN|STRETCH");
        }

        [Fact]
        [Trait("build-image", "github-actions-debian-buster")]
        public void PulledGitHubActionsBusterBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(Settings.GitHubActionsBusterBuildImageName, "DEBIAN|BUSTER");
        }

        [Fact]
        [Trait("build-image", "github-actions-debian-bullseye")]
        public void PulledGitHubActionsBullseyeBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(Settings.GitHubActionsBullseyeBuildImageName, "DEBIAN|BULLSEYE");
        }

        [Fact]
        [Trait("build-image", "cli-debian-stretch")]
        public void PulledCliStretchBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(Settings.CliBuildImageName, "DEBIAN|STRETCH");
        }

        [Fact]
        [Trait("build-image", "cli-debian-buster")]
        public void PulledCliBusterBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(Settings.CliBusterBuildImageName, "DEBIAN|BUSTER");
        }

        [Fact]
        [Trait("build-image", "azfunc-jamstack-debian-stretch")]
        public void PulledJamstackStretchBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(Settings.JamStackBuildImageName, "DEBIAN|STRETCH");
        }

        [Fact]
        [Trait("build-image", "azfunc-jamstack-debian-buster")]
        public void PulledJamstackBusterBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(Settings.JamStackBusterBuildImageName, "DEBIAN|BUSTER");
        }

        [Fact]
        [Trait("build-image", "azfunc-jamstack-debian-bullseye")]
        public void PulledJamstackBullseyeBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(Settings.JamStackBullseyeBuildImageName, "DEBIAN|BULLSEYE");
        }

        [Fact]
        [Trait("build-image", "vso-ubuntu-focal")]
        public void PulledVsoFocalBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(Settings.VsoUbuntuBuildImageName, "DEBIAN|FOCAL-SCM");
        }

        private void PulledBuildImages_Contains_BUILDOS_TYPE_Info(string buildImageName, string expectedBuildOsType)
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
