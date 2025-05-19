// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Ruby;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    [Trait("platform", "ruby")]
    public class RubyDynamicInstallationTest : SampleAppsTestBase
    {
        public RubyDynamicInstallationTest(ITestOutputHelper output) : base(output)
        {
        }

        private DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "ruby", sampleAppName));

        [Fact, Trait("category", "githubactions")]
        public void PipelineTestInvocationGithubActions()
        {
            var imageTestHelper = new ImageTestHelper();
            GeneratesScript_AndBuildSinatraAppWithDynamicInstall(
                RubyVersions.Ruby30Version, imageTestHelper.GetGitHubActionsBuildImage());
            GeneratesScript_AndBuildSinatraAppWithDynamicInstall(
                RubyVersions.Ruby31Version, imageTestHelper.GetGitHubActionsBuildImage());
        }

        private void GeneratesScript_AndBuildSinatraAppWithDynamicInstall(string version, string buildImageName)
        {
            // Please note:
            // This test method has at least 1 wrapper function that pases the imageName parameter.

            // Arrange
            var appName = "sinatra-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} --platform {RubyConstants.PlatformName} --platform-version {version} -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = buildImageName,
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
                    Assert.Contains("Ruby version", result.StdOut);
                },
                result.GetDebugInfo());
        }
    }
}