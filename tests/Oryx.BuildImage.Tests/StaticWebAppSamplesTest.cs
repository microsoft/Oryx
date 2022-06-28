// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class StaticWebAppSamplesTest : SampleAppsTestBase
    {
        public StaticWebAppSamplesTest(ITestOutputHelper output, DockerCli dockerCli = null)
            : base(output, dockerCli)
        {
        }

        [Fact, Trait("category", "jamstack")]
        public void BuildsHugoClientAndDotNetCoreServerApp_WithRecurisveLookupDisabled()
        {
            // NOTE: This test simulates a typical Azure Static Web Apps repo where the root folder is a static app
            // where as the "api" folder has the server side app.

            // Arrange
            var appName = "HugoClientDotNetCoreServer";
            var volume = DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "StaticWebAppsSamples", appName));
            var rootDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-location-output";
            var appLocationBuildScript = new ShellScriptBuilder()
                .AddCommand(
                    $"oryx build {rootDir} -i /tmp/app-location -o {appOutputDir} " +
                    $"-p {SettingsKeys.DisableRecursiveLookUp}=true")
                .AddFileExistsCheck($"{appOutputDir}/public/index.xml")
                .ToString();

            // Act1
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetAzureFunctionsJamStackBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", appLocationBuildScript }
            });

            // Assert1
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());

            // Arrange2
            appOutputDir = "/tmp/api-location-output";
            var apiLocationBuildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {rootDir}/api -i /tmp/api-location -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/bin/azureFunctionsApps.dll")
                .ToString();

            // Act2
            result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetAzureFunctionsJamStackBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", apiLocationBuildScript }
            });

            // Assert2
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }
    }
}
