// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    [Trait("platform", "dotnet")]
    public class DotNetCoreDynamicInstallTest : SampleAppsTestBase
    {
        private DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "DotNetCore", sampleAppName));

        private readonly string SdkVersionMessageFormat = "Using .NET Core SDK Version: {0}";

        public DotNetCoreDynamicInstallTest(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData("NetCoreApp21WebApp", "2.1")]
        [InlineData("NetCoreApp31.MvcApp", "3.1")]
        public void BuildsApplication_InIntermediateDirectory_WhenIntermediateDirectorySwitchIsUsed(
            string appName,
            string runtimeVersion)
        {
            // Arrange
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {DotNetCoreConstants.PlatformName} --platform-version {runtimeVersion}")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .ToString();
            var majorPart = runtimeVersion.Split('.')[0];
            var expectedSdkVersionPrefix = $"{majorPart}.";

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
                    Assert.Contains(string.Format(SdkVersionMessageFormat, expectedSdkVersionPrefix), result.StdOut);
                    Assert.Contains(
                        $"{ManifestFilePropertyKeys.DotNetCoreRuntimeVersion}=\"{runtimeVersion}",
                        result.StdOut);
                    Assert.Contains(
                        $"{ManifestFilePropertyKeys.DotNetCoreSdkVersion}=\"{expectedSdkVersionPrefix}",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void DynamicInstall_ReInstallsSdk_IfSentinelFileIsNotPresent()
        {
            // Arrange
            var globalJsonTemplate = @"
            {
                ""sdk"": {
                    ""version"": ""#version#"",
                    ""rollForward"": ""Disable"",
                }
            }";
            var globalJsonSdkVersion = "3.1.201";
            var globalJsonContent = globalJsonTemplate.Replace("#version#", globalJsonSdkVersion);
            var sentinelFile = $"{DotNetCoreConstants.DynamicDotNetCoreSdkVersionsInstallDir}/{globalJsonSdkVersion}/" +
                $"{SdkStorageConstants.SdkDownloadSentinelFileName}";
            var appName = "NetCoreApp31.MvcApp";
            var volume = CreateSampleAppVolume(appName);
            // Create a global.json in host's app directory so that it can be present in container directory
            File.WriteAllText(
                Path.Combine(volume.MountedHostDir, DotNetCoreConstants.GlobalJsonFileName),
                globalJsonContent);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var buildCmd = $"{appDir} -i /tmp/int -o {appOutputDir}";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddBuildCommand(buildCmd)
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .AddFileExistsCheck(sentinelFile)
                .AddCommand($"rm -f {sentinelFile}")
                .AddBuildCommand(buildCmd)
                .AddFileExistsCheck(sentinelFile)
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
                    Assert.Contains(string.Format(SdkVersionMessageFormat, globalJsonSdkVersion), result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void BuildsApplication_IgnoresExplicitRuntimeVersionBasedSdkVersion_AndUsesSdkVersionSpecifiedInGlobalJson()
        {
            // Here we are testing building a 2.1 runtime version app with a 3.1 sdk version

            // Arrange
            var expectedSdkVersion = "3.1.201";
            var globalJsonTemplate = @"
            {
                ""sdk"": {
                    ""version"": ""#version#"",
                    ""rollForward"": ""Disable"",
                }
            }";
            var globalJsonContent = globalJsonTemplate.Replace("#version#", expectedSdkVersion);
            var appName = "NetCoreApp21WebApp";
            var runtimeVersion = "2.1";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";

            // Create a global.json in host's app directory so that it can be present in container directory
            File.WriteAllText(
                Path.Combine(volume.MountedHostDir, DotNetCoreConstants.GlobalJsonFileName),
                globalJsonContent);

            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {DotNetCoreConstants.PlatformName} --platform-version {runtimeVersion}")
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
                    Assert.Contains(string.Format(SdkVersionMessageFormat, expectedSdkVersion), result.StdOut);
                    Assert.Contains(
                        $"{ManifestFilePropertyKeys.DotNetCoreRuntimeVersion}=\"{runtimeVersion}",
                        result.StdOut);
                    Assert.Contains(
                        $"{ManifestFilePropertyKeys.DotNetCoreSdkVersion}=\"{expectedSdkVersion}",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void BuildsApplication_IgnoresRuntimeVersionBasedSdkVersion_AndUsesSdkVersionSpecifiedInGlobalJson()
        {
            // Here we are testing building a 2.1 runtime version app with a 3.1 sdk version

            // Arrange
            var expectedSdkVersion = "3.1.201";
            var globalJsonTemplate = @"
            {
                ""sdk"": {
                    ""version"": ""#version#"",
                    ""rollForward"": ""Disable"",
                }
            }";
            var globalJsonContent = globalJsonTemplate.Replace("#version#", expectedSdkVersion);
            var appName = "NetCoreApp21WebApp";
            var runtimeVersion = "2.1";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";

            // Create a global.json in host's app directory so that it can be present in container directory
            File.WriteAllText(
                Path.Combine(volume.MountedHostDir, DotNetCoreConstants.GlobalJsonFileName),
                globalJsonContent);

            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
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
                    Assert.Contains(string.Format(SdkVersionMessageFormat, expectedSdkVersion), result.StdOut);
                    Assert.Contains(
                        $"{ManifestFilePropertyKeys.DotNetCoreRuntimeVersion}=\"{runtimeVersion}",
                        result.StdOut);
                    Assert.Contains(
                        $"{ManifestFilePropertyKeys.DotNetCoreSdkVersion}=\"{expectedSdkVersion}",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }
    }
}
