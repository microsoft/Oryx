// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    public class NodeImagesTest : TestBase, IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _hostSamplesDir;
        private readonly string _tempRootDir;
        protected readonly HttpClient _httpClient = new HttpClient();

        public NodeImagesTest(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture) : base(output)
        {
            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
            _tempRootDir = testTempDirTestFixture.RootDirPath;
        }

        [SkippableTheory]
        [MemberData(nameof(TestValueGenerator.GetNodeVersions), MemberType = typeof(TestValueGenerator))]
        public void NodeImage_Contains_VersionAndCommit_Information(string version)
        {
            var agentOS = Environment.GetEnvironmentVariable("AGENT_OS");
            var gitCommitID = Environment.GetEnvironmentVariable("BUILD_SOURCEVERSION");
            var buildNumber = Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");
            var expectedOryxVersion = string.Concat(Settings.OryxVersion, buildNumber);

            // We can't always rely on git commit ID as env variable in case build context is not correctly passed
            // so we should check agent_os environment variable to know if the build is happening in azure devops agent 
            // or locally, locally we need to skip this test
            Skip.If(string.IsNullOrEmpty(agentOS));
            // Act
            var result = _dockerCli.Run(
                "oryxdevms/node-" + version + ":latest",
                commandToExecuteOnRun: "oryx",
                commandArguments: new[] { "--version" });
            // Assert
            RunAsserts(
                () =>
                {
                    Assert.False(result.IsSuccess);
                    Assert.NotNull(result.StdErr);
                    Assert.DoesNotContain(".unspecified, Commit: unspecified", result.StdErr);
                    Assert.Contains(gitCommitID, result.StdErr);
                    Assert.Contains(expectedOryxVersion, result.StdErr);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("4.4", "4.4.7")]
        [InlineData("4.5", "4.5.0")]
        [InlineData("4.8", "4.8.7")]
        [InlineData("6.2", "6.2.2")]
        [InlineData("6.6", "6.6.0")]
        [InlineData("6.9", "6.9.5")]
        [InlineData("6.10", "6.10.3")]
        [InlineData("6.11", "6.11.5")]
        [InlineData("8.0", "8.0.0")]
        [InlineData("8.1", "8.1.4")]
        [InlineData("8.2", "8.2.1")]
        [InlineData("8.8", "8.8.1")]
        [InlineData("8.9", "8.9.4")]
        [InlineData("8.11", "8.11.4")]
        [InlineData("8.12", "8.12.0")]
        [InlineData("9.4", "9.4.0")]
        [InlineData("10.1", "10.1.0")]
        [InlineData("10.10", "10.10.0")]
        [InlineData("10.12", "10.12.0")]
        [InlineData("10.14", "10.14.1")]
        public void NodeVersionMatchesImageName(string nodeTag, string nodeVersion)
        {
            // Arrange & Act
            var expectedNodeVersion = "v" + nodeVersion;
            var result = _dockerCli.Run(
                "oryxdevms/node-" + nodeTag + ":latest",
                commandToExecuteOnRun: "node",
                commandArguments: new[] { "--version" });

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
        [MemberData(nameof(TestValueGenerator.GetNodeVersions), MemberType = typeof(TestValueGenerator))]
        public void NodeImage_Contains_RequiredPrograms(string nodeTag)
        {
            // Arrange & Act
            var result = _dockerCli.Run(
                $"oryxdevms/node-{nodeTag}:latest",
                "sh", new[] { "-c", "which tar && which unzip && which pm2 && /opt/node-wrapper/node --version" });

            // Assert
            RunAsserts(() => Assert.True(result.IsSuccess), result.GetDebugInfo());
        }

        [Fact]
        public void GeneratedScript_CanRunStartupScriptsFromAppRoot()
        {
            // Arrange
            const int exitCodeSentinel = 222;
            var appPath = "/tmp/app";
            var script = new ShellScriptBuilder()
                .CreateDirectory(appPath)
                .CreateFile(appPath + "/entry.sh", $"exit {exitCodeSentinel}")
                .AddCommand("oryx -userStartupCommand entry.sh -appPath " + appPath)
                .AddCommand(". ./run.sh") // Source the default output path
                .ToString();

            // Act
            var res = _dockerCli.Run("oryxdevms/node-10.14", "/bin/sh", new[] { "-c", script });
            
            // Assert
            RunAsserts(() => Assert.Equal(res.ExitCode, exitCodeSentinel), res.GetDebugInfo());
        }

        [Theory]
        [MemberData(nameof(TestValueGenerator.GetNodeVersions_SupportPm2), MemberType = typeof(TestValueGenerator))]
        public async Task RunNodeAppUsingProcessJson(string nodeVersion)
        {

            var appName = "express-process-json";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.Create(hostDir);
            var dir = volume.ContainerDir;
            int hostPort = 8585;
            int containerPort = 80;

            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"cd {dir}/app")
                .AddCommand("npm install")
                .AddCommand("cd ..")
                .AddCommand($"oryx -bindPort {containerPort}")
                .AddCommand("./run.sh")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: $"oryxdevms/node-{nodeVersion}",
                output: _output,
                volumes:  new List<DockerVolume> { volume },
                environmentVariables: null,
                portMapping: $"{hostPort}:{containerPort}",
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", runAppScript },
                assertAction: async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Equal("Hello World from express!", data);
                },
                dockerCli: _dockerCli);
            
        }

        [Theory(Skip="Investigating debugging using pm2")]
        [MemberData(nameof(TestValueGenerator.GetNodeVersions_SupportDebugging), MemberType = typeof(TestValueGenerator))]
        public async Task RunNodeAppUsingProcessJson_withDebugging(string nodeVersion)
        {

            var appName = "express-process-json";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.Create(hostDir);
            var dir = volume.ContainerDir;
            int hostPort = 8585;
            int containerDebugPort = 9593;

            var runAppScript = new ShellScriptBuilder()
                .AddCommand($"cd {dir}/app")
                .AddCommand("npm install")
                .AddCommand("cd ..")
                .AddCommand($"oryx -remoteDebug -debugPort={containerDebugPort}")
                .AddCommand("./run.sh")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: $"oryxdevms/node-{nodeVersion}",
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                portMapping: $"{hostPort}:{containerDebugPort}",
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", runAppScript },
                assertAction: async () =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/json/list");
                    Assert.Contains("devtoolsFrontendUrl", data);
                },
                dockerCli: _dockerCli);

        }
    }
}
