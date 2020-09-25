// --------------------------------------------------------------------------------------------
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
    [Trait("category", "dotnetcore")]
    public class DotNetCoreRuntimeVersion11Tests : DotNetCoreEndToEndTestsBase
    {
        public DotNetCoreRuntimeVersion11Tests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore11WebApp()
        {
            // Arrange
            var dotnetcoreVersion = "1.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp11WebApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    SettingsKeys.DynamicInstallRootDir,
                    BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot)
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddCommand(
                $"oryx build {appDir} --platform {DotNetCoreConstants.PlatformName} " +
                $"--platform-version {dotnetcoreVersion} -o {appOutputDir}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp11WebApp,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", dotnetcoreVersion),
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
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore11WebApp_HavingExplicitAssemblyName()
        {
            // Arrange
            var appName = "NetCoreApp11WithExplicitAssemblyName";
            var dotnetcoreVersion = "1.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    SettingsKeys.DynamicInstallRootDir, 
                    BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot)
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddCommand(
                $"oryx build {appDir} --platform {DotNetCoreConstants.PlatformName} " +
                $"--platform-version {dotnetcoreVersion} -o {appOutputDir}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", dotnetcoreVersion),
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
                    Assert.Contains("Hello World!", data);
                });
        }
    }
}