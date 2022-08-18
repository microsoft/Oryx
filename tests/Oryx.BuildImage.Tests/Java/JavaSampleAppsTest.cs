﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    [Trait("platform", "java")]
    public class JavaSampleAppsTest : SampleAppsTestBase
    {
        public JavaSampleAppsTest(ITestOutputHelper output) : base(output)
        {
        }

        private DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "java", sampleAppName));

        [Fact, Trait("category", "githubactions")]
        public void JavaSampleAppsTestsGithubActions()
        {
            var imageTag = "github-actions";
            BuildsMavenArcheTypeSample(imageTag);
            BuildsMavenJ2EESample(imageTag);
            BuildsMavenSimpleJavaApp(imageTag);
            BuildsSpringBootSampleApp(imageTag);
        }


        [Theory, Trait("category", "cli")]
        [InlineData("cli")]
        [InlineData("cli-buster")]
        public void JavaSampleAppsTestsCli(string imageTag)
        {
            BuildsMavenArcheTypeSample(imageTag);
            BuildsMavenJ2EESample(imageTag);
            BuildsMavenSimpleJavaApp(imageTag);
            BuildsSpringBootSampleApp(imageTag);
        }

        private void BuildsMavenArcheTypeSample(string imageTag)
        {
            // Arrange
            var appName = "MavenArcheType";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/target/classes/microsoft/oryx/App.class")
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
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        private void BuildsMavenJ2EESample(string imageTag)
        {
            // Arrange
            var appName = "MavenJ2EESample";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/ejbs/target/classes/META-INF/ejb-jar.xml")
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
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        private void BuildsMavenSimpleJavaApp(string imageTag)
        {
            // Arrange
            var appName = "MavenSimpleJavaApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/target/classes/microsoft/oryx/App.class")
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
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        private void BuildsSpringBootSampleApp(string imageTag)
        {
            // Arrange
            var appName = "SprintBootSample";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/target/classes/com/example/demo/DemoApplication.class")
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
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }
    }
}