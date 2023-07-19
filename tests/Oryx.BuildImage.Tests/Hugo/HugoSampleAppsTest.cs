// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class HugoSampleAppsTest : HugoSampleAppsTestBase
    {
        public HugoSampleAppsTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact, Trait("category", "latest")]
        public void PipelineTestInvocationLatest()
        {
            GeneratesScript_AndBuilds(Settings.BuildImageName);
        }

        [Fact, Trait("category", "ltsversions")]
        public void PipelineTestInvocationLtsVersions()
        {
            GeneratesScript_AndBuilds(Settings.LtsVersionsBuildImageName);
        }

        [Fact, Trait("category", "vso-focal")]
        public void PipelineTestInvocationVsoFocal()
        {
            var imageTestHelper = new ImageTestHelper();
            GeneratesScript_AndBuilds(imageTestHelper.GetVsoBuildImage(ImageTestHelperConstants.VsoFocal));
        }

        [Fact, Trait("category", "jamstack")]
        public void PipelineTestInvocationJamstack()
        {
            var imageTestHelper = new ImageTestHelper();
            GeneratesScript_AndBuilds(imageTestHelper.GetAzureFunctionsJamStackBuildImage());
        }

        [Fact, Trait("category", "cli-stretch")]
        public void PipelineTestInvocationCli()
        {
            var imageTestHelper = new ImageTestHelper();
            GeneratesScript_AndBuilds(imageTestHelper.GetCliImage(ImageTestHelperConstants.CliRepository));
        }

        [Fact, Trait("category", "cli-buster")]
        public void PipelineTestInvocationCliBuster()
        {
            var imageTestHelper = new ImageTestHelper();
            GeneratesScript_AndBuilds(imageTestHelper.GetCliImage(ImageTestHelperConstants.CliBusterTag));
        }

        [Fact, Trait("category", "cli-bullseye")]
        public void PipelineTestInvocationCliBullseye()
        {
            var imageTestHelper = new ImageTestHelper();
            GeneratesScript_AndBuilds(imageTestHelper.GetCliImage(ImageTestHelperConstants.CliBullseyeTag));
        }


        private void GeneratesScript_AndBuilds(string buildImageName)
        {
            // Please note:
            // This test method has at least 1 wrapper function that pases the imageName parameter.

            // Arrange
            var volume = CreateSampleAppVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/public/index.xml")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = buildImageName,
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("Using Hugo version:", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory, Trait("category", "githubactions")]
        [InlineData("hugo-sample")]
        [InlineData("hugo-sample-json")]
        [InlineData("hugo-sample-yaml")]
        [InlineData("hugo-sample-yml")]
        public void CanBuildHugoAppHavingDifferentConfigFileTypes(string appName)
        {
            // Arrange
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/public/index.xml")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("Using Hugo version:", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void CanBuildHugoAppHavingPackageJson_ByExplicitlySpecifyingHugoPlatform()
        {
            // Idea is here that even though the app has a package.json, a user can explicitly choose for Hugo
            // platform to take care of build.

            // Arrange
            var volume = CreateSampleAppVolume("hugo-sample-with-packagejson");
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} --platform hugo")
                .AddFileExistsCheck($"{appOutputDir}/public/index.xml")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("Using Hugo version:", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact,Trait("category", "githubactions")]
        public void CanBuildHugoAppWithNewHugoConfigName()
        {
            // Hugo changed naming convention from config.toml etc. to hugo.toml etc.
            // This test is just a safety check making sure new config name is recognized and app can built correctly

            // Arrange
            var volume = CreateSampleAppVolume("hugo-sample-new-config-name");
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} --platform hugo")
                .AddFileExistsCheck($"{appOutputDir}/public/index.xml")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("Using Hugo version:", result.StdOut);
                },
                result.GetDebugInfo());
        }
    }
}
