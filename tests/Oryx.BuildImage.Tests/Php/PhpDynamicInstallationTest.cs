// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildImage.Tests;
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
                // test default php-composer version
                var data = new TheoryData<string, string, string>();
                data.Add(
                    PhpVersions.Php73Version, 
                    ImageTestHelper.WithRestrictedPermissions().GetGitHubActionsBuildImage(), 
                    PhpVersions.ComposerVersion
                );
                var imageHelper = new ImageTestHelper();
                data.Add(PhpVersions.Php74Version, imageHelper.GetGitHubActionsBuildImage(), PhpVersions.ComposerVersion);
                data.Add(PhpVersions.Php80Version, imageHelper.GetGitHubActionsBuildImage("github-actions-buster"), PhpVersions.ComposerVersion);
                data.Add("8.1.4", imageHelper.GetGitHubActionsBuildImage("github-actions-buster"), PhpVersions.ComposerVersion);
                data.Add("8.0.17", imageHelper.GetGitHubActionsBuildImage("github-actions-buster"), PhpVersions.ComposerVersion);

                // test latest php-composer version
                data.Add(
                    PhpVersions.Php73Version,
                    ImageTestHelper.WithRestrictedPermissions().GetGitHubActionsBuildImage(),
                    PhpVersions.Composer23Version
                );
                data.Add(PhpVersions.Php74Version, imageHelper.GetGitHubActionsBuildImage(), PhpVersions.Composer23Version);
                data.Add(PhpVersions.Php80Version, imageHelper.GetGitHubActionsBuildImage("github-actions-buster"), PhpVersions.Composer23Version);
                data.Add("8.1.4", imageHelper.GetGitHubActionsBuildImage("github-actions-buster"), PhpVersions.Composer23Version);
                return data;
            }
        }

        [Theory]
        [MemberData(nameof(VersionAndImageNameData))]
        public void BuildsAppByInstallingSdkDynamically(string phpVersion, string imageName, string phpComposerVersion)
        {
            // Arrange
            var appName = "twig-example";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
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
                    $"PHP executable: " +
                    BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot, result.StdOut);
                Assert.Contains("Installing twig/twig", result.StdErr); // Composer prints its messages to STDERR
                Assert.Contains($"\'php-composer\' version \'{phpComposerVersion}\'", result.StdOut);
            },
            result.GetDebugInfo());
        }

        [Fact]
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
                .AddDefaultTestEnvironmentVariables()
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

        private DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "php", sampleAppName));
    }
}
