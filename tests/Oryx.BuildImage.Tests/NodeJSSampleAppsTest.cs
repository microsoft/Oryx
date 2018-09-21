// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System;
using System.IO;
using Oryx.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.BuildImage.Tests
{
    public class NodeJSSampleAppsTest
    {
        private const string BuildScriptGeneratorPath = "/opt/buildscriptgen/GenerateBuildScript";
        private readonly ITestOutputHelper _output;
        private readonly DockerCli _dockerCli;
        private readonly string _hostSamplesDir;

        public NodeJSSampleAppsTest(ITestOutputHelper output)
        {
            _output = output;

            _dockerCli = new DockerCli(waitTimeInSeconds: (int)TimeSpan.FromMinutes(2).TotalSeconds);

            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
        }

        [Fact]
        public void GeneratesScript_AndBuilds()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/nodejs/webfrontend";
            var appOutputDir = "/webfrontend-output";

            // Act
            var result = _dockerCli.Run(
                "oryxdevms/build:latest",
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    "\"" +
                    $"{BuildScriptGeneratorPath} {appDir} {appOutputDir} && " +
                    $"ls {appOutputDir}" +
                    "\""
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("node_modules", result.Output); // to see if the build actually happened
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_AndBuilds_WhenExplicitLanguageAndVersion_AreProvided()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/nodejs/webfrontend";
            var appOutputDir = "/webfrontend-output";

            // Act
            var result = _dockerCli.Run(
                "oryxdevms/build:latest",
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    "\"" +
                    $"{BuildScriptGeneratorPath} {appDir} {appOutputDir} -l nodejs -lv 8.2.1 && " +
                    $"ls {appOutputDir}" +
                    "\""
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("node_modules", result.Output); // to see if the build actually happened
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void CanBuild_UsingScriptGeneratedBy_ScriptOnlyOption()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/nodejs/webfrontend";
            var appOutputDir = "/webfrontend-output";
            var generatedScript = "/build.sh";

            // Act
            var result = _dockerCli.Run(
                "oryxdevms/build:latest",
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    "\"" +
                    $"{BuildScriptGeneratorPath} {appDir} --script-only --script-path {generatedScript} && " +
                    $"{generatedScript} {appDir} {appOutputDir} && " +
                    $"ls {appOutputDir}" +
                    "\""
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("node_modules", result.Output); // to see if the build actually happened
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void CanBuild_UsingScriptGeneratedBy_ScriptOnlyOption_AndWhenExplicitLanguageAndVersion_AreProvided()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/nodejs/webfrontend";
            var appOutputDir = "/webfrontend-output";
            var generatedScript = "/build.sh";

            // Act
            var result = _dockerCli.Run(
                "oryxdevms/build:latest",
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    "\"" +
                    $"{BuildScriptGeneratorPath} {appDir} -l nodejs -lv 8.2.1 --script-only --script-path {generatedScript} && " +
                    $"{generatedScript} {appDir} {appOutputDir} && " +
                    $"ls {appOutputDir}" +
                    "\""
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("node_modules", result.Output); // to see if the build actually happened
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_AndBuilds_UsingSuppliedIntermediateFolder()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/nodejs/webfrontend";
            var intermediateDir = $"/webfrontend-intermediate";
            var appOutputDir = "/webfrontend-output";

            // Act
            var result = _dockerCli.Run(
                "oryxdevms/build:latest",
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    "\"" +
                    $"{BuildScriptGeneratorPath} {appDir} {appOutputDir} -i {intermediateDir} && " +
                    $"ls {appOutputDir}" +
                    "\""
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("node_modules", result.Output); // to see if the build actually happened
                },
                result.GetDebugInfo());
        }

        private void RunAsserts(Action action, string message)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                _output.WriteLine(message);
                throw;
            }
        }
    }
}
