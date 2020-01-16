// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "dotnetcore")]
    public class DotNetCoreDynamicInstallationTest : DotNetCoreEndToEndTestsBase
    {
        private readonly string DefaultRuntimesRootDir = "/opt/dotnet/runtimes";
        private readonly string DefaultSdksRootDir = "/opt/dotnet/sdks";

        public DotNetCoreDynamicInstallationTest(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore31WebApp()
        {
            // Arrange
            var dotnetcoreVersion = "3.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp31MvcApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .SetEnvironmentVariable(SdkStorageConstants.UseLatestVersion, "true")
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
               .AddCommand(
                $"oryx build {appDir} --platform dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddCommand(
                $"oryx script -appPath {appOutputDir} -bindPort {ContainerPort} -setEnv true")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp30WebApp,
                _output,
                new[] { volume },
                _imageHelper.GetTestSlimBuildImage(),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetTestRuntimeImage("dotnetcore", "dynamic"),
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
        public async Task CanBuildAndRun_NetCore31WebApp_BySettingTheEnvironmentFirstInRuntimeImage()
        {
            // Arrange
            var dotnetcoreVersion = "3.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp31MvcApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .SetEnvironmentVariable(SdkStorageConstants.UseLatestVersion, "true")
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
               .AddCommand($"oryx build {appDir} --platform dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddCommand(
                $"oryx setupEnv -appPath {appOutputDir}")
                .AddCommand(
                $"oryx script -appPath {appOutputDir} -bindPort {ContainerPort} -setEnv false")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp30WebApp,
                _output,
                new[] { volume },
                _imageHelper.GetTestSlimBuildImage(),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetTestRuntimeImage("dotnetcore", "dynamic"),
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
        public async Task CanBuildAndRun_NetCore31WebApp_ByUsingScriptCommandAndSetEnvSwitchInRuntimeImage()
        {
            // Arrange
            var dotnetcoreVersion = "3.1";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp31MvcApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .SetEnvironmentVariable(SdkStorageConstants.UseLatestVersion, "true")
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
               .AddCommand($"oryx build {appDir} --platform dotnet --language-version {dotnetcoreVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddCommand(
                $"oryx script -appPath {appOutputDir} -bindPort {ContainerPort} -setupEnv true")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp30WebApp,
                _output,
                new[] { volume },
                _imageHelper.GetTestSlimBuildImage(),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetTestRuntimeImage("dotnetcore", "dynamic"),
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

        private string GetSnippetToCleanUpExistingInstallation()
        {
            return $"rm -rf {DefaultRuntimesRootDir}; mkdir -p {DefaultRuntimesRootDir}; " +
                $"rm -rf {DefaultSdksRootDir}; mkdir -p {DefaultSdksRootDir}";
        }
    }
}
