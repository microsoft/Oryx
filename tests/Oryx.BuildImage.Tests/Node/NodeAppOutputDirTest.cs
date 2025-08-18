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
        // Temporarily blocking Angular 14 app: Work item 1565890
        // [InlineData("angular14", "dist")]
        // Temporarily blocking next app as next build is failing accross npm
        // [InlineData("blog-starter-nextjs", ".next")]
        // [InlineData("hackernews-nuxtjs", ".nuxt")]
        // Temporarily blocking gastbysample app after node default version bumped to 16: #1715134
        // [InlineData("gatsbysample", "public")]
        [InlineData("vue-sample", "dist")]
        [InlineData("create-react-app-sample", "build")]
        [InlineData("hexo-sample", "public")]
        public void BuildsApp_AndAddsOutputDirToManifestFile(string appName, string expectedOutputDirPath)
        {
            // Arrange
            var version = "20.11.0";
            var volume = DockerVolume.CreateMirror(
                Path.Combine(_hostSamplesDir, "nodejs", appName));
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} --platform-version {version}")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.OsTypeFileName}")
                .AddStringExistsInFileCheck(
                $"{NodeManifestFilePropertyKeys.OutputDirPath}=\"{expectedOutputDirPath}\"",
                $"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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