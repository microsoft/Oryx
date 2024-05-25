// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "dotnetcore-6.0")]
    public class DotNetCoreRuntimeVersion60Tests : DotNetCoreEndToEndTestsBase
    {
        public DotNetCoreRuntimeVersion60Tests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        [Trait("build-image", "github-actions-debian-stretch")]
        public async Task CanBuildAndRun_NetCore60MvcAppAsync()
        {
            // Arrange
            var dotnetcoreVersion = FinalStretchVersions.FinalStretchDotNetCoreApp60RunTimeVersion;
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp60MvcApp);
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
                NetCoreApp60MvcApp,
                _output,
                new DockerVolume[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", "6.0", ImageTestHelperConstants.OsTypeDebianBullseye),
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
        public async Task CanBuildAndRun_NetCore60MvcApp_WithCustomizedRunCommand()
        {
            // Arrange
            var dotnetcoreVersion = FinalStretchVersions.FinalStretchDotNetCoreApp60RunTimeVersion;
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp60MvcApp);
            var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tmpDir);
            try
            {
                var tmpVolume = DockerVolume.CreateMirror(tmpDir, true);
                var tmpContainerDir = tmpVolume.ContainerDir;
                var volume = DockerVolume.CreateMirror(hostDir);
                var appDir = volume.ContainerDir;
                var appOutputDirVolume = CreateAppOutputDirVolume();
                var appOutputDir = appOutputDirVolume.ContainerDir;
                var appsvcFile = appOutputDirVolume.ContainerDir + "/appsvc.yaml";
                var runCommand = "echo 'Hello Azure! New Feature!!'";
                var buildImageScript = new ShellScriptBuilder()
                   .AddCommand(
                    $"oryx build {appDir} -i /tmp/int --platform {DotNetCoreConstants.PlatformName} " +
                    $"--platform-version {dotnetcoreVersion} -o {appOutputDir}")
                   .ToString();
                var runtimeImageScript = new ShellScriptBuilder()
                    .CreateFile(appsvcFile, $"\"run: {runCommand}\"")
                    .AddCommand(
                    $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort} -output {tmpContainerDir}/run.sh")
                    .AddCommand($".{tmpContainerDir}/run.sh")
                    .ToString();

                await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                    NetCoreApp60MvcApp,
                    _output,
                    new DockerVolume[] { volume, appOutputDirVolume, tmpVolume },
                    _imageHelper.GetGitHubActionsBuildImage(),
                    "/bin/sh",
                    new[]
                    {
                    "-c",
                    buildImageScript
                    },
                    _imageHelper.GetRuntimeImage("dotnetcore", "6.0", ImageTestHelperConstants.OsTypeDebianBullseye),
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

                        var runScript = File.ReadAllText(Path.Combine(tmpDir, "run.sh"));
                        Assert.Contains(runCommand, runScript);
                    });
            }
            finally
            {
                Directory.Delete(tmpDir, true);
            }
        }

        [Fact]
        [Trait("build-image", "github-actions-debian-stretch")]
        public async Task CanBuildAndRun_Adds_Oryx_AppInsights_Codeless_ConfigurationAsync()
        {
            // Arrange
            var dotnetcoreVersion = FinalStretchVersions.FinalStretchDotNetCoreApp60RunTimeVersion;
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp60MvcApp);
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
                .AddCommand("cat run.sh")
                .AddStringExistsInFileCheck(OryxAppInsightsAttachString1, $"run.sh")
                .AddStringExistsInFileCheck(OryxAppInsightsAttachString2, $"run.sh")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp60MvcApp,
                _output,
                new DockerVolume[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", "6.0", ImageTestHelperConstants.OsTypeDebianBullseye),
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
        public async Task CanBuildAndRun_DoesNot_Add_Oryx_AppInsights_Codeless_ConfigurationAsync()
        {
            // Arrange
            var dotnetcoreVersion = FinalStretchVersions.FinalStretchDotNetCoreApp60RunTimeVersion;
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp60MvcApp);
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
                .AddCommand($"export {agentExtensionVersionEnv}=~2")
                .AddCommand($"export {connectionStringEnv}=alkajsldkajd")
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand("cat run.sh")
                .AddStringDoesNotExistInFileCheck(OryxAppInsightsAttachString1, $"run.sh")
                .AddStringDoesNotExistInFileCheck(OryxAppInsightsAttachString2, $"run.sh")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp60MvcApp,
                _output,
                new DockerVolume[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", "6.0", ImageTestHelperConstants.OsTypeDebianBullseye),
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
        [Trait("build-image", "debian-stretch")]
        public async Task CanBuildAndRun_NetCore60MvcApp_UsingExplicitStartupCommandAsync()
        {
            // Arrange
            var dotnetcoreVersion = FinalStretchVersions.FinalStretchDotNetCoreApp60RunTimeVersion;
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp60MvcApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var startupCommand = $"./{NetCoreApp60MvcApp}";
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
               $"--platform {DotNetCoreConstants.PlatformName} " +
               $"--platform-version {dotnetcoreVersion}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -userStartupCommand {startupCommand} " +
                $"-bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp60MvcApp,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", "6.0", ImageTestHelperConstants.OsTypeDebianBullseye),
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
        public async Task CanBuildAndRunApp_FromNestedOutputDirectoryAsync()
        {
            // Arrange
            var dotnetcoreVersion = FinalStretchVersions.FinalStretchDotNetCoreApp60RunTimeVersion;
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp60MvcApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildImageScript = new ShellScriptBuilder()
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int --platform {DotNetCoreConstants.PlatformName} " +
                $"--platform-version {dotnetcoreVersion} " +
                $"-o {appOutputDir}")
               .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp60MvcApp,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", "6.0", ImageTestHelperConstants.OsTypeDebianBullseye),
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
        public async Task CanRunApp_UsingPreRunCommand_FromBuildEnvFileAsync()
        {
            // Arrange
            var dotnetcoreVersion = FinalStretchVersions.FinalStretchDotNetCoreApp60RunTimeVersion;
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", NetCoreApp60MvcApp);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var expectedFileInOutputDir = Guid.NewGuid().ToString("N");
            var buildImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int --platform {DotNetCoreConstants.PlatformName} " +
                $"--platform-version {dotnetcoreVersion} -o {appOutputDir}")
                // Create a 'build.env' file
                .AddCommand(
                $"echo '{FilePaths.PreRunCommandEnvVarName}=\"echo > {expectedFileInOutputDir}\"' > " +
                $"{appOutputDir}/{BuildScriptGeneratorCli.Constants.BuildEnvironmentFileName}")
                .ToString();
            var runtimeImageScript = new ShellScriptBuilder()
                .AddCommand(
                $"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                NetCoreApp60MvcApp,
                _output,
                new DockerVolume[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(),
                "/bin/sh",
                new[]
                {
                    "-c",
                    buildImageScript
                },
                _imageHelper.GetRuntimeImage("dotnetcore", "6.0", ImageTestHelperConstants.OsTypeDebianBullseye),
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

                    // Verify that the file created using the pre-run command is 
                    // in fact present in the output directory.
                    Assert.True(File.Exists(Path.Combine(appOutputDirVolume.MountedHostDir, expectedFileInOutputDir)));
                });
        }
    }
}