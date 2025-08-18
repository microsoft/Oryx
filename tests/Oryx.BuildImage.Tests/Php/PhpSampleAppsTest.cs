// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class PhpSampleAppsTest : SampleAppsTestBase
    {
        public PhpSampleAppsTest(ITestOutputHelper output) : base(output)
        {
        }

        private DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "php", sampleAppName));

        [Fact, Trait("category", "githubactions")]
        public void GeneratesScript_AndBuilds_TwigExample_InGitHubActionsBuildImage()
        {
            // Arrange
            var phpVersion = PhpVersions.Php74Version;
            var appName = "twig-example";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --platform {PhpConstants.PlatformName} --platform-version {phpVersion}")
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
                Assert.Contains($"PHP executable: /opt/php/{phpVersion}/bin/php", result.StdOut);
                Assert.Contains($"Installing twig/twig", result.StdErr); // Composer prints its messages to STDERR
            },
            result.GetDebugInfo());
        }

        [Theory, Trait("category", "githubactions")]
        [InlineData(PhpVersions.Php74Version)]
        [InlineData(PhpVersions.Php80Version)]
        [InlineData(PhpVersions.Php82Version)]
        public void GeneratesScript_AndBuilds_TwigExample(string phpVersion)
        {
            // Arrange
            var appName = "twig-example";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir} --platform {PhpConstants.PlatformName} --platform-version {phpVersion}")
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
                    Assert.Contains($"PHP executable: /opt/php/{phpVersion}/bin/php", result.StdOut);
                    Assert.Contains($"Installing twig/twig", result.StdErr); // Composer prints its messages to STDERR
                },
                result.GetDebugInfo());
        }

        [Theory, Trait("category", "githubactions")]
        [InlineData(PhpVersions.Php80Version, ImageTestHelperConstants.GitHubActionsBullseye)]
        public void GeneratesScript_AndBuilds_TwigExample_WithDynamicInstallation_GithubActions(string phpVersion, string imageTag)
        {
            GeneratesScript_AndBuilds_TwigExample_WithDynamicInstallation(phpVersion, imageTag);
        }

        private void GeneratesScript_AndBuilds_TwigExample_WithDynamicInstallation(string phpVersion, string imageTag)
        {
            // Arrange
            var appName = "twig-example";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir} --platform {PhpConstants.PlatformName} --platform-version {phpVersion}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetBuildImage(imageTag),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(() =>
            {
                Assert.True(result.IsSuccess);
                Assert.Contains($"PHP executable: /opt/php/{phpVersion}/bin/php", result.StdOut);
                Assert.Contains($"Installing twig/twig", result.StdErr); // Composer prints its messages to STDERR
            },
                result.GetDebugInfo());
        }

        [Theory, Trait("category", "githubactions")]
        [InlineData(PhpVersions.Php74Version)]
        [InlineData(PhpVersions.Php80Version)]
        [InlineData(PhpVersions.Php82Version)]
        public void GeneratesScript_AndBuilds_WithoutComposerFile(string phpVersion)
        {
            // Arrange
            var appName = "twig-example";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var manifestFile = $"{appOutputDir}/{FilePaths.BuildManifestFileName}";
            var osTypeFile = $"{appOutputDir}/{FilePaths.OsTypeFileName}";
            var script = new ShellScriptBuilder()
                .AddCommand($"rm {appDir}/composer.json")
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --platform {PhpConstants.PlatformName} --platform-version {phpVersion}")
                .AddFileExistsCheck(osTypeFile)
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
            RunAsserts(() =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains($"not running 'composer install'", result.StdOut);
                    Assert.Contains(
                       $"{ManifestFilePropertyKeys.PhpVersion}=\"{phpVersion}\"",
                       result.StdOut);
                },
                result.GetDebugInfo());
        }
    }
}
