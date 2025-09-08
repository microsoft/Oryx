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
    [Trait("category", "dotnetcore-10.0")]
    public class DotNetCoreRuntimeVersion100Tests : DotNetCoreEndToEndTestsBase
    {
        public DotNetCoreRuntimeVersion100Tests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Theory]
        [Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-ubuntu-noble")]
        [InlineData(NetCoreApp100MvcApp, "Welcome to ASP.NET Core 10 MVC!")]
        [InlineData(NetCoreApp100WebApp, "Welcome to a .NET 10 Web App!")]
        public async Task CanBuildAndRun_NetCore100AppAsync(string sampleApp, string webpageMessage)
        {
            // Arrange
            var dotnetcoreVersion = DotNetCoreRunTimeVersions.NetCoreApp100;
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", sampleApp);
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
                sampleApp,
                _output,
                new DockerVolume[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsNoble),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", "10.0", ImageTestHelperConstants.OsTypeUbuntuNoble),
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
                    Assert.Contains(webpageMessage, data);
                });
        }

        [Theory]
        [Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-ubuntu-noble")]
        [InlineData(NetCoreApp100MvcApp)]
        [InlineData(NetCoreApp100WebApp)]
        public async Task CanBuildAndRun_NetCore100AppWithExplicitPackageReferences_Async(string sampleApp)
        {
            // Arrange
            var dotnetcoreVersion = DotNetCoreRunTimeVersions.NetCoreApp100;
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", sampleApp);
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
                sampleApp,
                _output,
                new DockerVolume[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsNoble),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", "10.0", ImageTestHelperConstants.OsTypeUbuntuNoble),
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
                    Assert.DoesNotContain("Exception", data);
                });
        }

        [Theory]
        [Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-ubuntu-noble")]
        [InlineData(NetCoreApp100MvcApp)]
        public async Task CanBuildAndRun_NetCore100App_WhenUsingExplicitStartupCommand_Async(string sampleApp)
        {
            // Arrange
            var dotnetcoreVersion = DotNetCoreRunTimeVersions.NetCoreApp100;
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", sampleApp);
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
                .AddCommand($"cd {appOutputDir}")
                .AddCommand($"dotnet {sampleApp}.dll --urls http://0.0.0.0:{ContainerPort}")
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                sampleApp,
                _output,
                new DockerVolume[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsNoble),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", "10.0", ImageTestHelperConstants.OsTypeUbuntuNoble),
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
                    Assert.DoesNotContain("Exception", data);
                });
        }
    }
}
