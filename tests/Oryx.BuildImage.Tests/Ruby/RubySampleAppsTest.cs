// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Ruby;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    [Trait("platform", "ruby")]
    public class RubySampleAppsTest : SampleAppsTestBase
    {
        public RubySampleAppsTest(ITestOutputHelper output) : base(output)
        {
        }
        private DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "ruby", sampleAppName));

        [Fact, Trait("category", "vso-focal")]
        public void PipelineTestInvocationVsoFocal()
        {
            var imageTestHelper = new ImageTestHelper();
            GeneratesScript_AndBuildRailsApp(imageTestHelper.GetVsoBuildImage(ImageTestHelperConstants.VsoFocal));
        }

        [Fact, Trait("category", "jamstack")]
        public void PipelineTestInvocationJamstack()
        {
            var imageTestHelper = new ImageTestHelper();
            Builds_JekyllStaticWebApp_UsingCustomBuildCommand(
                imageTestHelper.GetAzureFunctionsJamStackBuildImage());
        }

        [Fact, Trait("category", "vso-focal")]
        public void GeneratesScript_AndBuildSinatraApp()
        {
            // Arrange
            var appName = "sinatra-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetVsoBuildImage(ImageTestHelperConstants.VsoFocal),
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
                    Assert.Contains("Ruby version", result.StdOut);
                },
                result.GetDebugInfo());
        }

        private void GeneratesScript_AndBuildRailsApp(string imageName)
        {
            // Arrange
            var appName = "ruby-on-rails-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
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
                    Assert.Contains("Ruby version", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "vso-focal")]
        public void Builds_JekyllStaticWebApp_When_Apptype_Is_SetAs_StaticSiteApplications()
        {
            // Arrange
            var appName = "Jekyll-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --apptype {Constants.StaticSiteApplications} ")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.OsTypeFileName}")
                .AddStringExistsInFileCheck(
                $"{ManifestFilePropertyKeys.PlatformName}=\"{RubyConstants.PlatformName}\"",
                $"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{RubyConstants.DefaultAppLocationDirName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetVsoBuildImage(ImageTestHelperConstants.VsoFocal),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });
        }

        private void Builds_JekyllStaticWebApp_UsingCustomBuildCommand(string buildImage)
        {
            // Please note:
            // This test method has at least 1 wrapper function that pases the imageName parameter.

            // Arrange
            var appName = "Jekyll-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable("CUSTOM_BUILD_COMMAND", "touch example.txt")
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --apptype {Constants.StaticSiteApplications} ")
                .AddFileExistsCheck($"{appOutputDir}/example.txt")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.OsTypeFileName}")
                .AddStringExistsInFileCheck(
                $"{ManifestFilePropertyKeys.PlatformName}=\"{RubyConstants.PlatformName}\"",
                $"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = buildImage,
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });
        }
    }
} 