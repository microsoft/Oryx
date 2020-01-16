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
    public class DotNetCoreDynamicInstallationTest : SampleAppsTestBase
    {
        private readonly string DefaultRuntimesRootDir = "/opt/dotnet/runtimes";
        private readonly string DefaultSdksRootDir = "/opt/dotnet/sdks";

        public DotNetCoreDynamicInstallationTest(ITestOutputHelper output) : base(output)
        {
        }

        private DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "DotNetCore", sampleAppName));

        private readonly string SdkVersionMessageFormat = "Using .NET Core SDK Version: {0}";

        [Fact]
        public void BuildsUsingMaximumSatisfyingVersion()
        {
            // NOTE: Here we explicitly provide the 'major' version only. This means in the end latest minor+patch
            // should be used, which is what this test verifies.

            // Arrange
            var appName = "NetCoreApp21WebApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/NetCoreApp21WebApp-output";
            var script = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .SetEnvironmentVariable(SdkStorageConstants.UseLatestVersion, "true")
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddBuildCommand($"{appDir} -o {appOutputDir} --platform dotnet --platform-version 2")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetTestSlimBuildImage(),
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
        public void Builds_NetCore21App_UsingNetCore21_DotNetSdkVersion()
        {
            // Arrange
            var appName = "NetCoreApp21WebApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/NetCoreApp21WebApp-output";
            var script = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .SetEnvironmentVariable(SdkStorageConstants.UseLatestVersion, "true")
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetTestSlimBuildImage(),
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
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .SetEnvironmentVariable(SdkStorageConstants.UseLatestVersion, "true")
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetTestSlimBuildImage(),
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
        public void Builds_NetCore31App_UsingNetCore31_DotNetSdkVersion()
        {
            // Arrange
            var appName = "NetCoreApp31.MvcApp";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/NetCoreApp31MvcApp-output";
            var script = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .SetEnvironmentVariable(SdkStorageConstants.UseLatestVersion, "true")
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetTestSlimBuildImage(),
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

        private string GetSnippetToCleanUpExistingInstallation()
        {
            return $"rm -rf {DefaultRuntimesRootDir}; mkdir -p {DefaultRuntimesRootDir}; " +
                $"rm -rf {DefaultSdksRootDir}; mkdir -p {DefaultSdksRootDir}";
        }
    }
}
