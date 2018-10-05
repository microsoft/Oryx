// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Xunit;

namespace BuildScriptGeneratorCli.Tests
{
    public class BuildCommandTest : IClassFixture<BuildCommandTest.TestFixutre>
    {
        private static string _rootDirPath;

        public BuildCommandTest(TestFixutre testFixutre)
        {
            _rootDirPath = testFixutre.RootDirPath;
        }

        [Fact]
        public void OnExecute_ShowsHelp_AndExits_WhenSourceDirIsNull()
        {
            // Arrange
            var buildCommand = new BuildCommand
            {
                SourceDir = null
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = buildCommand.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.NotEqual(0, exitCode);
            Assert.Contains("Usage:", testConsole.StdOutput);
        }

        [Fact]
        public void OnExecute_ShowsHelp_AndExits_WhenSourceDirectoryDoesNotExist()
        {
            // Arrange
            var buildCommand = new BuildCommand
            {
                SourceDir = Path.Combine(_rootDirPath, Guid.NewGuid().ToString()),
                DestinationDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()),
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = buildCommand.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.NotEqual(0, exitCode);
            var error = testConsole.StdError;
            Assert.DoesNotContain("Usage:", error);
            Assert.Contains("Could not find the source directory", error);
        }

        [Fact]
        public void OnExecute_ShowsHelp_AndExits_WhenDestinationDirIsNull()
        {
            // Arrange
            var buildCommand = new BuildCommand
            {
                SourceDir = _rootDirPath,
                DestinationDir = null
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = buildCommand.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.NotEqual(0, exitCode);
            var output = testConsole.StdOutput;
            Assert.Contains("Usage:", output);
        }

        [Fact]
        public void IsValidInput_IsTrue_EvenIfDestinationDirDoesNotExist()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions
            {
                SourceDir = _rootDirPath,

                // New directory which does not exist yet
                DestinationDir = Path.Combine(_rootDirPath, Guid.NewGuid().ToString())
            };
            var testConsole = new TestConsole();
            var program = new BuildCommand();

            // Act
            var isValid = program.IsValidInput(options, testConsole);

            // Assert
            Assert.True(isValid);
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        [Fact]
        public void IsValidInput_IsTrue_EvenIfDestinationDirExists_AndIsEmpty()
        {
            // Arrange
            var sourceDir = Directory.CreateDirectory(Path.Combine(_rootDirPath, Guid.NewGuid().ToString()));
            var destinationDir = Directory.CreateDirectory(Path.Combine(_rootDirPath, Guid.NewGuid().ToString()));
            var options = new BuildScriptGeneratorOptions
            {
                SourceDir = sourceDir.FullName,

                // New directory which is empty
                DestinationDir = destinationDir.FullName
            };
            var testConsole = new TestConsole();
            var program = new BuildCommand();

            // Act
            var isValid = program.IsValidInput(options, testConsole);

            // Assert
            Assert.True(isValid);
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        public static TheoryData<string> DestinationDirectoryPathData
        {
            get
            {
                var data = new TheoryData<string>();

                // Sub-directory with a file
                var destinationDir = Directory.CreateDirectory(
                    Path.Combine(_rootDirPath, Guid.NewGuid().ToString()));
                var subDir = Directory.CreateDirectory(
                    Path.Combine(destinationDir.FullName, Guid.NewGuid().ToString()));
                File.WriteAllText(Path.Combine(subDir.FullName, "file1.txt"), "file1 content");
                data.Add(destinationDir.FullName);

                // Sub-directory which is empty
                destinationDir = Directory.CreateDirectory(
                    Path.Combine(_rootDirPath, Guid.NewGuid().ToString()));
                subDir = Directory.CreateDirectory(
                    Path.Combine(destinationDir.FullName, Guid.NewGuid().ToString()));
                data.Add(destinationDir.FullName);

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(DestinationDirectoryPathData))]
        public void IsValidInput_IsFalse_IfDestinationDirIsNotEmpty_AndForceOption_IsFalse(
            string destinationDir)
        {
            // Arrange
            var sourceDir = Directory.CreateDirectory(Path.Combine(_rootDirPath, Guid.NewGuid().ToString()));

            var options = new BuildScriptGeneratorOptions
            {
                SourceDir = sourceDir.FullName,
                DestinationDir = destinationDir,
                Force = false,
            };
            var testConsole = new TestConsole();
            var program = new BuildCommand();

            // Act
            var isValid = program.IsValidInput(options, testConsole);

            // Assert
            Assert.False(isValid);
            Assert.Contains("Destination directory is not empty.", testConsole.StdError);
        }

        [Theory]
        [MemberData(nameof(DestinationDirectoryPathData))]
        public void IsValidInput_IsTrue_IfDestinationDirIsNotEmpty_AndForceOption_IsTrue(
            string destinationDir)
        {
            // Arrange
            var sourceDir = Directory.CreateDirectory(Path.Combine(_rootDirPath, Guid.NewGuid().ToString()));

            var options = new BuildScriptGeneratorOptions
            {
                SourceDir = sourceDir.FullName,
                DestinationDir = destinationDir,
                Force = true,
            };
            var testConsole = new TestConsole();
            var program = new BuildCommand();

            // Act
            var isValid = program.IsValidInput(options, testConsole);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void DoesNotShowHelp_EvenIfIntermediateDir_DoesNotExistYet()
        {
            // Arrange
            var buildCommand = new CustomBuildCommand
            {
                // temp is always available
                SourceDir = Path.GetTempPath(),

                // New directory which does not exist yet
                DestinationDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()),

                // New directory which does not exist yet
                IntermediateDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = buildCommand.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        [Fact]
        public void ConfiguresOptions_WithScriptGeneratorRootDirectory_InTemp()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var buildCommand = new BuildCommand
            {
                SourceDir = "app",
                DestinationDir = "app-output",
            };

            // Act
            buildCommand.ConfigureBuildScriptGeneratorOptoins(options);

            // Assert
            Assert.StartsWith(
                Path.Combine(Path.GetTempPath(), nameof(Microsoft.Oryx.BuildScriptGenerator)),
                options.TempDir);
        }

        [Fact]
        public void ResolvesToCurrentDirectoryAbsolutePaths_WhenDotNotationIsUsed()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var currentDir = Directory.GetCurrentDirectory();
            var buildCommand = new BuildCommand
            {
                SourceDir = ".",
                DestinationDir = ".",
                IntermediateDir = ".",
                LogFile = "logFile.txt",
            };

            // Act
            buildCommand.ConfigureBuildScriptGeneratorOptoins(options);

            // Assert
            Assert.Equal(currentDir, options.SourceDir);
            Assert.Equal(currentDir, options.DestinationDir);
            Assert.Equal(currentDir, options.IntermediateDir);
            Assert.Equal(Path.Combine(currentDir, "logFile.txt"), options.LogFile);
        }

        [Theory]
        [InlineData("dir1")]
        [InlineData("dir1", "dir2")]
        public void ResolvesToAbsolutePaths_WhenRelativePathsAreGiven(params string[] paths)
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var providedPath = Path.Combine(paths);
            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), providedPath);
            var logFile = Path.Combine(Directory.GetCurrentDirectory(), "logFile.txt");
            var buildCommand = new BuildCommand
            {
                SourceDir = providedPath,
                DestinationDir = providedPath,
                IntermediateDir = providedPath,
                LogFile = "logFile.txt",
            };

            // Act
            buildCommand.ConfigureBuildScriptGeneratorOptoins(options);

            // Assert
            Assert.Equal(absolutePath, options.SourceDir);
            Assert.Equal(absolutePath, options.DestinationDir);
            Assert.Equal(absolutePath, options.IntermediateDir);
            Assert.Equal(logFile, options.LogFile);
        }

