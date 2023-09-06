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
    [Trait("category", "dotnetcore-dynamic")]
    public class DotNetCoreDynamicInstallTest : DotNetCoreEndToEndTestsBase
    {
        public DotNetCoreDynamicInstallTest(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Theory]
        [Trait("build-image", "github-actions-debian-stretch")]
        [InlineData("3.1", ImageTestHelperConstants.OsTypeDebianBuster, NetCoreApp31MvcApp, "Welcome to ASP.NET Core MVC!")]
        [InlineData("3.1", ImageTestHelperConstants.OsTypeDebianBullseye, NetCoreApp31MvcApp, "Welcome to ASP.NET Core MVC!")]
        public async Task CanBuildAndRun_NetCore31WebAppAsync(
            string runtimeVersion,
            string osType,
            string appName,
            string expectedResponseContent)
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int " +
                $"--platform {DotNetCoreConstants.PlatformName} --platform-version {runtimeVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", runtimeVersion, osType),
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
                    Assert.Contains(expectedResponseContent, data);
                });
        }

        [Theory]
        [Trait("build-image", "github-actions-debian-stretch")]
        [InlineData("3.1", NetCoreApp31MvcApp, "Welcome to ASP.NET Core MVC!")]
        [InlineData("5.0", Net5MvcApp, "Welcome to ASP.NET Core MVC!")]
        public async Task CanBuildAndRunAppUsingDynamicInstallationOfRuntimeInRuntimeImageAsync(
            string runtimeVersion,
            string appName,
            string expectedResponseContent)
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int " +
                $"--platform {DotNetCoreConstants.PlatformName} --platform-version {runtimeVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", "dynamic", ImageTestHelperConstants.OsTypeDebianBuster),
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
                    Assert.Contains(expectedResponseContent, data);
                });
        }
    }
}