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
    public class PythonSampleAppsDjangoAppWithoutRunningCollectStatic : PythonSampleAppsTestBase
    {
        public PythonSampleAppsDjangoAppWithoutRunningCollectStatic(ITestOutputHelper output) : base(output)
        {
        }

        [Theory, Trait("category", "latest")]
        [InlineData("true")]
        [InlineData("True")]
        public void GeneratesScript_AndBuilds_DjangoApp_WithoutRunningCollectStatic_IfDisableCollectStatic_IsTrue(
            string disableCollectStatic)
        {
            // Arrange
            var appName = "django-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddCommand($"export {EnvironmentSettingsKeys.DisableCollectStatic}={disableCollectStatic}")
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --platform {PythonConstants.PlatformName}")
                // These css files should NOT be available since 'collectstatic' is set off
                .AddFileDoesNotExistCheck($"{appOutputDir}/staticfiles/css/boards.css")
                .AddFileDoesNotExistCheck($"{appOutputDir}/staticfiles/css/uservoice.css")
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
