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

        [Theory]
        [InlineData("angular14", "dist")]
        // Temporarily blocking next app as next build is failing accross npm
        // [InlineData("blog-starter-nextjs", ".next")]
        [InlineData("hackernews-nuxtjs", ".nuxt")]
        [InlineData("vue-sample", "dist")]
        [InlineData("create-react-app-sample", "build")]
        [InlineData("gatsbysample", "public")]
        [InlineData("hexo-sample", "public")]
        public void BuildsApp_AndAddsOutputDirToManifestFile(string appName, string expectedOutputDirPath)
        {
            // Arrange
            var volume = DockerVolume.CreateMirror(
                Path.Combine(_hostSamplesDir, "nodejs", appName));
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddStringExistsInFileCheck(
                $"{NodeManifestFilePropertyKeys.OutputDirPath}=\"{expectedOutputDirPath}\"",
                $"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetAzureFunctionsJamStackBuildImage(),
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