        [Fact]
        public void ResolvesToAbsolutePaths_WhenAbsolutePathsAreGiven()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var absolutePath = Path.GetTempPath();
            var logFile = Path.Combine(Path.GetTempPath(), "logFile.txt");
            var buildCommand = new BuildCommand
            {
                SourceDir = absolutePath,
                DestinationDir = absolutePath,
                IntermediateDir = absolutePath,
                LogFile = logFile,
            };

            // Act
            buildCommand.ConfigureBuildScriptGeneratorOptoins(options);

            // Assert
            Assert.Equal(absolutePath, options.SourceDir);
            Assert.Equal(absolutePath, options.DestinationDir);
            Assert.Equal(absolutePath, options.IntermediateDir);
            Assert.Equal(logFile, options.LogFile);
        }

        [Theory]
        [InlineData("trace", LogLevel.Trace)]
        [InlineData("debug", LogLevel.Debug)]
        [InlineData("information", LogLevel.Information)]
        [InlineData("warning", LogLevel.Warning)]
        [InlineData("error", LogLevel.Error)]
        [InlineData("critical", LogLevel.Critical)]
        public void ConfiguresOptions_ForAllAllowedLoggingLevels(string logLevel, LogLevel expected)
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var buildCommand = new BuildCommand
            {
                SourceDir = "app",
                DestinationDir = "app-output",
                LogFile = Path.Combine(Path.GetTempPath(), "logFile.txt"),
                MinimumLogLevel = logLevel,
            };

            // Act
            buildCommand.ConfigureBuildScriptGeneratorOptoins(options);

            // Assert
            Assert.Equal(expected, options.MinimumLogLevel);
        }

        private class CustomBuildCommand : BuildCommand
        {
            internal override int Execute(IServiceProvider serviceProvider, IConsole console)
            {
                return 0;
                //return base.Execute(serviceProvider, console);
            }
        }

        public class TestFixutre : IDisposable
        {
            public TestFixutre()
            {
                RootDirPath = Path.Combine(
                    Path.GetTempPath(),
                    nameof(BuildCommandTest));

                Directory.CreateDirectory(RootDirPath);
            }

            public string RootDirPath { get; }

            public void Dispose()
            {
                if (Directory.Exists(RootDirPath))
                {
                    try
                    {
                        Directory.Delete(RootDirPath, recursive: true);
                    }
                    catch
                    {
                        // Do not throw in dispose
                    }
                }
            }
        }
    }
}
