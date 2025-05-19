// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator.Common;
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
                // Test default PHP composer version
                var data = new TheoryData<string, string, string>();
                data.Add(
                    PhpVersions.Php73Version, 
                    ImageTestHelper.WithRestrictedPermissions().GetGitHubActionsBuildImage(), 
                    PhpVersions.ComposerDefaultVersion
                );
                var imageHelper = new ImageTestHelper();
                data.Add(PhpVersions.Php74Version, imageHelper.GetGitHubActionsBuildImage(), PhpVersions.ComposerDefaultVersion);
                data.Add(PhpVersions.Php74Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.ComposerDefaultVersion);
                data.Add(PhpVersions.Php80Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBuster), PhpVersions.ComposerDefaultVersion);
                data.Add(PhpVersions.Php81Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.ComposerDefaultVersion);
                data.Add(PhpVersions.Php82Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.ComposerDefaultVersion);
                data.Add(PhpVersions.Php83Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.ComposerDefaultVersion);
                data.Add(PhpVersions.Php83Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBookworm), PhpVersions.ComposerDefaultVersion);
                data.Add(PhpVersions.Php84Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.ComposerDefaultVersion);
                data.Add(PhpVersions.Php84Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBookworm), PhpVersions.ComposerDefaultVersion);

                // // Test PHP composer version 2.2.x
                data.Add(
                    PhpVersions.Php73Version,
                    ImageTestHelper.WithRestrictedPermissions().GetGitHubActionsBuildImage(),
                    PhpVersions.Composer22Version
                );
                data.Add(PhpVersions.Php74Version, imageHelper.GetGitHubActionsBuildImage(), PhpVersions.Composer22Version);
                data.Add(PhpVersions.Php80Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBuster), PhpVersions.Composer22Version);
                data.Add(PhpVersions.Php81Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer22Version);
                data.Add(PhpVersions.Php82Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer22Version);
                data.Add(PhpVersions.Php83Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer22Version);
                data.Add(PhpVersions.Php84Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer22Version);

                // // Test PHP composer version 2.3.x
                data.Add(
                    PhpVersions.Php73Version,
                    ImageTestHelper.WithRestrictedPermissions().GetGitHubActionsBuildImage(),
                    PhpVersions.Composer23Version
                );
                data.Add(PhpVersions.Php74Version, imageHelper.GetGitHubActionsBuildImage(), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php80Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBuster), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php81Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php82Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php83Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php84Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer23Version);

                // Test PHP composer version 2.4.x
                data.Add(
                    PhpVersions.Php73Version,
                    ImageTestHelper.WithRestrictedPermissions().GetGitHubActionsBuildImage(),
                    PhpVersions.Composer24Version
                );
                data.Add(PhpVersions.Php74Version, imageHelper.GetGitHubActionsBuildImage(), PhpVersions.Composer24Version);
                data.Add(PhpVersions.Php80Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBuster), PhpVersions.Composer24Version);
                data.Add(PhpVersions.Php81Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer24Version);
                data.Add(PhpVersions.Php82Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer24Version);
                data.Add(PhpVersions.Php83Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer24Version);
                data.Add(PhpVersions.Php84Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer24Version);

                // Test PHP composer version 2.5.x
                data.Add(
                    PhpVersions.Php73Version,
                    ImageTestHelper.WithRestrictedPermissions().GetGitHubActionsBuildImage(),
                    PhpVersions.Composer25Version
                );
                data.Add(PhpVersions.Php74Version, imageHelper.GetGitHubActionsBuildImage(), PhpVersions.Composer25Version);
                data.Add(PhpVersions.Php80Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBuster), PhpVersions.Composer25Version);
                data.Add(PhpVersions.Php81Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer25Version);
                data.Add(PhpVersions.Php82Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer25Version);
                data.Add(PhpVersions.Php83Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer25Version);
                data.Add(PhpVersions.Php84Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer25Version);                

                // Test PHP composer version 2.6.x
                data.Add(
                    PhpVersions.Php73Version,
                    ImageTestHelper.WithRestrictedPermissions().GetGitHubActionsBuildImage(),
                    PhpVersions.Composer26Version
                );
                data.Add(PhpVersions.Php74Version, imageHelper.GetGitHubActionsBuildImage(), PhpVersions.Composer26Version);
                data.Add(PhpVersions.Php80Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBuster), PhpVersions.Composer26Version);
                data.Add(PhpVersions.Php81Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer26Version);
                data.Add(PhpVersions.Php82Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer26Version);
                data.Add(PhpVersions.Php83Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer26Version);
                data.Add(PhpVersions.Php83Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBookworm), PhpVersions.Composer26Version);
                data.Add(PhpVersions.Php84Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer26Version);
                data.Add(PhpVersions.Php84Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBookworm), PhpVersions.Composer26Version);
                
                // Test PHP composer version 2.7.x
                data.Add(PhpVersions.Php81Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer27Version);
                data.Add(PhpVersions.Php82Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer27Version);
                data.Add(PhpVersions.Php83Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer27Version);
                data.Add(PhpVersions.Php83Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBookworm), PhpVersions.Composer27Version);
                data.Add(PhpVersions.Php84Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer27Version);
                data.Add(PhpVersions.Php84Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBookworm), PhpVersions.Composer27Version);

                // Test PHP composer version 2.8.x
                data.Add(PhpVersions.Php81Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer28Version);
                data.Add(PhpVersions.Php82Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer28Version);
                data.Add(PhpVersions.Php83Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer28Version);
                data.Add(PhpVersions.Php83Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBookworm), PhpVersions.Composer28Version);
                data.Add(PhpVersions.Php84Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye), PhpVersions.Composer28Version);
                data.Add(PhpVersions.Php84Version, imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBookworm), PhpVersions.Composer28Version);

                return data;
            }
        }

        [Theory, Trait("category", "githubactions")]
        [MemberData(nameof(VersionAndImageNameData))]
        public void BuildsAppByInstallingSdkDynamicallyGithubActions(string phpVersion, string imageName, string phpComposerVersion)
        {
            BuildsAppByInstallingSdkDynamically(phpVersion, imageName, phpComposerVersion);
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
            var phpVersion = "7.3.21"; //NOTE: use the full version so that we know the install directory path
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

        // [Fact, Trait("category", "githubactions")]
        // public void BuildPhpApp_AfterInstallingStretchSpecificSdk()
        // {
        //     // Arrange
        //     var version = "7.0.33"; // version only exists on stretch
        //     var composerVersion = "1.10.0";

        //     var appName = "twig-example";
        //     var volume = CreateSampleAppVolume(appName);
        //     var appDir = volume.ContainerDir;
        //     var appOutputDir = "/tmp/app-output";
        //     var script = new ShellScriptBuilder()
        //         .SetEnvironmentVariable("PHP_COMPOSER_VERSION", composerVersion)
        //         .AddBuildCommand(
        //         $"{appDir} -o {appOutputDir} --platform {PhpConstants.PlatformName} --platform-version {version}")
        //         .ToString();

        //     // Act
        //     var result = _dockerCli.Run(new DockerRunArguments
        //     {
        //         ImageId = _imageHelper.GetGitHubActionsBuildImage(),
        //         EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
        //         Volumes = new List<DockerVolume> { volume },
        //         CommandToExecuteOnRun = "/bin/bash",
        //         CommandArguments = new[] { "-c", script }
        //     });

        //     // Assert
        //     RunAsserts(() =>
        //     {
        //         Assert.True(result.IsSuccess);
        //         Assert.Contains(
        //             $"PHP executable: " +
        //             BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot, result.StdOut);
        //         Assert.Contains("Installing twig/twig", result.StdErr); // Composer prints its messages to STDERR
        //         Assert.Contains($"\'php-composer\' version \'{composerVersion}\'", result.StdOut);
        //     },
        //     result.GetDebugInfo());
        // }

        // [Theory, Trait("category", "githubactions")]
        // [InlineData(ImageTestHelperConstants.GitHubActionsBuster)]
        // [InlineData(ImageTestHelperConstants.GitHubActionsBullseye)]
        // public void PhpFails_ToInstallStretchSdk_OnNonStretchImage(string imageTag)
        // {
        //     // Arrange
        //     var version = "7.0.33"; // version only exists on stretch
        //     var composerVersion = "1.10.0";

        //     var appName = "twig-example";
        //     var volume = CreateSampleAppVolume(appName);
        //     var appDir = volume.ContainerDir;
        //     var appOutputDir = "/tmp/app-output";
        //     var script = new ShellScriptBuilder()
        //         .SetEnvironmentVariable("PHP_COMPOSER_VERSION", composerVersion)
        //         .AddBuildCommand(
        //         $"{appDir} -o {appOutputDir} --platform {PhpConstants.PlatformName} --platform-version {version}")
        //         .ToString();

        //     // Act
        //     var result = _dockerCli.Run(new DockerRunArguments
        //     {
        //         ImageId = _imageHelper.GetGitHubActionsBuildImage(imageTag),
        //         EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
        //         Volumes = new List<DockerVolume> { volume },
        //         CommandToExecuteOnRun = "/bin/bash",
        //         CommandArguments = new[] { "-c", script }
        //     });

        //     // Assert
        //     RunAsserts(() =>
        //     {
        //         Assert.False(result.IsSuccess);
        //         Assert.Contains($"Error: Platform '{PhpConstants.PlatformName}' version '{version}' is unsupported.", result.StdErr);
        //     },
        //     result.GetDebugInfo());
        // }

        private DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "php", sampleAppName));
    }
}
