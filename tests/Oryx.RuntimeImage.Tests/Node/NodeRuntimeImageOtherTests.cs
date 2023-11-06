// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    public class NodeRuntimeImageOtherTests : NodeRuntimeImageTestBase
    {
        public NodeRuntimeImageOtherTests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Theory]
        [Trait("category", "runtime-buster")]
        [InlineData("14", NodeVersions.Node14Version)]
        [InlineData("16", NodeVersions.Node16Version)]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void NodeVersionMatchesBusterImageName(string version, string nodeVersion)
        {
            // Arrange & Act
            var expectedNodeVersion = "v" + nodeVersion;
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", version, ImageTestHelperConstants.OsTypeDebianBuster),
                CommandToExecuteOnRun = "node",
                CommandArguments = new[] { "--version" }
            });

            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedNodeVersion, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [Trait("category", "runtime-bullseye")]
        [InlineData("14", NodeVersions.Node14Version)]
        [InlineData("16", NodeVersions.Node16Version)]
        [InlineData("18", NodeVersions.Node18Version)]
        [InlineData("20", NodeVersions.Node20Version)]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void NodeVersionMatchesBullseyeImageName(string version, string nodeVersion)
        {
            // Arrange & Act
            var expectedNodeVersion = "v" + nodeVersion;
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", version, ImageTestHelperConstants.OsTypeDebianBullseye),
                CommandToExecuteOnRun = "node",
                CommandArguments = new[] { "--version" }
            });

            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedNodeVersion, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [Trait("category", "runtime-bookworm")]
        [InlineData("20", NodeVersions.Node20Version)]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void NodeVersionMatchesBookwormImageName(string version, string nodeVersion)
        {
            // Arrange & Act
            var expectedNodeVersion = "v" + nodeVersion;
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", version, ImageTestHelperConstants.OsTypeDebianBookworm),
                CommandToExecuteOnRun = "node",
                CommandArguments = new[] { "--version" }
            });

            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedNodeVersion, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [Trait("category", "runtime-bookworm")]
        [MemberData(
            nameof(TestValueGenerator.GetBookwormNodeVersions),
            MemberType = typeof(TestValueGenerator))]
        public void HasExpected_Global_Bookworm_Node_Module_Path(string nodeVersion, string osType)
        {
            // Arrange & Act
            var script = new ShellScriptBuilder()
                .AddCommand("npm root --quiet -g")
                .ToString();

            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(FilePaths.NodeGlobalModulesPath, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [Trait("category", "runtime-bullseye")]
        [MemberData(
            nameof(TestValueGenerator.GetBullseyeNodeVersions),
            MemberType = typeof(TestValueGenerator))]
        public void HasExpected_Global_Bullseye_Node_Module_Path(string nodeVersion, string osType)
        {
            // Arrange & Act
            var script = new ShellScriptBuilder()
                .AddCommand("npm root --quiet -g")
                .ToString();

            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(FilePaths.NodeGlobalModulesPath, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [Trait("category", "runtime-buster")]
        [MemberData(
            nameof(TestValueGenerator.GetBusterNodeVersions),
            MemberType = typeof(TestValueGenerator))]
        public void HasExpected_Global_Buster_Node_Module_Path(string nodeVersion, string osType)
        {
            // Arrange & Act
            var script = new ShellScriptBuilder()
                .AddCommand("npm root --quiet -g")
                .ToString();

            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(FilePaths.NodeGlobalModulesPath, actualOutput);
                },
                result.GetDebugInfo());
        }


        [Fact]
        [Trait("category", "runtime-bullseye")]
        public void GeneratedScript_CanRunStartupScriptsFromAppRoot()
        {
            // Arrange
            const int exitCodeSentinel = 222;
            var appPath = "/tmp/app";
            var script = new ShellScriptBuilder()
                .CreateDirectory(appPath)
                .CreateFile(appPath + "/entry.sh", $"exit {exitCodeSentinel}")
                .AddCommand("oryx create-script -userStartupCommand entry.sh -appPath " + appPath)
                .AddCommand(". ./run.sh") // Source the default output path
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", "14", ImageTestHelperConstants.OsTypeDebianBullseye),
                CommandToExecuteOnRun = "/bin/sh",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(() => Assert.Equal(exitCodeSentinel, result.ExitCode), result.GetDebugInfo());
        }

        [Theory(Skip = "Investigating debugging using pm2")]
        [Trait("category", "runtime-buster")]
        [MemberData(
            nameof(TestValueGenerator.GetBusterNodeVersions_SupportDebugging),
            MemberType = typeof(TestValueGenerator))]
        public async Task RunBusterNodeAppUsingProcessJson_withDebuggingAsync(string nodeVersion, string osType)
        {
            var appName = "express-process-json";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var dir = volume.ContainerDir;
            int containerDebugPort = 8080;

            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"cd {dir}/app")
                .AddCommand("npm install")
                .AddCommand("cd ..")
                .AddCommand($"oryx create-script -remoteDebug -debugPort={containerDebugPort}")
                .AddCommand("./run.sh")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                port: containerDebugPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", runAppScript },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Say It Again", data);
                },
                dockerCli: _dockerCli);

        }

        [Theory(Skip = "Investigating debugging using pm2")]
        [Trait("category", "runtime-bullseye")]
        [MemberData(
            nameof(TestValueGenerator.GetBullseyeNodeVersions_SupportDebugging),
            MemberType = typeof(TestValueGenerator))]
        public async Task RunBullseyeNodeAppUsingProcessJson_withDebuggingAsync(string nodeVersion, string osType)
        {
            var appName = "express-process-json";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var dir = volume.ContainerDir;
            int containerDebugPort = 8080;

            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"cd {dir}/app")
                .AddCommand("npm install")
                .AddCommand("cd ..")
                .AddCommand($"oryx create-script -remoteDebug -debugPort={containerDebugPort}")
                .AddCommand("./run.sh")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion, osType),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                port: containerDebugPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", runAppScript },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Say It Again", data);
                },
                dockerCli: _dockerCli);

        }
    }
}
