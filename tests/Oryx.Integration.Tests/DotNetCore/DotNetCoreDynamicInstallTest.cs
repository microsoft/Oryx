// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "dotnetcore")]
    public class DotNetCoreDynamicInstallTest : DotNetCoreEndToEndTestsBase
    {
        public DotNetCoreDynamicInstallTest(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Theory]
        [InlineData("2.1", NetCoreApp21WebApp, "Hello World!")]
        [InlineData("3.1", NetCoreApp31MvcApp, "Welcome to ASP.NET Core MVC!")]
        public async Task CanBuildAndRun_NetCore31WebApp(
            string runtimeVersion,
            string appName,
            string expectedResponseContent)
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
               .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
               .AddCommand(
                $"oryx build {appDir} " +
                $"--platform {DotNetCoreConstants.PlatformName} --platform-version {runtimeVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume },
                _imageHelper.GetGitHubActionsBuildImage(),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", runtimeVersion),
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