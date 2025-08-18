// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    [Trait("platform", "dotnet")]
    public class DotNetCoreSampleAppsTest : SampleAppsTestBase
    {
        public DotNetCoreSampleAppsTest(ITestOutputHelper output) : base(output)
        {
        }

        private DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "DotNetCore", sampleAppName));

        [Fact, Trait("category", "githubactions")]
        public void PipelineTestInvocation()
        {
            GDIPlusLibrary_IsPresentInTheImage(ImageTestHelperConstants.GitHubActionsBullseye);
            GDIPlusLibrary_IsPresentInTheImage(ImageTestHelperConstants.GitHubActionsBookworm);
        }

        private readonly string SdkVersionMessageFormat = "Using .NET Core SDK Version: {0}";

        [Fact, Trait("category", "githubactions")]
        public void Builds_NetCore31App_UsingNetCore31_DotNetSdkVersion()
        {
            // Arrange
            var appName = "NetCoreApp31.MvcApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/NetCoreApp31MvcApp-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --platform dotnet " +
                $"--platform-version {FinalStretchVersions.FinalStretchDotNetCoreApp31RunTimeVersion}")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        string.Format(SdkVersionMessageFormat, FinalStretchVersions.FinalStretchDotNetCore31SdkVersion),
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void Builds_NetCore31App_UsingNetCore31_DotNetSdkVersion_CustomError()
        {
            // Arrange
            var appName = "NetCoreApp31.MvcApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/NetCoreApp31MvcApp-output";
            var script = new ShellScriptBuilder()
                .AddCommand($"echo RandomText >> {appDir}/Program.cs") // triggers a failure
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --platform dotnet " +
                $"--platform-version {FinalStretchVersions.FinalStretchDotNetCoreApp31RunTimeVersion}")
                .ToString();
            // Regex will match:
            // "yyyy-mm-dd hh:mm:ss"|ERROR|Micro
            Regex regex = new Regex(@"""[0-9]{4}-(0[1-9]|1[0-2])-(0[1-9]|[1-2][0-9]|3[0-1]) (0[0-9]|1[0-9]|2[0-3]):([0-5][0-9]):([0-5][0-9])""\|ERROR\|.*");

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.False(result.IsSuccess);
                    Match match = regex.Match(result.StdOut);
                    Assert.True(match.Success);
                },
                result.GetDebugInfo());
        }

        // This test is necessary once .NET 6 preview 5 come out.
        [Fact, Trait("category", "githubactions")]
        public void Builds_Net6BlazorWasmApp_RunsAOTCompilationInstallCommands()
        {
            // Arrange
            var appName = "NetCore6BlazorWasmApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} --platform dotnet " +
                $"--platform-version {FinalStretchVersions.FinalStretchDotNetCoreApp60RunTimeVersion}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(string.Format(SdkVersionMessageFormat, FinalStretchVersions.FinalStretchDotNet60SdkVersion), result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void Build_ExecutesPreAndPostBuildScripts_WithinBenvContext()
        {
            // Arrange
            var appName = "NetCoreApp8WebApp";
            var volume = CreateSampleAppVolume(appName);
            using (var sw = File.AppendText(
                Path.Combine(volume.MountedHostDir, BuildScriptGeneratorCli.Constants.BuildEnvironmentFileName)))
            {
                sw.NewLine = "\n";
                sw.WriteLine("PRE_BUILD_SCRIPT_PATH=scripts/prebuild.sh");
                sw.WriteLine("POST_BUILD_SCRIPT_PATH=scripts/postbuild.sh");
            }
            var scriptsDir = Directory.CreateDirectory(Path.Combine(volume.MountedHostDir, "scripts"));
            using (var sw = File.AppendText(Path.Combine(scriptsDir.FullName, "prebuild.sh")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("#!/bin/bash");
                sw.WriteLine("echo \"Pre-build script: $dotnet\"");
            }
            using (var sw = File.AppendText(Path.Combine(scriptsDir.FullName, "postbuild.sh")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("#!/bin/bash");
                sw.WriteLine("echo \"Post-build script: $dotnet\"");
            }
            if (RuntimeInformation.IsOSPlatform(Settings.LinuxOS))
            {
                ProcessHelper.RunProcess(
                    "chmod",
                    new[] { "-R", "777", scriptsDir.FullName },
                    workingDirectory: null,
                    waitTimeForExit: null);
            }

            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} --platform {DotNetCoreConstants.PlatformName} " +
                $"--platform-version 8.0.412")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    var dotnetExecutable = $"{Constants.TemporaryInstallationDirectoryRoot}/dotnet/8.0.412/dotnet";
                    Assert.Matches($"Pre-build script: {dotnetExecutable}", result.StdOut);
                    Assert.Matches($"Post-build script: {dotnetExecutable}", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void Build_CopiesContentCreatedByPreAndPostBuildScript_ToExplicitOutputDirectory()
        {
            // NOTE: Here we are trying to verify that the pre and post build scripts are able to access the
            // source and destination directory environment variables.

            // Arrange
            var appName = "NetCoreApp8WebApp";
            var volume = CreateSampleAppVolume(appName);
            using (var sw = File.AppendText(
                Path.Combine(volume.MountedHostDir, BuildScriptGeneratorCli.Constants.BuildEnvironmentFileName)))
            {
                sw.NewLine = "\n";
                sw.WriteLine("PRE_BUILD_SCRIPT_PATH=scripts/prebuild.sh");
                sw.WriteLine("POST_BUILD_SCRIPT_PATH=scripts/postbuild.sh");
            }
            var scriptsDir = Directory.CreateDirectory(Path.Combine(volume.MountedHostDir, "scripts"));
            var fileName = $"{Guid.NewGuid().ToString("N")}.txt";
            using (var sw = File.AppendText(Path.Combine(scriptsDir.FullName, "prebuild.sh")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("#!/bin/bash");
                sw.WriteLine($"echo > $DESTINATION_DIR/pre-{fileName}");
            }
            using (var sw = File.AppendText(Path.Combine(scriptsDir.FullName, "postbuild.sh")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("#!/bin/bash");
                sw.WriteLine($"echo > $DESTINATION_DIR/post-{fileName}");
            }
            if (RuntimeInformation.IsOSPlatform(Settings.LinuxOS))
            {
                ProcessHelper.RunProcess(
                    "chmod",
                    new[] { "-R", "777", scriptsDir.FullName },
                    workingDirectory: null,
                    waitTimeForExit: null);
            }

            var appDir = volume.ContainerDir;
            var tempOutputDir = "/tmp/output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {tempOutputDir} --platform {DotNetCoreConstants.PlatformName} --platform-version 8.0")
                .AddFileExistsCheck($"{tempOutputDir}/pre-{fileName}")
                .AddFileExistsCheck($"{tempOutputDir}/post-{fileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void Build_Executes_InlinePreAndPostBuildCommands()
        {
            // Arrange
            var appName = "NetCoreApp8WebApp";
            var volume = CreateSampleAppVolume(appName);
            using (var sw = File.AppendText(
                Path.Combine(volume.MountedHostDir, BuildScriptGeneratorCli.Constants.BuildEnvironmentFileName)))
            {
                sw.NewLine = "\n";
                sw.WriteLine("PRE_BUILD_COMMAND=\"echo from pre-build command\"");
                sw.WriteLine("POST_BUILD_COMMAND=\"echo from post-build command\"");
            }

            var appDir = volume.ContainerDir;
            var tempOutputDir = "/tmp/output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {tempOutputDir} --platform {DotNetCoreConstants.PlatformName} --platform-version 8.0")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("from pre-build command", result.StdOut);
                    Assert.Contains("from post-build command", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void Build_DoesNotClean_DestinationDirectory_ByDefault()
        {
            // Arrange
            var appName = "NetCoreApp21WebApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/NetCoreApp21WebApp-output";
            var extraFile = $"{Guid.NewGuid().ToString("N")}.txt";
            var script = new ShellScriptBuilder()
                .CreateDirectory($"{appOutputDir}")
                .AddCommand($"echo > {appOutputDir}/{extraFile}")
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .AddFileExistsCheck($"{appOutputDir}/{extraFile}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void BuildsApplication_InIntermediateDirectory_WhenIntermediateDirectorySwitchIsUsed()
        {
            // Arrange
            var appName = "NetCoreApp21WebApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/NetCoreApp21WebApp-output";
            var intermediateDir = "/tmp/int";
            var script = new ShellScriptBuilder()
                .AddCommand($"rm -rf {appDir}/bin")
                .AddBuildCommand($"{appDir} -i {intermediateDir} -o {appOutputDir}")
                .AddDirectoryDoesNotExistCheck($"{appDir}/bin")
                .AddDirectoryExistsCheck($"{intermediateDir}/bin")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void BuildsMultiWebAppRepoApp_InIntermediateDirectory_WhenIntermediateDirectorySwitchIsUsed()
        {
            // Arrange
            var appName = "MultiWebAppRepo";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var projectDir = $"{appDir}/src/WebApp1";
            var appOutputDir = "/tmp/MultiWebAppRepo-output";
            var intermediateDir = "/tmp/int";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(EnvironmentSettingsKeys.Project, "src/WebApp1/WebApp1.csproj")
                .AddCommand($"rm -rf {projectDir}/bin")
                .AddBuildCommand($"{appDir} -i {intermediateDir} -o {appOutputDir}")
                .AddDirectoryDoesNotExistCheck($"{projectDir}/bin")
                .AddDirectoryExistsCheck($"{intermediateDir}/src/WebApp1/bin")
                .AddFileExistsCheck($"{appOutputDir}/MyWebApp.dll")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void Builds_AzureFunctionsProject()
        {
            // Arrange
            var appName = "AzureFunctionsHttpTriggerApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/AzureFunctionsHttpTriggerApp-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir} --platform dotnet --platform-version 8.0.412")
                .AddFileExistsCheck($"{appOutputDir}/bin/{appName}.dll")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.OsTypeFileName}")
                .AddStringExistsInFileCheck(
                    $"{ManifestFilePropertyKeys.PlatformName}=\"{DotNetCoreConstants.PlatformName}\"",
                    $"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(string.Format(SdkVersionMessageFormat, "8.0.412"), result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact(Skip = "Skipping till we fix Bug#1288173"), Trait("category", "latest")]
        public void Builds_SingleBlazorWasmProject_Without_Setting_Apptype_Option()
        {
            // Arrange
            var appName = "Net5BlazorWasmClientApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/blazor-wasm-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --platform dotnet " +
                $"--platform-version 5.0.0")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.OsTypeFileName}")
                .AddStringExistsInFileCheck(
                ManifestFilePropertyKeys.PlatformName, $"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                EnvironmentVariables = new List<EnvironmentVariable>
                {
                    CreateAppNameEnvVar(appName)
                },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(string.Format(SdkVersionMessageFormat, "5.0.100"), result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [Trait("category", "githubactions")]
        [InlineData("BlazorStarterAppNet8")]
        [InlineData("BlazorVanillaApiAppNet8")]
        public void Builds_AzureBlazorWasmFunctionProject_By_Setting_Apptype_Via_BuildCommand(string appName)
        {
            // Arrange
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --apptype {Constants.StaticSiteApplications} " +
                $"--platform dotnet --platform-version 8.0")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.OsTypeFileName}")
                .AddStringExistsInFileCheck(ManifestFilePropertyKeys.PlatformName, $"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable>
                {
                    CreateAppNameEnvVar(appName)
                },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(string.Format(SdkVersionMessageFormat, DotNetCoreSdkVersions.DotNet80SdkVersion), result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [Trait("category", "githubactions")]
        [InlineData("BlazorStarterAppNet8")]
        [InlineData("BlazorVanillaApiAppNet8")]
        public void Builds_AzureFunctionProject_FromBlazorFunctionRepo_When_Apptype_Is_SetAs_Functions(string appName)
        {
            // Arrange
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/blazor-wasm-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --apptype {Constants.FunctionApplications} --platform dotnet " +
                $"--platform-version 8.0")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.OsTypeFileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable>
                {
                    CreateAppNameEnvVar(appName)
                },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        string.Format(SdkVersionMessageFormat, DotNetCoreSdkVersions.DotNet80SdkVersion),
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void Builds_Application_Checks_OutputType_In_Manifest()
        {
            // Arrange
            var appName = "NetCore6PreviewMvcApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/netcore6-preview-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir}/ -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.OsTypeFileName}")
                .AddStringExistsInFileCheck($"{ManifestFilePropertyKeys.OutputType}=\"Exe\"", $"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable>
                {
                    CreateAppNameEnvVar(appName)
                },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Theory, Trait("category", "githubactions")]
        [InlineData(DotNetCoreSdkVersions.DotNet80SdkVersion)]
        public void DotNetCore_Muxer_ChoosesAppropriateSDKVersion(string sdkversion)
        {
            // Arrange
            var appDir = "/tmp/app1";
            var flattenedDotNetInstallDir = "/opt/dotnet/all";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir} && cd {appDir}")
                .AddCommand($"dotnet new globaljson --sdk-version {sdkversion}")
                .SetEnvironmentVariable("PATH", $"{flattenedDotNetInstallDir}:$PATH", true)
                .AddCommand("dotnet --version")
                .AddCommand("which dotnet")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable>(),
                Volumes = Enumerable.Empty<DockerVolume>(),
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(sdkversion, result.StdOut);
                    Assert.Contains($"{flattenedDotNetInstallDir}/dotnet", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void Builds_AndCopiesOutput_ToOutputDirectory_NestedUnderSourceDirectory()
        {
            // Arrange
            var appName = "NetCoreApp31.MvcApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appDir}/output --platform dotnet --platform-version 3.1.8")
                .AddFileExistsCheck($"{appDir}/output/{appName}.dll")
                .AddDirectoryDoesNotExistCheck($"{appDir}/output/output")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(string.Format(SdkVersionMessageFormat, "3.1.402"), result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void SubsequentBuilds_CopyOutput_ToOutputDirectory_NestedUnderSourceDirectory()
        {
            // Arrange
            var appName = "NetCoreApp31.MvcApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            // NOTE: we want to make sure that even after subsequent builds(like in case of AppService),
            // the output structure is like what we expect.
            var platformNameAndVersion = "--platform dotnet --platform-version 3.1.8";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appDir}/output {platformNameAndVersion}")
                .AddBuildCommand($"{appDir} -o {appDir}/output {platformNameAndVersion}")
                .AddBuildCommand($"{appDir} -o {appDir}/output {platformNameAndVersion}")
                .AddFileExistsCheck($"{appDir}/output/{appName}.dll")
                .AddDirectoryDoesNotExistCheck($"{appDir}/output/output")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(string.Format(SdkVersionMessageFormat, "3.1.402"), result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData(ImageTestHelperConstants.GitHubActionsBullseye)]
        [InlineData(ImageTestHelperConstants.GitHubActionsBookworm)]
        public void GDIPlusLibrary_IsPresentInTheImage(string tagName)
        {
            // Arrange
            var expectedLibrary = "libgdiplus";

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetBuildImage(tagName),
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", $"ldconfig -p | grep {expectedLibrary}" },
            });

            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedLibrary, actualOutput);
                },
                result.GetDebugInfo());
        }

        /// <summary>
        /// Tests that a v3 Azure Function app targeting .NET Core 3.1 can be built with the githubactions image.
        /// </summary>
        /// <remarks>Find supported Azure Function app target frameworks here: https://docs.microsoft.com/en-us/azure/static-web-apps/apis</remarks>
        [Fact, Trait("category", "githubactions")]
        public void GithubactionsImage_CanBuild_NetCore31_V3Functions_apps()
        {
            // Arrange
            var appName = "DotNetCore_HttpTriggerV3Sample";
            var volume = DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "azureFunctionsApps", appName));
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/dotnetcore-functions-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.OsTypeFileName}")
                .AddStringExistsInFileCheck($"{ManifestFilePropertyKeys.PlatformName}=\"{DotNetCoreConstants.PlatformName}\"", $"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        /// <summary>
        /// Tests that a v4 Azure Function app targeting .NET 6.0 can be built with the GithubActions image.
        /// </summary>
        /// <remarks>Find supported Azure Function app target frameworks here: https://docs.microsoft.com/en-us/azure/static-web-apps/apis</remarks>
        [Fact, Trait("category", "githubactions")]
        public void GithubactionsImage_CanBuild_Dotnet6_V4Functions_apps()
        {
            // Arrange
            var appName = "DotNetCore_HttpTriggerV4Sample";
            var volume = DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "azureFunctionsApps", appName));
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/dotnetcore-functions-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.OsTypeFileName}")
                .AddStringExistsInFileCheck($"{ManifestFilePropertyKeys.PlatformName}=\"{DotNetCoreConstants.PlatformName}\"", $"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        /// <summary>
        /// Tests that a v4 isolated Azure Function app targeting .NET 6 can be built with the GithubActions image.
        /// </summary>
        /// <remarks>Find supported Azure Function app target frameworks here: https://docs.microsoft.com/en-us/azure/static-web-apps/apis</remarks>
        [Fact, Trait("category", "githubactions")]
        public void GithubActionsImage_CanBuild_Dotnet6_Isolated_apps()
        {
            // Arrange
            var appName = "NetCore60IsolatedApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/isolatedapp-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.OsTypeFileName}")
                .AddStringExistsInFileCheck($"{ManifestFilePropertyKeys.PlatformName}=\"{DotNetCoreConstants.PlatformName}\"", $"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }
    }
}