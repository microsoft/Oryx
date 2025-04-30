// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class PulledBuildOsTypeTest
    {
        private readonly DockerCli _dockerCli;
        private readonly ITestOutputHelper _output;
        private readonly ImageTestHelper _imageHelper;

        public PulledBuildOsTypeTest(ITestOutputHelper output)
        {
            _output = output;
            _dockerCli = new DockerCli();
            _imageHelper = new ImageTestHelper(output);
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
        [Trait("category", "latest")]
        public void PulledLatestStretchBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(_imageHelper.GetBuildImage(ImageTestHelperConstants.LatestStretchTag), "DEBIAN|STRETCH");
        }

        [Fact]
        [Trait("category", "ltsversions")]
        public void PulledLtsVersionsStretchBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(_imageHelper.GetBuildImage(ImageTestHelperConstants.LtsVersionsStretch), "DEBIAN|STRETCH");
        }

        [Fact]
        [Trait("category", "ltsversions")]
        public void PulledLtsVersionsBusterBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(_imageHelper.GetBuildImage(ImageTestHelperConstants.LtsVersionsBuster), "DEBIAN|BUSTER");
        }

        // [Fact]
        // [Trait("category", "githubactions")]
        // public void PulledGitHubActionsStretchBuildImages_Contains_BUILDOS_TYPE_Info()
        // {
        //     PulledBuildImages_Contains_BUILDOS_TYPE_Info(_imageHelper.GetBuildImage(ImageTestHelperConstants.GitHubActionsStretch), "DEBIAN|STRETCH");
        // }

        [Fact]
        [Trait("category", "githubactions")]
        public void PulledGitHubActionsBusterBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(_imageHelper.GetBuildImage(ImageTestHelperConstants.GitHubActionsBuster), "DEBIAN|BUSTER");
        }

        [Fact]
        [Trait("category", "githubactions")]
        public void PulledGitHubActionsBullseyeBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(_imageHelper.GetBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), "DEBIAN|BULLSEYE");
        }
        
        [Fact]
        [Trait("category", "jamstack")]
        public void PulledJamstackBusterBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(_imageHelper.GetBuildImage(ImageTestHelperConstants.AzureFunctionsJamStackBuster), "DEBIAN|BUSTER");
        }

        [Fact]
        [Trait("category", "jamstack")]
        public void PulledJamstackBullseyeBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(_imageHelper.GetBuildImage(ImageTestHelperConstants.AzureFunctionsJamStackBullseye), "DEBIAN|BULLSEYE");
        }

        [Fact]
        [Trait("category", "vso-focal")]
        public void PulledVsoFocalBuildImages_Contains_BUILDOS_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDOS_TYPE_Info(_imageHelper.GetBuildImage(ImageTestHelperConstants.VsoFocal), "DEBIAN|FOCAL-SCM");
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
