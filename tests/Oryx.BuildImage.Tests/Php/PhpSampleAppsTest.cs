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

        public static IEnumerable<object[]> TwigExampleDynamicInstallationTestData => new[]
        {
            new object[] { PhpVersions.Php81Version },
            new object[] { PhpVersions.Php82Version },
            new object[] { PhpVersions.Php83Version },
            new object[] { PhpVersions.Php84Version },
        };

        [Theory, Trait("category", "githubactions")]
        [MemberData(nameof(TwigExampleDynamicInstallationTestData))]
        public void GeneratesScript_AndBuilds_TwigExample_WithDynamicInstallation(string phpVersion)
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
                    Assert.Contains($"PHP executable: /tmp/oryx/platforms/php/{phpVersion}/bin/php", result.StdOut);
                    Assert.Contains($"Installing twig/twig", result.StdErr); // Composer prints its messages to STDERR
                },
                result.GetDebugInfo());
        }

        public static IEnumerable<object[]> WithoutComposerFileTestData => new[]
        {
            new object[] { PhpVersions.Php74Version },
            new object[] { PhpVersions.Php80Version },
            new object[] { PhpVersions.Php82Version },
        };

        [Theory, Trait("category", "githubactions")]
        [MemberData(nameof(WithoutComposerFileTestData))]
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
