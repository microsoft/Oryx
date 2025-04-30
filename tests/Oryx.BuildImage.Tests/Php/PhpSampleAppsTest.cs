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

        [Fact, Trait("category", "ltsversions")]
        public void GeneratesScript_AndBuilds_TwigExample_InLtsVersionsBuildImage()
        {
            // Arrange
            var phpVersion = PhpVersions.Php73Version;
            var buildImageName = _imageHelper.GetLtsVersionsBuildImage();
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
                ImageId = buildImageName,
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

        [Theory, Trait("category", "latest")]
        [InlineData(PhpVersions.Php74Version)]
        [InlineData(PhpVersions.Php73Version)]
        [InlineData(PhpVersions.Php72Version)]
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
                ImageId = Settings.BuildImageName,
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

        [Theory, Trait("category", "ltsversions")]
        [InlineData(PhpVersions.Php74Version)]
        [InlineData(PhpVersions.Php73Version)]
        [InlineData(PhpVersions.Php72Version)]
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
                ImageId = Settings.LtsVersionsBuildImageName,
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

        [Theory, Trait("category", "ltsversions")]
        [InlineData(PhpVersions.Php80Version)]
        public void GeneratesScript_AndBuilds_TwigExample_InLtsVersionsBusterImage(string phpVersion)
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
                ImageId = _imageHelper.GetBuildImage(ImageTestHelperConstants.LtsVersionsBuster),
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
    }
}
