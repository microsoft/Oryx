// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
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
    public class NodeLegacyVersionsRuntimeImageTests : NodeRuntimeImageTestBase
    {
        public NodeLegacyVersionsRuntimeImageTests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [SkippableTheory]
        [MemberData(nameof(TestValueGenerator.GetLegacyNodeVersions), MemberType = typeof(TestValueGenerator))]
        public void NodeImage_Contains_VersionAndCommit_Information(string version)
        {
            // We can't always rely on git commit ID as env variable in case build context is not correctly passed
            // so we should check agent_os environment variable to know if the build is happening in azure devops agent
            // or locally, locally we need to skip this test
            var agentOS = Environment.GetEnvironmentVariable("AGENT_OS");
            Skip.If(string.IsNullOrEmpty(agentOS));

            // Arrange
            var gitCommitID = GitHelper.GetCommitID();
            var buildNumber = Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");
            var expectedOryxVersion = string.Concat(Settings.OryxVersion, buildNumber);

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", version),
                CommandToExecuteOnRun = "oryx",
                CommandArguments = new[] { "version" }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.NotNull(result.StdErr);
                    Assert.DoesNotContain(".unspecified, Commit: unspecified", result.StdOut);
                    Assert.Contains(gitCommitID, result.StdOut);
                    Assert.Contains(expectedOryxVersion, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("4.4", "4.4.7")]
        [InlineData("4.5", "4.5.0")]
        [InlineData("4.8", "4.8.7")]
        [InlineData("6", NodeVersions.Node6Version)]
        [InlineData("6.2", "6.2.2")]
        [InlineData("6.6", "6.6.0")]
        [InlineData("6.9", "6.9.5")]
        [InlineData("6.10", "6.10.3")]
        [InlineData("6.11", "6.11.5")]
        [InlineData("8", NodeVersions.Node8Version)]
        [InlineData("8.2", "8.2.1")]
        [InlineData("8.11", "8.11.4")]
        [InlineData("9.4", "9.4.0")]
        [InlineData("10", NodeVersions.Node10Version)]
        [InlineData("10.10", "10.10.0")]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void NodeVersionMatchesImageName(string nodeTag, string nodeVersion)
        {
            // Arrange & Act
            var expectedNodeVersion = "v" + nodeVersion;
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", nodeTag),
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
        [InlineData("10")]
        [InlineData("10.1")]
        [InlineData("10.10")]
        [InlineData("10.12")]
        [InlineData("10.14")]
        public void Node10ImageContains_Correct_NPM_Version(string imageTag)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", imageTag),
                CommandToExecuteOnRun = "npm",
                CommandArguments = new[] { "-v" }
            });

            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(NodeVersions.NpmVersion, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [MemberData(
            nameof(TestValueGenerator.GetLegacyNodeVersions),
            MemberType = typeof(TestValueGenerator))]
        public void HasExpected_Global_Node_Module_Path(string nodeVersion)
        {
            // Arrange & Act
            var script = new ShellScriptBuilder()
                .AddCommand("npm root --quiet -g")
                .ToString();

            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", nodeVersion),
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

        [Theory(Skip = "Legacy node versions are out of support")]
        [MemberData(nameof(TestValueGenerator.GetLegacyNodeVersions), MemberType = typeof(TestValueGenerator))]
        public async Task RunNodeAppUsingProcessJson(string nodeVersion)
        {

            var appName = "express-process-json";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var dir = volume.ContainerDir;
            int containerPort = 80;

            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"cd {dir}/app")
                .AddCommand("npm install")
                .AddCommand("cd ..")
                .AddCommand($"oryx create-script -bindPort {containerPort} -usePM2")
                .AddCommand("./run.sh")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                containerPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", runAppScript },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Equal("Hello World from express!", data);
                },
                dockerCli: _dockerCli);

        }

        [Theory(Skip = "Legacy node versions are out of support")]
        [MemberData(nameof(TestValueGenerator.GetLegacyNodeVersions), MemberType = typeof(TestValueGenerator))]
        public async Task RunNodeAppUsingConfigYml(string nodeVersion)
        {

            var appName = "express-config-yaml";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var dir = volume.ContainerDir;
            int containerPort = 80;

            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"cd {dir}/app")
                .AddCommand("npm install")
                .AddCommand("cd ..")
                .AddCommand($"oryx create-script -bindPort {containerPort} -userStartupCommand config.yml -usePM2")
                .AddCommand("./run.sh")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                containerPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", runAppScript },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Equal("Hello World from express!", data);
                },
                dockerCli: _dockerCli);
        }

        [Theory(Skip = "Legacy node versions are out of support")]
        [MemberData(nameof(TestValueGenerator.GetLegacyNodeVersions), MemberType = typeof(TestValueGenerator))]
        public async Task RunNodeAppUsingConfigJs(string nodeVersion)
        {

            var appName = "express-config-js";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var dir = volume.ContainerDir;
            int containerPort = 80;

            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"cd {dir}/app")
                .AddCommand("npm install")
                .AddCommand("cd ..")
                .AddCommand($"oryx create-script -bindPort {containerPort} -usePM2")
                .AddCommand("./run.sh")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("node", nodeVersion),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                containerPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", runAppScript },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Equal("Hello World from express!", data);
                },
                dockerCli: _dockerCli);

        }
    }
}
