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
    [Trait("category", "dotnetcore-9.0")]
    public class DotNetCoreRuntimeVersion90Tests : DotNetCoreEndToEndTestsBase
    {
        public DotNetCoreRuntimeVersion90Tests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Theory]
        [Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-debian-bookworm")]
        [InlineData(NetCoreApp90MvcApp, "Welcome to ASP.NET Core 9 MVC!")]
        [InlineData(NetCoreApp90WebApp, "Welcome to a .NET 9 Web App!")]
        public async Task CanBuildAndRun_NetCore90AppAsync(string sampleApp, string webpageMessage)
        {
            // Arrange
            var dotnetcoreVersion = DotNetCoreRunTimeVersions.NetCoreApp90;
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
                _imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBookworm),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", "9.0", ImageTestHelperConstants.OsTypeDebianBookworm),
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
    }
}