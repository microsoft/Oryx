// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class NodeDynamicInstallationTest : NodeJSSampleAppsTestBase
    {
        private readonly string DefaultInstallationRootDir = "/opt/nodejs";

        public NodeDynamicInstallationTest(ITestOutputHelper output) : base(output)
        {
        }

        public static TheoryData<string, string> ImageNameData
        {
            get
            {
                var data = new TheoryData<string, string>();
                var imageTestHelper = new ImageTestHelper();
                data.Add("17.1.0", imageTestHelper.GetGitHubActionsBuildImage());
                data.Add(FinalStretchVersions.FinalStretchNode14Version, imageTestHelper.GetGitHubActionsBuildImage());
                data.Add(FinalStretchVersions.FinalStretchNode16Version, imageTestHelper.GetGitHubActionsBuildImage());
                return data;
            }
        }

        public static TheoryData<string, string> ImageNameDataCliBuilderBullseye
        {
            get
            {
                var data = new TheoryData<string, string>();
                var imageTestHelper = new ImageTestHelper();
                data.Add("12.22.11", imageTestHelper.GetCliBuilderImage(ImageTestHelperConstants.CliBuilderBullseyeTag));
                data.Add("14.19.1", imageTestHelper.GetCliBuilderImage(ImageTestHelperConstants.CliBuilderBullseyeTag));
                data.Add("16.14.2", imageTestHelper.GetCliBuilderImage(ImageTestHelperConstants.CliBuilderBullseyeTag));
                return data;
            }
        }


        [Theory, Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-debian-stretch")]
        [MemberData(nameof(ImageNameData))]
        public void GeneratesScript_AndBuildNodeAppsWithDynamicInstallationGithubActions(string version, string buildImageName)
        {
            GeneratesScript_AndBuildNodeAppsWithDynamicInstallation(version, buildImageName);
        }

        [Theory, Trait("category", "cli-builder-bullseye")]
        [MemberData(nameof(ImageNameDataCliBuilderBullseye))]
        public void GeneratesScript_AndBuildNodeAppsWithDynamicInstallationCliBuilderBullseye(string version, string buildImageName)
        {
            GeneratesScript_AndBuildNodeAppsWithDynamicInstallation(version, buildImageName);
        }

        private void GeneratesScript_AndBuildNodeAppsWithDynamicInstallation(string version, string buildImageName)
        {
            // Arrange
            var devPackageName = "nodemon";
            var prodPackageName = "express";
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} --platform-version {version} --debug")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{devPackageName}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{prodPackageName}")
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
                },
                result.GetDebugInfo());
        }

        [Theory, Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-debian-stretch")]
        [InlineData("14.19.1", "14.19.1")]
        [InlineData("16", "16.20.2")]
        public void GeneratesScript_AndBuildNodeAppsWithDynamicInstallation_DefaultEnvVar(string defaultVersion, string expectedVersion)
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var manifestFile = $"{appOutputDir}/{FilePaths.BuildManifestFileName}";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(SettingsKeys.NodeDefaultVersion, defaultVersion)
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} --debug")
                .AddCommand($"cat {manifestFile}")
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
                    Assert.Contains($"{ManifestFilePropertyKeys.NodeVersion}=\"{expectedVersion}\"", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "ltsversions")]
        [Trait("build-image", "lts-versions-debian-stretch")]
        public void DynamicallyInstallsNodeRuntimeAndBuilds()
        {
            // Arrange
            // Here 'nodemon' and 'express' are packages specified in package.json
            var devPackageName = "nodemon";
            var prodPackageName = "express";
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{devPackageName}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{prodPackageName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _restrictedPermissionsImageHelper.GetLtsVersionsBuildImage(),
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

        [Fact, Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-debian-stretch")]
        public void DynamicInstall_ReInstallsSdk_IfSentinelFileIsNotPresent()
        {
            // Arrange
            var version = "12.22.4"; //NOTE: use the full version so that we know the install directory path
            var installationDir = $"{BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot}/nodejs/{version}";
            var sentinelFile = $"{installationDir}/{SdkStorageConstants.SdkDownloadSentinelFileName}";
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var buildCmd = $"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {NodeConstants.PlatformName} --platform-version {version}";
            var script = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddBuildCommand(buildCmd)
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddFileExistsCheck(sentinelFile)
                .AddCommand($"rm -f {sentinelFile}")
                .AddBuildCommand(buildCmd)
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddFileExistsCheck(sentinelFile)
                .AddBuildCommand(buildCmd)
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
                },
                result.GetDebugInfo());

        }

        [Fact, Trait("category", "latest")]
        [Trait("build-image", "debian-stretch")]
        public void BuildsApplication_ByDynamicallyInstalling_IntoCustomDynamicInstallationDir()
        {
            // Arrange
            var version = "14.0.0"; //NOTE: use the full version so that we know the install directory path
            var expectedDynamicInstallRootDir = "/foo/bar";
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var buildCmd = $"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {NodeConstants.PlatformName} --platform-version {version} " +
                $"--dynamic-install-root-dir {expectedDynamicInstallRootDir}";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(buildCmd)
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddDirectoryExistsCheck(
                $"{expectedDynamicInstallRootDir}/{NodeConstants.PlatformName}/{version}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetBuildImage(),
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

        [Fact, Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-debian-buster")]
        public void BuildNodeApp_AfterInstallingBusterSpecificSdk()
        {
            // Arrange
            var version = "16.20.2";

            var devPackageName = "nodemon";
            var prodPackageName = "express";
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} --platform-version {version} --debug")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{devPackageName}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{prodPackageName}")
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
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-debian-buster")]
        public void NodeFails_ToInstallStretchSdk_OnBusterImage()
        {
            Run_NodeFails_ToInstallStretchSdk_OnNonStretchImage(ImageTestHelperConstants.GitHubActionsBuster);
        }

        [Fact, Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        public void NodeFails_ToInstallStretchSdk_OnBullseyeImage()
        {
            Run_NodeFails_ToInstallStretchSdk_OnNonStretchImage(ImageTestHelperConstants.GitHubActionsBullseye);
        }

        private void Run_NodeFails_ToInstallStretchSdk_OnNonStretchImage(string imageTag)
        {
            // Arrange
            var version = "9.4.0"; // version only exists for stretch images

            var devPackageName = "nodemon";
            var prodPackageName = "express";
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} --platform-version {version} --debug")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{devPackageName}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{prodPackageName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetBuildImage(imageTag),
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.False(result.IsSuccess);
                    Assert.Contains($"Error: Platform '{NodeConstants.PlatformName}' version '{version}' is unsupported.", result.StdErr);
                },
                result.GetDebugInfo());
        }

        [Theory, Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        [InlineData(NodeVersions.Node18Version, ImageTestHelperConstants.GitHubActionsBullseye)]
        [InlineData(NodeVersions.Node20Version, ImageTestHelperConstants.GitHubActionsBullseye)]
        [InlineData(NodeVersions.Node22Version, ImageTestHelperConstants.GitHubActionsBullseye)]
        public void GeneratesScript_AndBuildNodeAppsWithDynamicInstallationOnBullseyeImage(string version, string buildImageName)
        {
            // Arrange
            var devPackageName = "nodemon";
            var prodPackageName = "express";
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} --platform-version {version} --debug")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{devPackageName}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{prodPackageName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetBuildImage(buildImageName),
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
        [Trait("build-image", "github-actions-debian-bookworm")]
        [InlineData(NodeVersions.Node20Version, ImageTestHelperConstants.GitHubActionsBookworm)]
        [InlineData(NodeVersions.Node22Version, ImageTestHelperConstants.GitHubActionsBookworm)]
        public void GeneratesScript_AndBuildNodeAppsWithDynamicInstallationOnBookwormImage(string version, string buildImageName)
        {
            // Arrange
            var devPackageName = "nodemon";
            var prodPackageName = "express";
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} --platform-version {version} --debug")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{devPackageName}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{prodPackageName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetBuildImage(buildImageName),
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

        private string GetSnippetToCleanUpExistingInstallation()
        {
            return $"rm -rf {DefaultInstallationRootDir}; mkdir -p {DefaultInstallationRootDir}";
        }
    }
}