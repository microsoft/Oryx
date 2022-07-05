// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Tests.Common;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class NodeJSSampleAppsTestBase : SampleAppsTestBase
    {
        public static readonly string SampleAppName = "webfrontend";

        public DockerVolume CreateWebFrontEndVolume() => DockerVolume.CreateMirror(
            Path.Combine(_hostSamplesDir, "nodejs", SampleAppName));

        public NodeJSSampleAppsTestBase(ITestOutputHelper output) :
            base(output, new DockerCli(new EnvironmentVariable[]
            {
                new EnvironmentVariable(ExtVarNames.AppServiceAppNameEnvVarName, SampleAppName)
            }))
        {
        }

        [Fact]
        public void GeneratesScript_AndLoggerFormatCheck()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/" + SampleAppName + "-output";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddCommand($"echo RandomText >> {appDir}/Program.cs") // triggers a failure
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --package --property package_directory='oryxteststring'")
                .ToString();
            // Regex will match:
            // "yyyy-mm-dd hh:mm:ss"|WARNING|.
            Regex regex = new Regex(@"""[0-9]{4}-(0[1-9]|1[0-2])-(0[1-9]|[1-2][0-9]|3[0-1]) (0[0-9]|1[0-9]|2[0-3]):([0-5][0-9]):([0-5][0-9])""\|WARNING\|.*");

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.LtsVersionsBuildImageName,
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(SampleAppName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.False(result.IsSuccess);
                    Match match = regex.Match(result.StdOut);
                    Assert.True(match.Success);
                },
                result.GetDebugInfo());
        }
    }
}