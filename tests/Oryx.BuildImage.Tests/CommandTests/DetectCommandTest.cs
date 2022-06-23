// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildImage.Tests;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.BuildImage.Tests
{
    public class DetectCommandTest : SampleAppsTestBase
    {
        private DockerVolume CreateWebFrontEndVolume() => DockerVolume.CreateMirror(
            Path.Combine(_hostSamplesDir, "nodejs", "webfrontend"));

        public DetectCommandTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact, Trait("category", "githubactions")]
        public void DetectCommand_OutputsJsonFormat()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var subDir = Guid.NewGuid();
            var script = new ShellScriptBuilder()
                .AddCommand($"oryx detect {appDir} -o json")
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
                    Assert.Contains(
                         $"\"Platform\": \"{NodeConstants.PlatformName}\"",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }
    }
}
