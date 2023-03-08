﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class PulledBuildImageTypeTest
    {
        private readonly DockerCli _dockerCli;
        private readonly ITestOutputHelper _output;
        private readonly ImageTestHelper _imageHelper;

        public PulledBuildImageTypeTest(ITestOutputHelper output)
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
        public void PulledLatestStretchBuildImages_Contains_BUILDIMAGE_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDIMAGE_TYPE_Info(_imageHelper.GetBuildImage(ImageTestHelperConstants.LatestStretchTag), "full");
        }

        [Fact]
        [Trait("category", "ltsversions")]
        public void PulledLtsVersionsStretchBuildImages_Contains_BUILDIMAGE_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDIMAGE_TYPE_Info(_imageHelper.GetBuildImage(ImageTestHelperConstants.LtsVersionsStretch), "ltsversions");
        }

        [Fact]
        [Trait("category", "githubactions")]
        public void PulledGitHubActionsStretchBuildImages_Contains_BUILDIMAGE_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDIMAGE_TYPE_Info(_imageHelper.GetBuildImage(ImageTestHelperConstants.GitHubActionsStretch), "githubactions");
        }

        [Fact]
        [Trait("category", "cli-stretch")]
        public void PulledCliStretchBuildImages_Contains_BUILDIMAGE_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDIMAGE_TYPE_Info(_imageHelper.GetCliImage(ImageTestHelperConstants.CliRepository), "cli");
        }

        [Fact]
        [Trait("category", "cli-buster")]
        public void PulledCliBusterBuildImages_Contains_BUILDIMAGE_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDIMAGE_TYPE_Info(_imageHelper.GetCliImage(ImageTestHelperConstants.CliBusterTag), "cli");
        }

        [Fact]
        [Trait("category", "cli-bullseye")]
        public void PulledCliBullseyeBuildImages_Contains_BUILDIMAGE_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDIMAGE_TYPE_Info(_imageHelper.GetCliImage(ImageTestHelperConstants.CliBullseyeTag), "cli");
        }


        [Fact]
        [Trait("category", "jamstack")]
        public void PulledJamstackStretchBuildImages_Contains_BUILDIMAGE_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDIMAGE_TYPE_Info(_imageHelper.GetBuildImage(ImageTestHelperConstants.AzureFunctionsJamStackStretch), "jamstack");
        }

        [Fact]
        [Trait("category", "vso-focal")]
        public void PulledVsoFocalBuildImages_Contains_BUILDIMAGE_TYPE_Info()
        {
            PulledBuildImages_Contains_BUILDIMAGE_TYPE_Info(_imageHelper.GetBuildImage(ImageTestHelperConstants.VsoFocal), "vso-focal");
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