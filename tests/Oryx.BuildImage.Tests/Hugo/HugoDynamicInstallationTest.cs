// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator.Hugo;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class HugoDynamicInstallationTest : HugoSampleAppsTestBase
    {
        private readonly string DefaultInstallationRootDir = HugoConstants.InstalledHugoVersionsDir;

        public HugoDynamicInstallationTest(ITestOutputHelper output) : base(output)
        {
        }

        public static TheoryData<string> ImageNameData
        {
            get
            {
                var imageTestHelper = new ImageTestHelper();
                var data = new TheoryData<string>();
                data.Add(imageTestHelper.GetAzureFunctionsJamStackBuildImage());
                data.Add(imageTestHelper.GetGitHubActionsBuildImage());
                return data;
            }
        }

        [Theory]
        [MemberData(nameof(ImageNameData))]
        public void GeneratesScript_AndBuilds(string imageName)
        {
            // Arrange
            var version = "0.59.1";
            var installationDir = $"{BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot}/hugo/{version}";
            var appName = SampleAppName;
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable("HUGO_VERSION", version)
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = imageName,
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
                        $"Hugo Static Site Generator v{version}",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void DynamicInstall_ReInstallsSdk_IfSentinelFileIsNotPresent()
        {
            // Arrange
            var version = "0.70.0"; //NOTE: use the full version so that we know the install directory path
            var installationDir = $"{BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot}/hugo/{version}";
            var sentinelFile = $"{installationDir}/{SdkStorageConstants.SdkDownloadSentinelFileName}";
            var appName = SampleAppName;
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var buildCmd = $"{appDir} --platform {HugoConstants.PlatformName} --platform-version {version} -o {appOutputDir}";
            var script = new ShellScriptBuilder()
                 .AddCommand(GetSnippetToCleanUpExistingInstallation())
                 .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                .AddBuildCommand(buildCmd)
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
                    Assert.Contains(
                        $"Hugo Static Site Generator v{version}",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        private string GetSnippetToCleanUpExistingInstallation()
        {
            return $"rm -rf {DefaultInstallationRootDir}; mkdir -p {DefaultInstallationRootDir}";
        }
    }
}
