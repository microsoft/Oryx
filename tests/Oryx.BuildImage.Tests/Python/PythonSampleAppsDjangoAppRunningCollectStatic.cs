// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class PythonSampleAppsDjangoAppRunningCollectStatic : PythonSampleAppsTestBase
    {
        public PythonSampleAppsDjangoAppRunningCollectStatic(ITestOutputHelper output) : base(output)
        {
        }

        [Theory, Trait("category", "latest")]
        [InlineData(null)]
        [InlineData("false")]
        public void GeneratesScript_AndBuilds_DjangoApp_RunningCollectStatic(string disableCollectStatic)
        {
            // Arrange
            var appName = "django-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var scriptBuilder = new ShellScriptBuilder();
            if (string.IsNullOrEmpty(disableCollectStatic))
            {
                scriptBuilder.AddCommand(
                    $"export {EnvironmentSettingsKeys.DisableCollectStatic}={disableCollectStatic}");
            }
            var script = scriptBuilder
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --platform {PythonConstants.PlatformName}")
                // These css files should be available since 'collectstatic' is run in the script
                .AddFileExistsCheck($"{appOutputDir}/staticfiles/css/boards.css")
                .AddFileExistsCheck($"{appOutputDir}/staticfiles/css/uservoice.css")
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
