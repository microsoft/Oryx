// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class PythonSampleAppsBuildsJamspell : PythonSampleAppsTestBase
    {
        public PythonSampleAppsBuildsJamspell(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [MemberData(nameof(TestValueGenerator.GetPythonVersions), MemberType = typeof(TestValueGenerator))]
        public void GeneratesScript_AndBuilds_Jamspell_With_Python(string version)
        {
            // Arrange
            var appName = "jamspell-flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddCommand($"tar -xvzf {appDir}/en.tar.gz")
                .AddBuildCommand($"{appDir} -o {appOutputDir} --platform python --platform-version {version}")
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
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }
    }
}