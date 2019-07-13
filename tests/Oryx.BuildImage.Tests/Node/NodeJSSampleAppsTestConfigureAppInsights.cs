// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class NodeJSSampleAppsTestConfigureAppInsights : NodeJSSampleAppsTestBase
    {
        public NodeJSSampleAppsTestConfigureAppInsights(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [MemberData(nameof(TestValueGenerator.GetNodeVersions_SupportDebugging),
            MemberType = typeof(TestValueGenerator))]
        public void BuildNodeApp_ConfigureAppInsights_WithCorrectNodeVersion_AIEnvironmentVariableSet(string version)
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var spcifyNodeVersionCommand = "--platform nodejs --platform-version=" + version;
            var nestedOutputDir = "/tmp/output";
            var script = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -o {nestedOutputDir} {spcifyNodeVersionCommand} --log-file {appDir}/1.log")
                .AddDirectoryExistsCheck($"{nestedOutputDir}/node_modules")
                .AddFileExistsCheck($"{nestedOutputDir}/oryx-appinsightsloader.js")
                .AddFileExistsCheck($"{nestedOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddStringExistsInFileCheck(
                $"{NodeConstants.InjectedAppInsights}=\"True\"",
                $"{nestedOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                EnvironmentVariables = new List<EnvironmentVariable>
                {
                    new EnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", "xyz")
                },
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