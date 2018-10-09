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
            var script = new BashScriptBuilder()
                .AddBuildCommand($"{appDir} {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .ToString();

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
                    script +
                    "\""
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Builds_AndCopiesContentToOutputDirectory_Recursively()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/nodejs/webfrontend";
            var appOutputDir = "/webfrontend-output";
            var subDir = Guid.NewGuid();
            var script = new BashScriptBuilder()
                // Add a test sub-directory with a file
                .CreateDirectory($"{appDir}/{subDir}")
                .CreateFile($"{appDir}/{subDir}/file1.txt", "file1.txt")
                // Execute command
                .AddBuildCommand($"{appDir} {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                // Check the output directory for the sub directory
                .AddFileExistsCheck($"{appOutputDir}/{subDir}/file1.txt")
                .ToString();

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
                    script +
                    "\""
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Build_CopiesOutput_ToNestedOutputDirectory()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/nodejs/webfrontend";
            var nestedOutputDir = "/output/subdir1";
            var script = new BashScriptBuilder()
                .AddBuildCommand($"{appDir} {nestedOutputDir}")
                .AddDirectoryExistsCheck($"{nestedOutputDir}/node_modules")
                .ToString();

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
                    script +
                    "\""
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void BuildFails_WhenDestinationDirectoryIsNotEmpty_AndForceOption_IsFalse()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/nodejs/webfrontend";
            var appOutputDir = "/app-output";
            var script = new BashScriptBuilder()
                // Pre-populate the output directory with content
                .CreateDirectory(appOutputDir)
                .CreateFile($"{appOutputDir}/hi.txt", "hi")
                .CreateDirectory($"{appOutputDir}/blah")
                .CreateFile($"{appOutputDir}/blah/hi.txt", "hi")
                // No '-f' option used here
                .AddBuildCommand($"{appDir} {appOutputDir}")
                .ToString();

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
                    script +
                    "\""
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.False(result.IsSuccess);
                    Assert.Contains(
                        "Destination directory is not empty. Use '--force' option to replace the destination directory content.",
                        result.Error);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Build_ReplacesContentInDestinationDir_WhenForceOption_IsTrue()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/nodejs/webfrontend";
            var appOutputDir = "/output";
            var script = new BashScriptBuilder()
                // Pre-populate the output directory with content
                .CreateDirectory(appOutputDir)
                .CreateFile($"{appOutputDir}/hi.txt", "hi")
                .CreateDirectory($"{appOutputDir}/blah")
                .CreateFile($"{appOutputDir}/blah/hi.txt", "hi")
                // Using '-f' option here
                .AddBuildCommand($"{appDir} {appOutputDir} -f")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddFileDoesNotExistCheck($"{appOutputDir}/hi.txt")
                .AddDirectoryDoesNotExistCheck($"{appOutputDir}/blah")
                .ToString();

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
                    script +
                    "\""
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void ErrorDuringBuild_ResultsIn_NonSuccessfulExitCode()
        {
            // Arrange
            // Here 'createServerFoooo' is a non-existing function in 'http' library
            var serverJsWithErrors = @"var http = require(""http""); http.createServerFoooo();";
            var appDir = "/app";
            var appOutputDir = "/app-output";
            var script = new BashScriptBuilder()
                .CreateDirectory(appDir)
                .CreateFile($"{appDir}/server.js", serverJsWithErrors)
                .AddBuildCommand($"{appDir} {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                "oryxdevms/build:latest",
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    "\"" +
                    script +
                    "\""
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.False(result.IsSuccess);
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
            var script = new BashScriptBuilder()
                .AddBuildCommand($"{appDir} {appOutputDir} -l nodejs --language-version 8.2.1")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .ToString();

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
                    script +
                    "\""
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
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
            var script = new BashScriptBuilder()
                .AddBuildCommand($"{appDir} {appOutputDir} --script-only > {generatedScript}")
                .SetExecutePermissionOnFile(generatedScript)
                .AddCommand($"{generatedScript} {appDir} {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .ToString();

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
                    script +
                    "\""
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
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
            var script = new BashScriptBuilder()
                .AddBuildCommand($"{appDir} {appOutputDir} -l nodejs --language-version 8.2.1 --script-only > {generatedScript}")
                .SetExecutePermissionOnFile(generatedScript)
                .AddCommand($"{generatedScript} {appDir} {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .ToString();

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
                    script +
                    "\""
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_AndBuilds_UsingSuppliedIntermediateDir()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/nodejs/webfrontend";
            var intermediateDir = $"/webfrontend-intermediate";
            var appOutputDir = "/webfrontend-output";
            var script = new BashScriptBuilder()
                .AddBuildCommand($"{appDir} {appOutputDir} -i {intermediateDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .ToString();

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
                    script +
                    "\""
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
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
