﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "dotnetcore-50")]
    public class DotNetCoreRuntimeVersion50Tests : DotNetCoreEndToEndTestsBase
    {
        public DotNetCoreRuntimeVersion50Tests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanBuildAndRun_Without_Oryx_AppInsights_Codeless_Configuration()
        {
            // Arrange
            var dotnetcoreVersion = DotNetCoreRunTimeVersions.NetCoreApp50;
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", Net5MvcApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var agentExtensionVersionEnv = ExtVarNames.UserAppInsightsAgentExtensionVersion;
            var connectionStringEnv = ExtVarNames.UserAppInsightsConnectionStringEnv;
            var OryxAppInsightsAttachString1 = "export ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=";
            var OryxAppInsightsAttachString2 = "export DOTNET_STARTUP_HOOKS=";

            var buildImageScript = new ShellScriptBuilder()
               .AddDefaultTestEnvironmentVariables()
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int --platform {DotNetCoreConstants.PlatformName} " +
                $"--platform-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand($"export {agentExtensionVersionEnv}=~3")
                .AddCommand($"export {connectionStringEnv}=alkajsldkajd")
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddStringDoesNotExistInFileCheck(OryxAppInsightsAttachString1, $"run.sh")
                .AddStringDoesNotExistInFileCheck(OryxAppInsightsAttachString2, $"run.sh")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                Net5MvcApp,
                _output,
                new DockerVolume[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", "5.0"),
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Welcome to ASP.NET Core MVC!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore50MvcApp()
        {
            // Arrange
            var dotnetcoreVersion = DotNetCoreRunTimeVersions.NetCoreApp50;
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", Net5MvcApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
               .AddDefaultTestEnvironmentVariables()
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int --platform {DotNetCoreConstants.PlatformName} " +
                $"--platform-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                Net5MvcApp,
                _output,
                new DockerVolume[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", "5.0"),
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Welcome to ASP.NET Core MVC!", data);
                });
        }
    
        [Fact(Skip = "Skipping test temporarily")]
        public async Task CanRun_SelfContainedApp_TargetedForLinux()
        {
            // ****************************************************************
            // A self-contained app is an app which does not depend on whether a .NET Core runtime is present on the
            // target machine or not. It has all the required dependencies (.dlls etc.) in its published output itself,
            // hence it is called self-contained app.
            //
            // To test if our runtime images run self-contained apps correctly (i.e not using the dotnet runtime
            // installed in the runtime image itself), in this test we publish a self-contained 3.0 app and run it in
            // a 1.1 runtime container. This is because 1.1 runtime container does not have 3.0 bits at all and hence
            // if the app fails to run in that container, then we are doing something wrong. If all is well, this 3.0
            // app should run fine.
            // ****************************************************************

            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", Net5MvcApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
               .AddDefaultTestEnvironmentVariables()
               .SetEnvironmentVariable(
                    SettingsKeys.DynamicInstallRootDir,
                    BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot)
               .AddCommand($"oryx prep --skip-detection --platforms-and-versions dotnet=5.0.0")
               .Source($"benv dotnet=5.0.100")
               .AddCommand($"cd {appDir}")
               .AddCommand($"dotnet publish -c release -r linux-x64 -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                Net5MvcApp,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                // NOTE: it is 1.1 version on purpose. Read comments at the beginning of this method for more details
                _imageHelper.GetRuntimeImage("dotnetcore", "1.1"),
                ContainerPort,
                "/bin/sh",
                new[]
                {
                    "-c",
                    runtimeImageScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Welcome to ASP.NET Core MVC!", data);
                });
        }
    }
}