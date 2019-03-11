// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System;
using System.IO;
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
            DockerVolume.Create(Path.Combine(_hostSamplesDir, "php", sampleAppName));

        [Fact]
        public void GeneratesScript_AndBuilds_TwigExample()
        {
            // Arrange
            var appName = "twig-example";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(Settings.BuildImageName, CreateAppNameEnvVar(appName), volume, "/bin/bash", new[] { "-c", script });

            // Assert
            RunAsserts(() =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains($"PHP executable: /opt/php/{PhpVersions.Php73Version}/bin/php", result.Output);
                    Assert.Contains($"Installing twig/twig", result.Error); // Composer prints its messages to STDERR
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_AndBuilds_WithoutComposerFile()
        {
            // Arrange
            var appName = "twig-example";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddCommand($"rm {appDir}/composer.json")
                .AddBuildCommand($"{appOutputDir} --language php --language-version {PhpVersions.Php73Version}")
                .ToString();

            // Act
            var result = _dockerCli.Run(Settings.BuildImageName, CreateAppNameEnvVar(appName), volume, "/bin/bash", new[] { "-c", script });

            // Assert
            RunAsserts(() =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains($"PHP executable: /opt/php/{PhpVersions.Php73Version}/bin/php", result.Output);
                    Assert.Contains($"not running composer install", result.Output);
                },
                result.GetDebugInfo());
        }
    }
}
