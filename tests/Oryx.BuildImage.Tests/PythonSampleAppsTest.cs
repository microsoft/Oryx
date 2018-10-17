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
    public class PythonSampleAppsTest : SampleAppsTestBase
    {
        private readonly ITestOutputHelper _output;
        private readonly DockerCli _dockerCli;
        private readonly string _hostSamplesDir;

        public PythonSampleAppsTest(ITestOutputHelper output)
        {
            _output = output;

            _dockerCli = new DockerCli(waitTimeInSeconds: (int)TimeSpan.FromMinutes(2).TotalSeconds);

            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
        }

        [Fact]
        public override void GeneratesScript_AndBuilds()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/python/flask-app";
            var appOutputDir = "/flask-app-output";
            var script = new BashScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/antenv")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                BuildImageTestSettings.BuildImageName,
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
                    Assert.Contains("Python Version: /opt/python/3.7.0/bin/python3", result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public override void Builds_AndCopiesContentToOutputDirectory_Recursively()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/python/flask-app";
            var appOutputDir = "/flask-app-output";
            var subDir = Guid.NewGuid();
            var script = new BashScriptBuilder()
                // Add a test sub-directory with a file
                .CreateDirectory($"{appDir}/{subDir}")
                .CreateFile($"{appDir}/{subDir}/file1.txt", "file1.txt")
                // Execute command
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/antenv")
                // Check the output directory for the sub directory
                .AddFileExistsCheck($"{appOutputDir}/{subDir}/file1.txt")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                BuildImageTestSettings.BuildImageName,
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
        public override void Build_CopiesOutput_ToNestedOutputDirectory()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/python/flask-app";
            var nestedOutputDir = "/output/subdir1";
            var script = new BashScriptBuilder()
                .AddBuildCommand($"{appDir} -o {nestedOutputDir}")
                .AddDirectoryExistsCheck($"{nestedOutputDir}/antenv")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                BuildImageTestSettings.BuildImageName,
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
        public override void GeneratesScriptAndBuilds_WhenSourceAndDestinationFolders_AreSame()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/python/flask-app";
            var script = new BashScriptBuilder()
                .AddBuildCommand($"{appDir}")
                .AddDirectoryExistsCheck($"{appDir}/antenv")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                BuildImageTestSettings.BuildImageName,
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
        public override void GeneratesScriptAndBuilds_WhenDestination_IsSubDirectoryOfSource()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/python/flask-app";
            var appOutputDir = $"{appDir}/output";
            var script = new BashScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/antenv")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                BuildImageTestSettings.BuildImageName,
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
        public override void Build_ReplacesContentInDestinationDir_WhenDestinationDirIsNotEmpty()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/python/flask-app";
            var appOutputDir = "/flask-app-output";
            var script = new BashScriptBuilder()
                // Pre-populate the output directory with content
                .CreateDirectory(appOutputDir)
                .CreateFile($"{appOutputDir}/hi.txt", "hi")
                .CreateDirectory($"{appOutputDir}/blah")
                .CreateFile($"{appOutputDir}/blah/hi.txt", "hi")
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/antenv")
                .AddFileDoesNotExistCheck($"{appOutputDir}/hi.txt")
                .AddDirectoryDoesNotExistCheck($"{appOutputDir}/blah")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                BuildImageTestSettings.BuildImageName,
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

        [Fact(Skip = "Todo")]
        public override void ErrorDuringBuild_ResultsIn_NonSuccessfulExitCode()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public override void GeneratesScript_AndBuilds_WhenExplicitLanguageAndVersion_AreProvided()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/python/flask-app";
            var appOutputDir = $"{appDir}/output";
            var script = new BashScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir} -l python --language-version 3.6.6")
                .AddDirectoryExistsCheck($"{appOutputDir}/antenv3.6")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                BuildImageTestSettings.BuildImageName,
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
                    Assert.Contains("Python Version: /opt/python/3.6.6/bin/python3", result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public override void CanBuild_UsingScriptGeneratedBy_ScriptOnlyOption()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/python/flask-app";
            var generatedScript = "/build.sh";
            var appOutputDir = "/flask-app-output";
            var tempDir = "/tmp/" + Guid.NewGuid();
            var script = new BashScriptBuilder()
                .AddScriptCommand($"{appDir} -l python --language-version 3.6.6 > {generatedScript}")
                .SetExecutePermissionOnFile(generatedScript)
                .CreateDirectory(tempDir)
                .AddCommand($"{generatedScript} {appDir} {appOutputDir} {tempDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/antenv3.6")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                BuildImageTestSettings.BuildImageName,
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
        public override void GeneratesScript_AndBuilds_UsingSuppliedIntermediateDir()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);
            var appDir = $"{volume.ContainerDir}/python/flask-app";
            var appIntermediateDir = "/flask-app-int";
            var appOutputDir = "/flask-app-output";
            var script = new BashScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir} -i {appIntermediateDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/antenv")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                BuildImageTestSettings.BuildImageName,
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
