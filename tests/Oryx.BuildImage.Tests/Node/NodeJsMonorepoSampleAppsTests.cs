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
    public class NodeJsMonorepoSampleAppsTest : SampleAppsTestBase
    {
        public NodeJsMonorepoSampleAppsTest(ITestOutputHelper output) : base(output)
        {
        }
        private DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "nodejs", sampleAppName));

        [Fact(Skip = "InstallLernaCommand is never executed in NodeBashBuildSnippet.sh.tpl to install lerna globally before use"), Trait("category", "githubactions")]
        public void GeneratesScript_AndBuildMonorepoAppUsingLerna_Npm()
        {
            // Arrange
            var appName = "monorepo-lerna-npm";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app1-output";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    SettingsKeys.EnableNodeMonorepoBuild,
                    true.ToString())
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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
                },
                result.GetDebugInfo());
        }

        [Fact(Skip = "InstallLernaCommand is never executed in NodeBashBuildSnippet.sh.tpl to install lerna globally before use"), Trait("category", "githubactions")]
        public void GeneratesScript_AndBuildMonorepoAppUsingLerna_Yarn()
        {
            // Arrange
            var appName = "monorepo-lerna-yarn";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app2-output";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    SettingsKeys.EnableNodeMonorepoBuild,
                    true.ToString())
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/@babel")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/universalify")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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
                },
                result.GetDebugInfo());
        }
    }
}