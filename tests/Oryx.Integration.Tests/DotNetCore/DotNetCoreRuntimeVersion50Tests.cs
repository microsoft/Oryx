// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "dotnetcore-5.0")]
    public class DotNetCoreRuntimeVersion50Tests : DotNetCoreEndToEndTestsBase
    {
        public DotNetCoreRuntimeVersion50Tests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        [Trait("build-image", "github-actions-debian-stretch")]
        public async Task CanBuildAndRun_Without_Oryx_AppInsights_Codeless_ConfigurationAsync()
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
                _imageHelper.GetRuntimeImage("dotnetcore", "5.0", ImageTestHelperConstants.OsTypeDebianBuster),
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
        [Trait("build-image", "github-actions-debian-stretch")]
        public async Task CanBuildAndRun_NetCore50MvcAppAsync()
        {
            // Arrange
            var dotnetcoreVersion = DotNetCoreRunTimeVersions.NetCoreApp50;
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", Net5MvcApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
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
                _imageHelper.GetRuntimeImage("dotnetcore", "5.0", ImageTestHelperConstants.OsTypeDebianBuster),
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