// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "db")]
    public class PulledBuildImageTypeTest
    {
        private readonly DockerCli _dockerCli;
        private readonly ITestOutputHelper _output;

        public PulledBuildImageTypeTest(ITestOutputHelper output)
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
        public void PulledLatestStretchBuildImages_Contains_BUILDIMAGE_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDIMAGE_TYPE_Info(Settings.WithRootAccessBuildImageName, "full");
        }

        [Fact]
        [Trait("build-image", "lts-versions-debian-stretch")]
        public void PulledLtsVersionsStretchBuildImages_Contains_BUILDIMAGE_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDIMAGE_TYPE_Info(Settings.WithRootAccessLtsVersionsBuildImageName, "ltsversions");
        }

        [Fact]
        [Trait("build-image", "github-actions-debian-stretch")]
        public void PulledGitHubActionsStretchBuildImages_Contains_BUILDIMAGE_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDIMAGE_TYPE_Info(Settings.GitHubActionsBuildImageName, "githubactions");
        }

        [Fact]
        [Trait("build-image", "cli-debian-stretch")]
        public void PulledCliStretchBuildImages_Contains_BUILDIMAGE_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDIMAGE_TYPE_Info(Settings.CliBuildImageName, "cli");
        }

        [Fact]
        [Trait("build-image", "cli-debian-buster")]
        public void PulledCliBusterBuildImages_Contains_BUILDIMAGE_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDIMAGE_TYPE_Info(Settings.CliBusterBuildImageName, "cli");
        }

        [Fact]
        [Trait("build-image", "azfunc-jamstack-debian-stretch")]
        public void PulledJamstackStretchBuildImages_Contains_BUILDIMAGE_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDIMAGE_TYPE_Info(Settings.JamStackBuildImageName, "jamstack");
        }

        [Fact]
        [Trait("build-image", "vso-ubuntu-focal")]
        public void PulledVsoFocalBuildImages_Contains_BUILDIMAGE_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDIMAGE_TYPE_Info(Settings.VsoUbuntuBuildImageName, "vso-focal");
        }

        private void PulledBuildImages_Contains_BUILDIMAGE_TYPE_Info(string buildImageName, string expectedBuildImageType)
        {
            // Arrange and Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = buildImageName,
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", "cat /opt/oryx/.imagetype" }
            });

            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedBuildImageType, actualOutput);
                },
                result.GetDebugInfo());
        }
        
    }
}