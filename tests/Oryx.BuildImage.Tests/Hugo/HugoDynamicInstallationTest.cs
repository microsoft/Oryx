// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator.Hugo;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Microsoft.Oryx.BuildScriptGenerator.Go;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class HugoDynamicInstallationTest : HugoSampleAppsTestBase
    {
        private readonly string DefaultInstallationRootDir = HugoConstants.InstalledHugoVersionsDir;

        public HugoDynamicInstallationTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact, Trait("category", "jamstack")]
        public void PipelineTestInvocationJamstack()
        {
            var imageTestHelper = new ImageTestHelper();
            InstallsHugoVersionDynamically_UsingEnvironmentVariable_AndBuildsApp(imageTestHelper.GetAzureFunctionsJamStackBuildImage());
        }

        [Fact, Trait("category", "githubactions")]
        public void PipelineTestInvocationGithubactions()
        {
            var imageTestHelper = new ImageTestHelper();
            InstallsHugoVersionDynamically_UsingEnvironmentVariable_AndBuildsApp(imageTestHelper.GetGitHubActionsBuildImage());
        }

        [Fact, Trait("category", "cli-builder-bullseye")]
        public void PipelineTestInvocationCliBuilderBullseye()
        {
            var imageTestHelper = new ImageTestHelper();
            InstallsHugoVersionDynamically_UsingEnvironmentVariable_AndBuildsApp(imageTestHelper.GetCliBuilderImage(ImageTestHelperConstants.CliBuilderBullseyeTag));
        }

        private void InstallsHugoVersionDynamically_UsingEnvironmentVariable_AndBuildsApp(string imageName)
        {
            // Please note:
            // This test method has at least 1 wrapper function that pases the imageName parameter.

            // Arrange
            var hugoVersion = "0.59.1";
            var installationDir = $"{BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot}/hugo/{hugoVersion}";
            var appName = SampleAppName;
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable("HUGO_VERSION", hugoVersion)
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
                        $"Hugo Static Site Generator v{hugoVersion}",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void DynamicInstall_ReInstallsSdk_IfSentinelFileIsNotPresent()
        {
            // Arrange
            var hugoVersion = "0.70.0"; //NOTE: use the full version so that we know the install directory path
            var installationDir = $"{BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot}/hugo/{hugoVersion}";
            var sentinelFile = $"{installationDir}/{SdkStorageConstants.SdkDownloadSentinelFileName}";
            var appName = SampleAppName;
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var buildCmd = $"{appDir} --platform {HugoConstants.PlatformName} --platform-version {hugoVersion} -o {appOutputDir}";
            var script = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
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
                        $"Hugo Static Site Generator v{hugoVersion}",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "latest")]
        public void BuildsApplication_ByDynamicallyInstalling_IntoCustomDynamicInstallationDir()
        {
            // Arrange
            var hugoVersion = "0.59.1";
            var expectedDynamicInstallRootDir = "/foo/bar";
            var appName = SampleAppName;
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable("HUGO_VERSION", hugoVersion)
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} " +
                $"--dynamic-install-root-dir {expectedDynamicInstallRootDir}")
                .AddDirectoryExistsCheck(
                $"{expectedDynamicInstallRootDir}/{HugoConstants.PlatformName}/{hugoVersion}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetBuildImage(),
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
                        $"Hugo Static Site Generator v{hugoVersion}",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "latest")]
        public void BuildsApplication_ByDynamicallyInstallingIntoCustomDynamicInstallationDir()
        {
            // Arrange
            var hugoVersion = "0.96.0";
            var expectedDynamicInstallRootDir = "/foo/bar";
            var appName = SampleAppName;
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable("HUGO_VERSION", hugoVersion)
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} " +
                $"--dynamic-install-root-dir {expectedDynamicInstallRootDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetBuildImage(),
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

        private string GetSnippetToCleanUpExistingInstallation()
        {
            return $"rm -rf {DefaultInstallationRootDir}; mkdir -p {DefaultInstallationRootDir}";
        }

        [Fact, Trait("category", "jamstack")]
        public void JamStackImageHasGoLangInstalled()
        {
            // Arrange
            var expectedText = GoVersions.GoVersion;
            var appName = SampleAppName;
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir}")
                .ToString();

            // Act
            var imageName = _imageHelper.GetAzureFunctionsJamStackBuildImage();
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
                    Assert.Contains(expectedText, result.StdOut);
                },
                result.GetDebugInfo());
        }
    }
}
