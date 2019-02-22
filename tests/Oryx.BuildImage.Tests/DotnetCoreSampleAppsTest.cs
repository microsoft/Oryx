// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Oryx.BuildScriptGenerator.DotnetCore;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class DotnetCoreSampleAppsTest
    {
        private readonly ITestOutputHelper _output;
        private readonly DockerCli _dockerCli;
        private readonly string _hostSamplesDir;

        public DotnetCoreSampleAppsTest(ITestOutputHelper output)
        {
            _output = output;
            _dockerCli = new DockerCli();
            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
        }
        
        private DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.Create(Path.Combine(_hostSamplesDir, "DotNetCore", sampleAppName));

        private EnvironmentVariable CreateAppNameEnvVar(string sampleAppName) =>
            new EnvironmentVariable(LoggingConstants.AppServiceAppNameEnvironmentVariableName, sampleAppName);

        [Fact]
        public void Builds_NetCore11App_UsingNetCore11_DotnetSdkVersion()
        {
            // Arrange
            var appName = "NetCoreApp11WebApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/NetCoreApp11WebApp-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar(appName),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(".NET Core Version: " + DotnetCoreConstants.DotnetCoreSdkVersion11, result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Publishes_DotnetCore11App_ToOryxOutputDirectory_WhenSourceAndDestinationDir_AreSame()
        {
            // Arrange
            var appName = "NetCoreApp11WebApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir}")
                .AddFileExistsCheck($"{appDir}/{DotnetCoreConstants.OryxOutputPublishDirectory}/{appName}.dll")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar(appName),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(".NET Core Version: " + DotnetCoreConstants.DotnetCoreSdkVersion11, result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Builds_NetCore21App_UsingNetCore21_DotnetSdkVersion()
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
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar(appName),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(".NET Core Version: " + DotnetCoreConstants.DotnetCoreSdkVersion21, result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Publishes_DotnetCore21App_ToOryxOutputDirectory_WhenSourceAndDestinationDir_AreSame()
        {
            // Arrange
            var appName = "NetCoreApp21WebApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir}")
                .AddFileExistsCheck($"{appDir}/{DotnetCoreConstants.OryxOutputPublishDirectory}/{appName}.dll")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar(appName),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(".NET Core Version: " + DotnetCoreConstants.DotnetCoreSdkVersion21, result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Builds_NetCore22App_UsingNetCore22_DotnetSdkVersion()
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
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar(appName),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(".NET Core Version: " + DotnetCoreConstants.DotnetCoreSdkVersion22, result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Publishes_DotnetCore22App_ToOryxOutputDirectory_WhenSourceAndDestinationDir_AreSame()
        {
            // Arrange
            var appName = "NetCoreApp22WebApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir}")
                .AddFileExistsCheck($"{appDir}/{DotnetCoreConstants.OryxOutputPublishDirectory}/{appName}.dll")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar(appName),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(".NET Core Version: " + DotnetCoreConstants.DotnetCoreSdkVersion22, result.Output);
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
                .AddBuildCommand($"{appDir} -l dotnet --language-version 2.1")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar(appName),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Matches(@"Pre-build script: /opt/dotnet/2.1.\d+", result.Output);
                    Assert.Matches(@"Post-build script: /opt/dotnet/2.1.\d+", result.Output);
                },
                result.GetDebugInfo());
        }

        private void RunAsserts(Action action, string message)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                _output.WriteLine(message);
                throw;
            }
        }
    }
}