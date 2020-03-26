// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "dotnetcore")]
    public class DotNetCorePreRunCommandOrScriptTest : DotNetCoreEndToEndTestsBase
    {
        public DotNetCorePreRunCommandOrScriptTest(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore21WebApp_UsingPreRunCommand_WithDynamicInstall()
        {
            // Arrange
            var runtimeVersion = "2.1";
            var appName = "NetCoreApp30WebApp";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
               .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
               .AddCommand(
                $"oryx build {appDir} --platform dotnet --platform-version {runtimeVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(SettingsKeys.EnableDynamicInstall, true.ToString())
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .SetEnvironmentVariable(FilePaths.PreRunCommandEnvVarName, $"touch \"{appOutputDir}/test_pre_run.txt\"")
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp21WebApp,
                _output,
                new[] { volume },
                _imageHelper.GetGitHubActionsBuildImage(),
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
                    Assert.Contains("Hello World!", data);
                    Assert.IsTrue(File.Exists($"{appOutputDir}/test_pre_run.txt"));
                });
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore31WebApp_UsingPreRunScript_WithDynamicInstall()
        {
            // Arrange
            var runtimeVersion = "3.1";
            var appName = "NetCoreApp30MvcApp";
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildImageScript = new ShellScriptBuilder()
               .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
               .AddCommand(
                $"oryx build {appDir} --platform dotnet --platform-version {runtimeVersion} -o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(SettingsKeys.EnableDynamicInstall, true.ToString())
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .SetEnvironmentVariable(FilePaths.PreRunCommandEnvVarName, "./prerunscript.sh && apt-get install foo")
                .AddCommand($"touch {appOutputDir}/prerunscript.sh")
                .AddCommand($"echo \"export PRE_RUN_TEST_SUCCESS=true\" > {appOutputDir}/prerunscript.sh")
                .AddCommand($"chmod 755 {appOutputDir}/prerunscript.sh")
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp31WebApp,
                _output,
                new[] { volume },
                _imageHelper.GetGitHubActionsBuildImage(),
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
                    Assert.IsTrue(File.Exists($"{appOutputDir}/prerunscript.sh"));
                    Assert.Equals(Environment.GetEnvironmentVariable("PRE_RUN_TEST_SUCCESS"), "true");
                });
        }
    }
}