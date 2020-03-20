// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class BuildCommandOptionsTest : SampleAppsTestBase
    {
        public DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "python", sampleAppName));

        public BuildCommandOptionsTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void BuildsApp_UsingValue_FromBuildEnvFile()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            // create a build.env file
            File.WriteAllText(
                Path.Combine(volume.MountedHostDir, BuildScriptGeneratorCli.Constants.BuildEnvironmentFileName),
                $"{SettingsKeys.PythonVersion}=3.6");
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetTestBuildImage(),
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
                    Assert.Contains(
                        $"Python Version: /opt/python/{PythonVersions.Python36Version}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void BuildsApp_UsingValue_FromEnvironmentVariable_OverValueFromEnvFile()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            // create a build.env file
            File.WriteAllText(
                Path.Combine(volume.MountedHostDir, BuildScriptGeneratorCli.Constants.BuildEnvironmentFileName),
                $"{SettingsKeys.PythonVersion}=100.100.100");
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(SettingsKeys.PythonVersion, "2.7")
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetTestBuildImage(),
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
                    Assert.Contains(
                        $"Python Version: /opt/python/{PythonVersions.Python27Version}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void BuildsApp_UsingValue_FromCommandLine_OverValueFromEnvironmentVariables_OrEnvFile()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            // create a build.env file
            File.WriteAllText(
                Path.Combine(volume.MountedHostDir, BuildScriptGeneratorCli.Constants.BuildEnvironmentFileName),
                $"{SettingsKeys.PythonVersion}=100.100.100");
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(SettingsKeys.PythonVersion, "50.50.50")
                .AddBuildCommand($"{appDir} -o {appOutputDir} --platform python --platform-version 3.7")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetTestBuildImage(),
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
                    Assert.Contains(
                        $"Python Version: /opt/python/{PythonVersions.Python37Version}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }
    }
}
