// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    [Trait("platform", "dotnet")]
    public class DotNetCoreDynamicInstallTest : SampleAppsTestBase
    {
        protected const string NetCoreApp11WebApp = "NetCoreApp11WebApp";
        protected const string NetCoreApp21WebApp = "NetCoreApp21.WebApp";
        protected const string NetCoreApp22WebApp = "NetCoreApp22WebApp";
        protected const string NetCoreApp30WebApp = "NetCoreApp30.WebApp";
        protected const string NetCoreApp30MvcApp = "NetCoreApp30.MvcApp";
        protected const string NetCoreApp31MvcApp = "NetCoreApp31.MvcApp";
        protected const string NetCoreApp50MvcApp = "NetCoreApp50MvcApp";
        protected const string NetCore6PreviewWebApp = "NetCore6PreviewWebApp";
        protected const string NetCore7PreviewMvcApp = "NetCore7PreviewMvcApp";
        protected const string DefaultWebApp = "DefaultWebApp";

        private DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "DotNetCore", sampleAppName));

        private readonly string SdkVersionMessageFormat = "Using .NET Core SDK Version: {0}";

        public DotNetCoreDynamicInstallTest(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData(NetCoreApp21WebApp, "2.1")]
        [InlineData(NetCoreApp31MvcApp, "3.1")]
        [InlineData(NetCoreApp50MvcApp, "5.0")]
        [InlineData(NetCore7PreviewMvcApp, "7.0.0-preview.1.22076.8")]
        public void BuildsApplication_ByDynamicallyInstallingSDKs(
            string appName,
            string runtimeVersion)
        {
            // Arrange
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var manifestFile = $"{appOutputDir}/{FilePaths.BuildManifestFileName}";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {DotNetCoreConstants.PlatformName} --platform-version {runtimeVersion}")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .AddFileExistsCheck(manifestFile)
                .AddCommand($"cat {manifestFile}")
                .ToString();
            var majorPart = runtimeVersion.Split('.')[0];
            var expectedSdkVersionPrefix = $"{majorPart}.";

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _restrictedPermissionsImageHelper.GetGitHubActionsBuildImage(),
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
                    ""rollForward"": ""Disable""
                }
            }";
            var globalJsonSdkVersion = "3.1.201";
            var globalJsonContent = globalJsonTemplate.Replace("#version#", globalJsonSdkVersion);
            var sentinelFile = $"{Constants.TemporaryInstallationDirectoryRoot}/{DotNetCoreConstants.PlatformName}/{globalJsonSdkVersion}/" +
                $"{SdkStorageConstants.SdkDownloadSentinelFileName}";
            var appName = NetCoreApp31MvcApp;
            var volume = CreateSampleAppVolume(appName);
            // Create a global.json in host's app directory so that it can be present in container directory
            File.WriteAllText(
                Path.Combine(volume.MountedHostDir, DotNetCoreConstants.GlobalJsonFileName),
                globalJsonContent);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var buildCmd = $"{appDir} -i /tmp/int -o {appOutputDir}";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
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
                    ""rollForward"": ""Disable""
                }
            }";
            var globalJsonContent = globalJsonTemplate.Replace("#version#", expectedSdkVersion);
            var appName = NetCoreApp21WebApp;
            var runtimeVersion = "2.1";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";

            // Create a global.json in host's app directory so that it can be present in container directory
            File.WriteAllText(
                Path.Combine(volume.MountedHostDir, DotNetCoreConstants.GlobalJsonFileName),
                globalJsonContent);
            var manifestFile = $"{appOutputDir}/{FilePaths.BuildManifestFileName}";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {DotNetCoreConstants.PlatformName} --platform-version {runtimeVersion} --log-file log.txt")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .AddFileExistsCheck(manifestFile)
                .AddCommand($"cat {manifestFile}")
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
                    ""rollForward"": ""Disable""
                }
            }";
            var globalJsonContent = globalJsonTemplate.Replace("#version#", expectedSdkVersion);
            var appName = NetCoreApp21WebApp;
            var runtimeVersion = "2.1";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";

            // Create a global.json in host's app directory so that it can be present in container directory
            File.WriteAllText(
                Path.Combine(volume.MountedHostDir, DotNetCoreConstants.GlobalJsonFileName),
                globalJsonContent);
            var manifestFile = $"{appOutputDir}/{FilePaths.BuildManifestFileName}";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .AddFileExistsCheck(manifestFile)
                .AddCommand($"cat {manifestFile}")
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
        public void BuildsApplication_UsingPreviewVersionOfSdk()
        {
            // Arrange
            var expectedSdkVersion = "5.0.100-preview.3.20216.6";
            var globalJsonTemplate = @"
            {
                ""sdk"": {
                    ""version"": ""#version#"",
                    ""rollForward"": ""Disable""
                }
            }";
            var globalJsonContent = globalJsonTemplate.Replace("#version#", expectedSdkVersion);
            var appName = NetCoreApp50MvcApp;
            var runtimeVersion = "5.0";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";

            // Create a global.json in host's app directory so that it can be present in container directory
            File.WriteAllText(
                Path.Combine(volume.MountedHostDir, DotNetCoreConstants.GlobalJsonFileName),
                globalJsonContent);

            var manifestFile = $"{appOutputDir}/{FilePaths.BuildManifestFileName}";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .AddFileExistsCheck(manifestFile)
                .AddCommand($"cat {manifestFile}")
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
        public void BuildsAppAfterInstallingAllRequiredPlatforms()
        {
            // Arrange
            var appName = "dotNetCoreReactApp";
            var hostDir = Path.Combine(_hostSamplesDir, "multilanguage", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildScript = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --platform {DotNetCoreConstants.PlatformName} --platform-version 3.1")
                .AddFileExistsCheck($"{appOutputDir}/dotNetCoreReactApp.dll")
                .AddDirectoryExistsCheck($"{appOutputDir}/ClientApp/build")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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
                    Assert.Contains("Using .NET Core SDK Version: ", result.StdOut);
                    Assert.Contains("react-scripts build", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void BuildsApplication_ByDynamicallyInstallingSDKs_IntoCustomDynamicInstallationDir()
        {
            // Here we are testing building a 2.1 runtime version app with a 3.1 sdk version

            // Arrange
            var expectedSdkVersion = "3.1.201";
            var globalJsonTemplate = @"
            {
                ""sdk"": {
                    ""version"": ""#version#"",
                    ""rollForward"": ""Disable""
                }
            }";
            var globalJsonContent = globalJsonTemplate.Replace("#version#", expectedSdkVersion);
            var appName = NetCoreApp21WebApp;
            var runtimeVersion = "2.1";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var expectedDynamicInstallRootDir = "/foo/bar";
            // Create a global.json in host's app directory so that it can be present in container directory
            File.WriteAllText(
                Path.Combine(volume.MountedHostDir, DotNetCoreConstants.GlobalJsonFileName),
                globalJsonContent);
            var manifestFile = $"{appOutputDir}/{FilePaths.BuildManifestFileName}";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} --dynamic-install-root-dir {expectedDynamicInstallRootDir}")
                .AddDirectoryExistsCheck(
                $"{expectedDynamicInstallRootDir}/{DotNetCoreConstants.PlatformName}/{expectedSdkVersion}")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .AddFileExistsCheck(manifestFile)
                .AddCommand($"cat {manifestFile}")
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

        [Theory]
        [InlineData(NetCoreApp30MvcApp, "3.0", DotNetCoreSdkVersions.DotNetCore30SdkVersion)]
        public void BuildsApplication_SetLinksCorrectly_ByDynamicallyInstallingSDKs(
            string appName,
            string runtimeVersion,
            string sdkVersion)
        {
            // Arrange
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var manifestFile = $"{appOutputDir}/{FilePaths.BuildManifestFileName}";
            var preInstalledSdkLink = $"/home/codespace/.dotnet/sdk";
            var script = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .AddDirectoryExistsCheck($"/home/codespace/.dotnet/")
                .AddLinkExistsCheck($"{preInstalledSdkLink}/{DotNetCoreSdkVersions.DotNetCore31SdkVersion}")
                .AddLinkExistsCheck($"{preInstalledSdkLink}/{DotNetCoreSdkVersions.DotNet50SdkVersion}")
                .AddLinkDoesNotExistCheck($"{preInstalledSdkLink}/{sdkVersion}")
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {DotNetCoreConstants.PlatformName} --platform-version {runtimeVersion}")
                .AddFileExistsCheck($"{appOutputDir}/{appName}.dll")
                .AddDirectoryExistsCheck($"/opt/dotnet/{sdkVersion}")
                .AddLinkExistsCheck($"{preInstalledSdkLink}/{sdkVersion}")
                .AddFileExistsCheck(manifestFile)
                .AddCommand($"cat {manifestFile}")
                .AddCommand("/home/codespace/.dotnet/dotnet --list-sdks")
                .ToString();
            var majorPart = runtimeVersion.Split('.')[0];
            var expectedSdkVersionPrefix = $"{majorPart}.";

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetVsoBuildImage("vso-focal"),
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
                        $"{ManifestFilePropertyKeys.DotNetCoreSdkVersion}=\"{expectedSdkVersionPrefix}",
                        result.StdOut);
                    Assert.Contains($"{DotNetCoreSdkVersions.DotNetCore31SdkVersion} [/home/codespace/.dotnet/sdk]", result.StdOut);
                    Assert.Contains($"{DotNetCoreSdkVersions.DotNet50SdkVersion} [/home/codespace/.dotnet/sdk]", result.StdOut);
                    Assert.Contains($"{sdkVersion} [/home/codespace/.dotnet/sdk]", result.StdOut);
                },
                result.GetDebugInfo());
        }
    }
}
