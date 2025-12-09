// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Java;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    [Trait("platform", "java")]
    [Skip("Java detection disabled during Oryx build")]
    public class JavaDynamicInstallationTest : SampleAppsTestBase
    {
        public JavaDynamicInstallationTest(ITestOutputHelper output) : base(output)
        {
        }

        private DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "java", sampleAppName));

        public static TheoryData<string> VersionsData
        {
            get
            {
                var data = new TheoryData<string>();
                data.Add("17.0.1");
                data.Add("17.0.2");
                data.Add("11.0.14");
                return data;
            }
        }

        [Theory, Trait("category", "githubactions")]
        [MemberData(nameof(VersionsData))]
        public void BuildsMavenArcheTypeSampleWithDynamicInstallationGithubActions(string version)
        {
            BuildsMavenArcheTypeSampleWithDynamicInstallation(version, _imageHelper.GetGitHubActionsBuildImage());
        }

        private void BuildsMavenArcheTypeSampleWithDynamicInstallation(string version, string imageName)
        {
            // Arrange
            var appName = "MavenArcheType";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} --platform {JavaConstants.PlatformName} --platform-version {version} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/target/classes/microsoft/oryx/App.class")
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
                },
                result.GetDebugInfo());
        }

        [Theory, Trait("category", "githubactions")]
        [InlineData("14.0.2", "14.0.2")]
        [InlineData(null, JavaVersions.JavaVersion11)]
        public void BuildsWithDynamicInstallation_DefaultVersions(
            string envVarDefaultVersion, string expectedVersion)
        {
            // Arrange
            var appName = "MavenArcheType";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var manifestFile = $"{appOutputDir}/{FilePaths.BuildManifestFileName}";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(SettingsKeys.JavaDefaultVersion, envVarDefaultVersion)
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/target/classes/microsoft/oryx/App.class")
                .AddCommand($"cat {manifestFile}")
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
                    Assert.Contains($"{ManifestFilePropertyKeys.JavaVersion}=\"{expectedVersion}\"", result.StdOut);
                },
                result.GetDebugInfo());
        }

    }
}