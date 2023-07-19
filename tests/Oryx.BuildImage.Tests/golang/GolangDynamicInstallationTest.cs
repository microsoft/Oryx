// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    [Trait("platform", "golang-full")]
    public class GolangDynamicInstallationTest : SampleAppsTestBase
    {
        public GolangDynamicInstallationTest(ITestOutputHelper output) : base(output)
        {
        }

        private DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "golang", sampleAppName));

        public static TheoryData<string> ImageNameData
        {
            get
            {
                var data = new TheoryData<string>();
                var imageTestHelper = new ImageTestHelper();
                data.Add(imageTestHelper.GetLtsVersionsBuildImage());
                data.Add(imageTestHelper.GetGitHubActionsBuildImage());
                return data;
            }
        }

        [Fact, Trait("category", "ltsversions")]
        public void GeneratesScript_AndBuildGolangAppWithDynamicInstall_Lts()
        {
            GeneratesScript_AndBuildGolangAppWithDynamicInstall(_imageHelper.GetLtsVersionsBuildImage());
        }

        [Fact, Trait("category", "cli-stretch")]
        public void GeneratesScript_AndBuildGolangAppWithDynamicInstall_Cli()
        {
            GeneratesScript_AndBuildGolangAppWithDynamicInstall(_imageHelper.GetCliImage(ImageTestHelperConstants.CliRepository));
        }

        [Fact, Trait("category", "cli-buster")]
        public void GeneratesScript_AndBuildGolangAppWithDynamicInstall_CliBuster()
        {
            GeneratesScript_AndBuildGolangAppWithDynamicInstall(_imageHelper.GetCliImage(ImageTestHelperConstants.CliBusterTag));
        }

        [Fact, Trait("category", "cli-bullseye")]
        public void GeneratesScript_AndBuildGolangAppWithDynamicInstall_CliBullseye()
        {
            GeneratesScript_AndBuildGolangAppWithDynamicInstall(_imageHelper.GetCliImage(ImageTestHelperConstants.CliBullseyeTag));
        }

        private void GeneratesScript_AndBuildGolangAppWithDynamicInstall(string imageName)
        {
            var imageTestHelper = new ImageTestHelper();

            // Arrange
            var appName = "hello-world";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = imageName,
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("Golang version", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "ltsversions")]
        public void GeneratesScript_AndBuildGolangAppWithoutGoMod()
        {
            var imageTestHelper = new ImageTestHelper();

            // Arrange
            var appName = "hello-world";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddCommand($"echo RandomText > {appDir}/go.mod")  // triggers a failure
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .ToString();
            // Regex will match:
            // "yyyy-mm-dd hh:mm:ss"|ERROR|go: errors parsing go.mod
            Regex regex = new Regex(@"""[0-9]{4}-(0[1-9]|1[0-2])-(0[1-9]|[1-2][0-9]|3[0-1]) (0[0-9]|1[0-9]|2[0-3]):([0-5][0-9]):([0-5][0-9])""\|ERROR\|go:\serrors\sparsing\sgo\.mod.*");

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = imageTestHelper.GetLtsVersionsBuildImage(),   
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.False(result.IsSuccess);
                    Match match = regex.Match(result.StdOut);
                    Assert.True(match.Success);
                },
                result.GetDebugInfo());
        }
    }
}