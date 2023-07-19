﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildImage.Tests;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class PhpDynamicInstallationTest : SampleAppsTestBase
    {
        public PhpDynamicInstallationTest(ITestOutputHelper output) : base(output)
        {
        }

        public static TheoryData<string, string, string> VersionAndImageNameData
        {
            get
            {
                // test default php-composer version
                var data = new TheoryData<string, string, string>();
                data.Add(
                    PhpVersions.Php73Version, 
                    ImageTestHelper.WithRestrictedPermissions().GetGitHubActionsBuildImage(), 
                    PhpVersions.ComposerVersion
                );
                var imageHelper = new ImageTestHelper();
                data.Add(PhpVersions.Php74Version, imageHelper.GetGitHubActionsBuildImage(), PhpVersions.ComposerVersion);
                data.Add(PhpVersions.Php74Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.ComposerVersion);
                data.Add(PhpVersions.Php80Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBuster), PhpVersions.ComposerVersion);
                data.Add(PhpVersions.Php81Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBuster), PhpVersions.ComposerVersion);
                data.Add(PhpVersions.Php82Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBuster), PhpVersions.ComposerVersion);

                // test latest php-composer version
                data.Add(
                    PhpVersions.Php73Version,
                    ImageTestHelper.WithRestrictedPermissions().GetGitHubActionsBuildImage(),
                    PhpVersions.Composer23Version
                );
                data.Add(PhpVersions.Php74Version, imageHelper.GetGitHubActionsBuildImage(), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php80Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBuster), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php81Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBuster), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php82Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBuster), PhpVersions.Composer23Version);
                return data;
            }
        }

        public static TheoryData<string, string, string> VersionAndImageNameDataCli
        {
            get
            {
                var data = new TheoryData<string, string, string>();
                var imageHelper = new ImageTestHelper();
                data.Add(PhpVersions.Php73Version, imageHelper.GetCliImage(),PhpVersions.ComposerVersion);
                data.Add(PhpVersions.Php74Version, imageHelper.GetCliImage(), PhpVersions.ComposerVersion);

                // test latest php-composer version
                data.Add(PhpVersions.Php73Version, imageHelper.GetCliImage(), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php74Version, imageHelper.GetCliImage(), PhpVersions.Composer23Version);
                return data;
            }
        }

        public static TheoryData<string, string, string> VersionAndImageNameDataCliBuster
        {
            get
            {
                var data = new TheoryData<string, string, string>();
                var imageHelper = new ImageTestHelper();
                data.Add(PhpVersions.Php74Version, imageHelper.GetCliImage(ImageTestHelperConstants.CliBusterTag), PhpVersions.ComposerVersion);
                data.Add(PhpVersions.Php80Version, imageHelper.GetCliImage(ImageTestHelperConstants.CliBusterTag), PhpVersions.ComposerVersion);
                data.Add(PhpVersions.Php81Version, imageHelper.GetCliImage(ImageTestHelperConstants.CliBusterTag), PhpVersions.ComposerVersion);
                data.Add(PhpVersions.Php82Version, imageHelper.GetCliImage(ImageTestHelperConstants.CliBusterTag), PhpVersions.ComposerVersion);

                // test latest php-composer version
                data.Add(PhpVersions.Php74Version, imageHelper.GetCliImage(ImageTestHelperConstants.CliBusterTag), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php80Version, imageHelper.GetCliImage(ImageTestHelperConstants.CliBusterTag), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php81Version, imageHelper.GetCliImage(ImageTestHelperConstants.CliBusterTag), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php82Version, imageHelper.GetCliImage(ImageTestHelperConstants.CliBusterTag), PhpVersions.Composer23Version);
                return data;
            }
        }

        public static TheoryData<string, string, string> VersionAndImageNameDataCliBullseye
        {
            get
            {
                var data = new TheoryData<string, string, string>();
                var imageHelper = new ImageTestHelper();
                data.Add(PhpVersions.Php74Version, imageHelper.GetCliImage(ImageTestHelperConstants.CliBullseyeTag), PhpVersions.ComposerVersion);
                data.Add(PhpVersions.Php80Version, imageHelper.GetCliImage(ImageTestHelperConstants.CliBullseyeTag), PhpVersions.ComposerVersion);
                data.Add(PhpVersions.Php81Version, imageHelper.GetCliImage(ImageTestHelperConstants.CliBullseyeTag), PhpVersions.ComposerVersion);
                data.Add(PhpVersions.Php82Version, imageHelper.GetCliImage(ImageTestHelperConstants.CliBullseyeTag), PhpVersions.ComposerVersion);

                // test latest php-composer version
                data.Add(PhpVersions.Php74Version, imageHelper.GetCliImage(ImageTestHelperConstants.CliBullseyeTag), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php80Version, imageHelper.GetCliImage(ImageTestHelperConstants.CliBullseyeTag), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php81Version, imageHelper.GetCliImage(ImageTestHelperConstants.CliBullseyeTag), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php82Version, imageHelper.GetCliImage(ImageTestHelperConstants.CliBullseyeTag), PhpVersions.Composer23Version);
                return data;
            }
        }
        
        public static TheoryData<string, string, string> VersionAndImageNameDataCliBuilderBullseye
        {
            get
            {
                var data = new TheoryData<string, string, string>();
                var imageHelper = new ImageTestHelper();
                data.Add(PhpVersions.Php74Version, imageHelper.GetCliBuilderImage(ImageTestHelperConstants.CliBuilderBullseyeTag), PhpVersions.ComposerVersion);
                data.Add(PhpVersions.Php80Version, imageHelper.GetCliBuilderImage(ImageTestHelperConstants.CliBuilderBullseyeTag), PhpVersions.ComposerVersion);
                data.Add(PhpVersions.Php81Version, imageHelper.GetCliBuilderImage(ImageTestHelperConstants.CliBuilderBullseyeTag), PhpVersions.ComposerVersion);
                data.Add(PhpVersions.Php82Version, imageHelper.GetCliBuilderImage(ImageTestHelperConstants.CliBuilderBullseyeTag), PhpVersions.ComposerVersion);

                // test latest php-composer version
                data.Add(PhpVersions.Php74Version, imageHelper.GetCliBuilderImage(ImageTestHelperConstants.CliBuilderBullseyeTag), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php80Version, imageHelper.GetCliBuilderImage(ImageTestHelperConstants.CliBuilderBullseyeTag), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php81Version, imageHelper.GetCliBuilderImage(ImageTestHelperConstants.CliBuilderBullseyeTag), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php82Version, imageHelper.GetCliBuilderImage(ImageTestHelperConstants.CliBuilderBullseyeTag), PhpVersions.Composer23Version);
                return data;
            }
        }


        [Theory, Trait("category", "githubactions")]
        [MemberData(nameof(VersionAndImageNameData))]
        public void BuildsAppByInstallingSdkDynamicallyGithubActions(string phpVersion, string imageName, string phpComposerVersion)
        {
            BuildsAppByInstallingSdkDynamically(phpVersion, imageName, phpComposerVersion);
        }

        [Theory, Trait("category", "cli-stretch")]
        [MemberData(nameof(VersionAndImageNameDataCli))]
        public void BuildsAppByInstallingSdkDynamicallyCli(string phpVersion, string imageName, string phpComposerVersion)
        {
            BuildsAppByInstallingSdkDynamically(phpVersion, imageName, phpComposerVersion, "/opt/php");
        }

        [Theory, Trait("category", "cli-buster")]
        [MemberData(nameof(VersionAndImageNameDataCliBuster))]
        public void BuildsAppByInstallingSdkDynamicallyCliBuster(string phpVersion, string imageName, string phpComposerVersion)
        {
            BuildsAppByInstallingSdkDynamically(phpVersion, imageName, phpComposerVersion, "/opt/php");
        }

        [Theory, Trait("category", "cli-bullseye")]
        [MemberData(nameof(VersionAndImageNameDataCliBullseye))]
        public void BuildsAppByInstallingSdkDynamicallyCliBullseye(string phpVersion, string imageName, string phpComposerVersion)
        {
            BuildsAppByInstallingSdkDynamically(phpVersion, imageName, phpComposerVersion, "/opt/php");
        }

        [Theory, Trait("category", "cli-builder-bullseye")]
        [MemberData(nameof(VersionAndImageNameDataCliBuilderBullseye))]
        public void BuildsAppByInstallingSdkDynamicallyCliBuilderBullseye(string phpVersion, string imageName, string phpComposerVersion)
        {
            BuildsAppByInstallingSdkDynamically(phpVersion, imageName, phpComposerVersion, "/opt/php");
        }

        private void BuildsAppByInstallingSdkDynamically(
            string phpVersion, 
            string imageName, 
            string phpComposerVersion, 
            string installationRoot = BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot)
        {
            // Arrange
            var appName = "twig-example";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable("PHP_COMPOSER_VERSION", phpComposerVersion)
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --platform {PhpConstants.PlatformName} --platform-version {phpVersion}")
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
            RunAsserts(() =>
            {
                Assert.True(result.IsSuccess);
                Assert.Contains(
                    $"PHP executable: " + installationRoot, result.StdOut);
                Assert.Contains("Installing twig/twig", result.StdErr); // Composer prints its messages to STDERR
                Assert.Contains($"\'php-composer\' version \'{phpComposerVersion}\'", result.StdOut);
            },
            result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void BuildsApplication_ByDynamicallyInstalling_IntoCustomDynamicInstallationDir()
        {
            // Arrange
            var phpVersion = "7.3.15"; //NOTE: use the full version so that we know the install directory path
            var appName = "twig-example";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var expectedDynamicInstallRootDir = "/foo/bar";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --platform {PhpConstants.PlatformName} --platform-version {phpVersion} " +
                $"--dynamic-install-root-dir {expectedDynamicInstallRootDir}")
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
            RunAsserts(() =>
            {
                Assert.True(result.IsSuccess);
                Assert.Contains(
                    $"PHP executable: " +
                    expectedDynamicInstallRootDir, result.StdOut);
                Assert.Contains($"Installing twig/twig", result.StdErr); // Composer prints its messages to STDERR
            },
            result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void BuildPhpApp_AfterInstallingStretchSpecificSdk()
        {
            // Arrange
            var version = "7.0.33"; // version only exists on stretch
            var composerVersion = "1.10.0";

            var appName = "twig-example";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable("PHP_COMPOSER_VERSION", composerVersion)
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --platform {PhpConstants.PlatformName} --platform-version {version}")
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
            RunAsserts(() =>
            {
                Assert.True(result.IsSuccess);
                Assert.Contains(
                    $"PHP executable: " +
                    BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot, result.StdOut);
                Assert.Contains("Installing twig/twig", result.StdErr); // Composer prints its messages to STDERR
                Assert.Contains($"\'php-composer\' version \'{composerVersion}\'", result.StdOut);
            },
            result.GetDebugInfo());
        }

        [Theory, Trait("category", "githubactions")]
        [InlineData(ImageTestHelperConstants.GitHubActionsBuster)]
        [InlineData(ImageTestHelperConstants.GitHubActionsBullseye)]
        public void PhpFails_ToInstallStretchSdk_OnNonStretchImage(string imageTag)
        {
            // Arrange
            var version = "7.0.33"; // version only exists on stretch
            var composerVersion = "1.10.0";

            var appName = "twig-example";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable("PHP_COMPOSER_VERSION", composerVersion)
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --platform {PhpConstants.PlatformName} --platform-version {version}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(imageTag),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(() =>
            {
                Assert.False(result.IsSuccess);
                Assert.Contains($"Error: Platform '{PhpConstants.PlatformName}' version '{version}' is unsupported.", result.StdErr);
            },
            result.GetDebugInfo());
        }

        private DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "php", sampleAppName));
    }
}
