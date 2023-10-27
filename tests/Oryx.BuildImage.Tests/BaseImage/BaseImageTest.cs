// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Tests.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class BaseImageTest : SampleAppsTestBase
    {
        protected const string NetCoreApp31MvcApp = "NetCoreApp31.MvcApp";
        protected const string NetCore8PreviewMvcApp = "NetCore8PreviewMvcApp";

        public BaseImageTest(ITestOutputHelper output) : base(output)
        {
        }

        private DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "DotNetCore", sampleAppName));

        private readonly string OsTypeMessage = "Warning: DEBIAN_FLAVOR environment variable not found. Falling back to debian flavor in the";
        private readonly string NoDebianFlavorError = "Error: Image debian flavor not found in";
        private readonly string DetectedDebianFlavorMessage = "Detected image debian flavor:";

        public static TheoryData<string, string, string, string> VersionAndNoEnvBaseImageNameData
        {
            get
            {
                var data = new TheoryData<string, string, string, string>();
                var imageHelper = new ImageTestHelper();

                // stretch
                data.Add(
                    DotNetCoreRunTimeVersions.NetCoreApp31,
                    NetCoreApp31MvcApp,
                    imageHelper.GetGitHubActionsAsBaseBuildImage(),
                    "stretch");

                // buster
                data.Add(
                    DotNetCoreRunTimeVersions.NetCoreApp31,
                    NetCoreApp31MvcApp,
                    imageHelper.GetGitHubActionsAsBaseBuildImage(ImageTestHelperConstants.GitHubActionsBusterBase),
                    "buster");

                // bullseye
                data.Add(
                    DotNetCoreRunTimeVersions.NetCoreApp80,
                    NetCore8PreviewMvcApp,
                    imageHelper.GetGitHubActionsAsBaseBuildImage(ImageTestHelperConstants.GitHubActionsBullseyeBase),
                    "bullseye");

                // bookworm
                data.Add(
                    DotNetCoreRunTimeVersions.NetCoreApp80,
                    NetCore8PreviewMvcApp,
                    imageHelper.GetGitHubActionsAsBaseBuildImage(ImageTestHelperConstants.GitHubActionsBookwormBase),
                    "bookworm");

                return data;
            }
        }

        /// <summary>
        /// This test uses an image that has Oryx githubactions as a base image, but does not provide 
        /// the DEBIAN_FLAVOR environment variable to the final image.
        /// This means that the Oryx CLI must parse the <see cref="FilePaths.OsTypeFileName"/> file
        /// to provide the debian flavor.
        /// </summary>
        [Theory, Trait("category", "dotnetcore-dynamic")]
        [MemberData(nameof(VersionAndNoEnvBaseImageNameData))]
        public void BuildsApplication_WithOryxBaseImage_UsingOsTypeFile(
            string runtimeVersion,
            string appName,
            string imageName,
            string expectedDebianFlavor)
        {
           var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var manifestFile = $"{appOutputDir}/{FilePaths.BuildManifestFileName}";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {DotNetCoreConstants.PlatformName} --platform-version {runtimeVersion} --debug")
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
                    Assert.Contains(OsTypeMessage, result.StdOut);
                    Assert.Contains($"{DetectedDebianFlavorMessage} {expectedDebianFlavor}", result.StdOut);
                },
                result.GetDebugInfo());
        }

        public static TheoryData<string, string, string, string> VersionAndWithEnvBaseImageNameData
        {
            get
            {
                var data = new TheoryData<string, string, string, string>();
                var imageHelper = new ImageTestHelper();

                // stretch
                data.Add(
                    DotNetCoreRunTimeVersions.NetCoreApp31,
                    NetCoreApp31MvcApp,
                    imageHelper.GetGitHubActionsAsBaseWithEnvBuildImage(),
                    "stretch");

                //buster
                data.Add(
                    DotNetCoreRunTimeVersions.NetCoreApp31,
                    NetCoreApp31MvcApp,
                    imageHelper.GetGitHubActionsAsBaseWithEnvBuildImage(ImageTestHelperConstants.GitHubActionsBusterBaseWithEnv),
                    "buster");


                //bullseye
                data.Add(
                    DotNetCoreRunTimeVersions.NetCoreApp31,
                    NetCoreApp31MvcApp,
                    imageHelper.GetGitHubActionsAsBaseWithEnvBuildImage(ImageTestHelperConstants.GitHubActionsBullseyeBaseWithEnv),
                    "bullseye");

                return data;
            }
        }

        /// <summary>
        /// This test uses an image that has Oryx githubactions as a base image, and provides
        /// the DEBIAN_FLAVOR environment variable to the final image.
        /// This means that the Oryx CLI should be able to use that environment variable instead
        /// of the <see cref="FilePaths.OsTypeFileName"/> file to provide the debian flavor.
        /// </summary>
        [Theory, Trait("category", "dotnetcore-dynamic")]
        [MemberData(nameof(VersionAndWithEnvBaseImageNameData))]
        public void BuildsApplication_WithOryxBaseImage_UsingDebianFlavorEnv(
            string runtimeVersion,
            string appName,
            string imageName,
            string expectedDebianFlavor)
        {
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {DotNetCoreConstants.PlatformName} --platform-version {runtimeVersion} --debug")
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
                    Assert.DoesNotContain(OsTypeMessage, result.StdOut);
                    Assert.Contains($"{DetectedDebianFlavorMessage} {expectedDebianFlavor}", result.StdOut);
                },
                result.GetDebugInfo());
        }

        /// <summary>
        /// This test uses an image that has Oryx githubactions as a base image. We manually remove the
        /// DEBIAN_FLAVOR environment variable and <see cref="FilePaths.OsTypeFileName"/>.
        /// This means that the Oryx CLI cannot determine the debian flavor, and should exit with an error.
        /// </summary>
        [Theory, Trait("category", "dotnetcore-dynamic")]
        [MemberData(nameof(VersionAndWithEnvBaseImageNameData))]
        public void FailsToBuildApplication_WithOryxBaseImage_NoDebianFlavor(
            string runtimeVersion,
            string appName,
            string imageName,
            string expectedDebianFlavor)
        {
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var manifestFile = $"{appOutputDir}/{FilePaths.BuildManifestFileName}";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(ExtVarNames.DebianFlavor, string.Empty) // remove debian flavor env var
                .AddCommand($"rm /opt/oryx/{FilePaths.OsTypeFileName}") // remove os type file
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {DotNetCoreConstants.PlatformName} --platform-version {runtimeVersion} --debug")
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
                    Assert.False(result.IsSuccess);
                    Assert.Contains(NoDebianFlavorError, result.StdErr);
                    Assert.DoesNotContain($"{DetectedDebianFlavorMessage} {expectedDebianFlavor}", result.StdOut);
                },
                result.GetDebugInfo());
        }
    }
}
