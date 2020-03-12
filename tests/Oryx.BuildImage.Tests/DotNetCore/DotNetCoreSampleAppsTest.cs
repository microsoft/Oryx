// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;
using ScriptGenerator = Microsoft.Oryx.BuildScriptGenerator;

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

        private readonly string SdkVersionMessageFormat = "Using .NET Core SDK Version: {0}";

        [Fact]
        public void Builds_NetCore10App_UsingNetCore11_DotNetSdkVersion()
        {
            // Arrange
            var appName = "aspnetcore10";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/aspnetcore10-output";
            var manifestFile = $"{appOutputDir}/{FilePaths.BuildManifestFileName}";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/app.dll")
                .AddFileExistsCheck(manifestFile)
                .AddCommand($"cat {manifestFile}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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
                        string.Format(SdkVersionMessageFormat, DotNetCoreSdkVersions.DotNetCore11SdkVersion),
                        result.StdOut);
                    Assert.Contains(
                        $"{ManifestFilePropertyKeys.DotNetCoreRuntimeVersion}=\"{DotNetCoreRunTimeVersions.NetCoreApp10}\"",
                        result.StdOut);
                    Assert.Contains(
                        $"{ManifestFilePropertyKeys.DotNetCoreSdkVersion}=\"{DotNetCoreSdkVersions.DotNetCore11SdkVersion}\"",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Builds_NetCore11App_UsingNetCore11_DotNetSdkVersion()
        {
            // Arrange
            var appName = "NetCoreApp11WebApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/NetCoreApp11WebApp-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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
                        string.Format(SdkVersionMessageFormat, DotNetCoreSdkVersions.DotNetCore11SdkVersion),
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Builds_NetCore20App_UsingNetCore21_DotNetSdkVersion()
        {
            // Arrange
            var appName = "aspnetcore20";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/aspnetcore10-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/app.dll")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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
                        string.Format(SdkVersionMessageFormat, DotNetCoreSdkVersions.DotNetCore21SdkVersion),
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData(Settings.BuildImageName)]
        [InlineData(Settings.SlimBuildImageName)]
        public void Builds_NetCore21App_UsingNetCore21_DotNetSdkVersion(string buildImageName)
        {
            // Arrange
            var appName = "NetCoreApp21WebApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/NetCoreApp21WebApp-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = buildImageName,
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
                        string.Format(SdkVersionMessageFormat, DotNetCoreSdkVersions.DotNetCore21SdkVersion),
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Builds_NetCore22App_UsingNetCore22_DotNetSdkVersion()
        {
            // Arrange
            var appName = "NetCoreApp22WebApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/NetCoreApp22WebApp-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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
                        string.Format(SdkVersionMessageFormat, DotNetCoreSdkVersions.DotNetCore22SdkVersion),
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Builds_NetCore30App_UsingNetCore30_DotNetSdkVersion()
        {
            // Arrange
            var appName = "NetCoreApp30.WebApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/NetCoreApp30WebApp-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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
                        string.Format(
                            SdkVersionMessageFormat,
                            DotNetCoreSdkVersions.DotNetCore30SdkVersion),
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Builds_NetCore31App_UsingNetCore31_DotNetSdkVersion()
        {
            // Arrange
            var appName = "NetCoreApp31.MvcApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/NetCoreApp31MvcApp-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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
                        string.Format(
                            SdkVersionMessageFormat,
                            DotNetCoreSdkVersions.DotNetCore31SdkVersion),
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Build_ExecutesPreAndPostBuildScripts_WithinBenvContext()
        {
            // Arrange
            var appName = "NetCoreApp21WebApp";
            var volume = CreateSampleAppVolume(appName);
            using (var sw = File.AppendText(Path.Combine(volume.MountedHostDir, "build.env")))
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
                .AddBuildCommand($"{appDir} --platform dotnet --platform-version 2.1")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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
                    var dotnetExecutable = $"/opt/dotnet/sdks/{DotNetCoreSdkVersions.DotNetCore21SdkVersion}/dotnet";
                    Assert.Matches($"Pre-build script: {dotnetExecutable}", result.StdOut);
                    Assert.Matches($"Post-build script: {dotnetExecutable}", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Build_CopiesContentCreatedByPreAndPostBuildScript_ToExplicitOutputDirectory()
        {
            // Arrange
            var appName = "NetCoreApp21WebApp";
            var volume = CreateSampleAppVolume(appName);
            using (var sw = File.AppendText(Path.Combine(volume.MountedHostDir, "build.env")))
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
                .AddBuildCommand($"{appDir} -o {tempOutputDir} --platform dotnet --platform-version 2.1")
                .AddFileExistsCheck($"{tempOutputDir}/pre-{fileName}")
                .AddFileExistsCheck($"{tempOutputDir}/post-{fileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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

        [Fact]
        public void Build_Executes_InlinePreAndPostBuildCommands()
        {
            // Arrange
            var appName = "NetCoreApp21WebApp";
            var volume = CreateSampleAppVolume(appName);
            using (var sw = File.AppendText(Path.Combine(volume.MountedHostDir, "build.env")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("PRE_BUILD_COMMAND=\"echo from pre-build command\"");
                sw.WriteLine("POST_BUILD_COMMAND=\"echo from post-build command\"");
            }

            var appDir = volume.ContainerDir;
            var tempOutputDir = "/tmp/output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {tempOutputDir} --platform dotnet --platform-version 2.1")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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

        [Fact]
        public void Build_CopiesContentCreatedByPostBuildScript_ToExplicitOutputDirectory_AndOutpuIsZipped()
        {
            // Arrange
            var appName = "NetCoreApp21WebApp";
            var volume = CreateSampleAppVolume(appName);
            using (var sw = File.AppendText(Path.Combine(volume.MountedHostDir, "build.env")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("POST_BUILD_SCRIPT_PATH=scripts/postbuild.sh");
            }
            var scriptsDir = Directory.CreateDirectory(Path.Combine(volume.MountedHostDir, "scripts"));
            var fileName = $"{Guid.NewGuid().ToString("N")}.txt";
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
                .AddBuildCommand(
                $"{appDir} -o {tempOutputDir} --platform dotnet --platform-version 2.1 " +
                $"-p {ScriptGenerator.Constants.ZipAllOutputBuildPropertyKey}=true")
                .AddFileDoesNotExistCheck($"{tempOutputDir}/post-{fileName}")
                .AddCommand($"cd {tempOutputDir} && tar -xvf {FilePaths.CompressedOutputFileName}")
                .AddFileExistsCheck($"{tempOutputDir}/post-{fileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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

        [Fact]
        public void MultiPlatformBuild_IsDisabled()
        {
            // Arrange
            var appName = "dotnetreact";
            var hostDir = Path.Combine(_hostSamplesDir, "multilanguage", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildScript = new ShellScriptBuilder()
                .AddCommand("export ENABLE_MULTIPLATFORM_BUILD=true")
                .AddBuildCommand($"{appDir} -o {appOutputDir} --platform dotnet --platform-version 2.2")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", buildScript }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.DoesNotContain(@"npm install", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
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
                ImageId = Settings.BuildImageName,
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

        [Fact]
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
                ImageId = Settings.BuildImageName,
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

        [Fact]
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
                ImageId = Settings.BuildImageName,
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

        [Fact]
        public void Builds_AzureFunctionsProject()
        {
            // Arrange
            var appName = "AzureFunctionsHttpTriggerApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/AzureFunctionsHttpTriggerApp-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/bin/{appName}.dll")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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
                        string.Format(SdkVersionMessageFormat, DotNetCoreSdkVersions.DotNetCore21SdkVersion),
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData(DotNetCoreSdkVersions.DotNetCore11SdkVersion)]
        [InlineData(DotNetCoreSdkVersions.DotNetCore21SdkVersion)]
        [InlineData(DotNetCoreSdkVersions.DotNetCore22SdkVersion)]
        [InlineData(DotNetCoreSdkVersions.DotNetCore30SdkVersion)]
        [InlineData(DotNetCoreSdkVersions.DotNetCore31SdkVersion)]
        public void DotNetCore_Muxer_ChoosesAppropriateSDKVersion(string sdkversion)
        {
            // Arrange
            var appDir = "/tmp/app1";
            var flattenedDotNetInstallDir = "/opt/dotnet/all";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable("PATH", $"{flattenedDotNetInstallDir}:$PATH")
                .AddCommand($"mkdir -p {appDir} && cd {appDir}")
                .AddCommand($"dotnet new globaljson --sdk-version {sdkversion}")
                .AddCommand("dotnet --version")
                .AddCommand("which dotnet")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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

        [Fact]
        public void Builds_AndCopiesOutput_ToOutputDirectory_NestedUnderSourceDirectory()
        {
            // Arrange
            var appName = "NetCoreApp31.MvcApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appDir}/output")
                .AddFileExistsCheck($"{appDir}/output/{appName}.dll")
                .AddDirectoryDoesNotExistCheck($"{appDir}/output/output")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.SlimBuildImageName,
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
                        string.Format(
                            SdkVersionMessageFormat,
                            DotNetCoreSdkVersions.DotNetCore31SdkVersion),
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void SubsequentBuilds_CopyOutput_ToOutputDirectory_NestedUnderSourceDirectory()
        {
            // Arrange
            var appName = "NetCoreApp31.MvcApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            // NOTE: we want to make sure that even after subsequent builds(like in case of AppService),
            // the output structure is like what we expect.
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appDir}/output")
                .AddBuildCommand($"{appDir} -o {appDir}/output")
                .AddBuildCommand($"{appDir} -o {appDir}/output")
                .AddFileExistsCheck($"{appDir}/output/{appName}.dll")
                .AddDirectoryDoesNotExistCheck($"{appDir}/output/output")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.SlimBuildImageName,
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
                        string.Format(
                            SdkVersionMessageFormat,
                            DotNetCoreSdkVersions.DotNetCore31SdkVersion),
                        result.StdOut);
                },
                result.GetDebugInfo());
        }
    }
}