// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildImage.Tests;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;
using Settings = Microsoft.Oryx.Tests.Common.Settings;

namespace Oryx.BuildImage.Tests.Node
{
    public class NodeAppOutputDirTest : NodeJSSampleAppsTestBase
    {
        public NodeAppOutputDirTest(ITestOutputHelper output) : base(output)
        {
        }

        [Theory, Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        // Temporarily blocking Angular 14 app: Work item 1565890
        // [InlineData("angular14", "dist")]
        // Temporarily blocking next app as next build is failing accross npm
        // [InlineData("blog-starter-nextjs", ".next")]
        // [InlineData("hackernews-nuxtjs", ".nuxt")]
        // Temporarily blocking gastbysample app after node default version bumped to 16: #1715134
        // [InlineData("gatsbysample", "public")]
        [InlineData("angular14", "dist", NodeVersions.Node18Version)]
        [InlineData("angular14", "dist", NodeVersions.Node20Version)]
        [InlineData("angular14", "dist", NodeVersions.Node22Version)]
        [InlineData("gatsby-sample", "public", NodeVersions.Node18Version)]
        [InlineData("gatsby-sample", "public", NodeVersions.Node20Version)]
        [InlineData("gatsby-sample", "public", NodeVersions.Node22Version)]
        [InlineData("blog-starter-nextjs", ".next", NodeVersions.Node18Version)]
        [InlineData("blog-starter-nextjs", ".next", NodeVersions.Node20Version)]
        [InlineData("blog-starter-nextjs", ".next", NodeVersions.Node22Version)]
        [InlineData("hackernews-nuxtjs", ".nuxt", NodeVersions.Node18Version)]
        [InlineData("hackernews-nuxtjs", ".nuxt", NodeVersions.Node20Version)]
        [InlineData("hackernews-nuxtjs", ".nuxt", NodeVersions.Node22Version)]
        [InlineData("vue-sample", "dist", NodeVersions.Node18Version)]
        [InlineData("vue-sample", "dist", NodeVersions.Node20Version)]
        [InlineData("vue-sample", "dist", NodeVersions.Node22Version)]
        [InlineData("create-react-app-sample", "build", NodeVersions.Node18Version)]
        [InlineData("create-react-app-sample", "build", NodeVersions.Node20Version)]
        [InlineData("create-react-app-sample", "build", NodeVersions.Node22Version)]
        [InlineData("hexo-sample", "public", NodeVersions.Node18Version)]
        [InlineData("hexo-sample", "public", NodeVersions.Node20Version)]
        [InlineData("hexo-sample", "public", NodeVersions.Node22Version)]
        public void BuildsApp_AndAddsOutputDirToManifestFile_WithBullseyeBasedImages(string appName, string expectedOutputDirPath, string version)
        {
            // Arrange
            var volume = DockerVolume.CreateMirror(
                Path.Combine(_hostSamplesDir, "nodejs", appName));
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var script = new ShellScriptBuilder()
                .AddCommand("node -v")
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} --platform-version {version}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.OsTypeFileName}")
                .AddStringExistsInFileCheck(
                $"{NodeManifestFilePropertyKeys.OutputDirPath}=\"{expectedOutputDirPath}\"",
                $"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetBuildImage(ImageTestHelperConstants.GitHubActionsBullseye),
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
        [Trait("build-image", "github-actions-debian-bookworm")]
        // Temporarily blocking Angular 14 app: Work item 1565890
        // [InlineData("angular14", "dist")]
        // Temporarily blocking next app as next build is failing accross npm
        // [InlineData("blog-starter-nextjs", ".next")]
        // [InlineData("hackernews-nuxtjs", ".nuxt")]
        // Temporarily blocking gastbysample app after node default version bumped to 16: #1715134
        // [InlineData("gatsbysample", "public")]
        [InlineData("angular14", "dist", NodeVersions.Node18Version)]
        [InlineData("angular14", "dist", NodeVersions.Node20Version)]
        [InlineData("angular14", "dist", NodeVersions.Node22Version)]
        [InlineData("gatsby-sample", "public", NodeVersions.Node18Version)]
        [InlineData("gatsby-sample", "public", NodeVersions.Node20Version)]
        [InlineData("gatsby-sample", "public", NodeVersions.Node22Version)]
        [InlineData("blog-starter-nextjs", ".next", NodeVersions.Node18Version)]
        [InlineData("blog-starter-nextjs", ".next", NodeVersions.Node20Version)]
        [InlineData("blog-starter-nextjs", ".next", NodeVersions.Node22Version)]
        [InlineData("hackernews-nuxtjs", ".nuxt", NodeVersions.Node18Version)]
        [InlineData("hackernews-nuxtjs", ".nuxt", NodeVersions.Node20Version)]
        [InlineData("hackernews-nuxtjs", ".nuxt", NodeVersions.Node22Version)]
        [InlineData("vue-sample", "dist", NodeVersions.Node18Version)]
        [InlineData("vue-sample", "dist", NodeVersions.Node20Version)]
        [InlineData("vue-sample", "dist", NodeVersions.Node22Version)]
        [InlineData("create-react-app-sample", "build", NodeVersions.Node18Version)]
        [InlineData("create-react-app-sample", "build", NodeVersions.Node20Version)]
        [InlineData("create-react-app-sample", "build", NodeVersions.Node22Version)]
        [InlineData("hexo-sample", "public", NodeVersions.Node18Version)]
        [InlineData("hexo-sample", "public", NodeVersions.Node20Version)]
        [InlineData("hexo-sample", "public", NodeVersions.Node22Version)]
        public void BuildsApp_AndAddsOutputDirToManifestFile_WithBookwormBasedImages(string appName, string expectedOutputDirPath, string version)
        {
            // Arrange
            var volume = DockerVolume.CreateMirror(
                Path.Combine(_hostSamplesDir, "nodejs", appName));
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var script = new ShellScriptBuilder()
                .AddCommand("node -v")
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} --platform-version {version}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.OsTypeFileName}")
                .AddStringExistsInFileCheck(
                $"{NodeManifestFilePropertyKeys.OutputDirPath}=\"{expectedOutputDirPath}\"",
                $"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetBuildImage(ImageTestHelperConstants.GitHubActionsBookworm),
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