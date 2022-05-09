// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
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

        [Fact]
        public void GeneratesScript_AndBuildSinatraAppWithDynamicInstall()
        {
            var imageTestHelper = new ImageTestHelper();

            // Arrange
            var appName = "hello-world";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .ToString();

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
                    Assert.True(result.IsSuccess);
                    Assert.Contains("Golang version", result.StdOut);
                },
                result.GetDebugInfo());
        }
    }
}