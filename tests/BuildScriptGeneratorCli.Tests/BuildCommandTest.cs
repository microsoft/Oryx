// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Xunit;

namespace BuildScriptGeneratorCli.Tests
{
    public class BuildCommandTest
    {
        [Fact]
        public void OnExecute_ShowsHelp_AndExits_WhenSourceFolderIsNull()
        {
            // Arrange
            var buildCommand = new BuildCommand
            {
                SourceCodeFolder = null
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
                SourceCodeFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()),
                OutputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()),
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = buildCommand.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.NotEqual(0, exitCode);
            var error = testConsole.StdError;
            Assert.DoesNotContain("Usage:", error);
            Assert.Contains("Could not find the source code folder", error);
        }

        [Fact]
        public void OnExecute_ShowsHelp_AndExits_WhenOutputFolderIsNull()
        {
            // Arrange
            var buildCommand = new BuildCommand
            {
                SourceCodeFolder = Path.GetTempPath(),
                OutputFolder = null
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
        public void IsValidInput_IsTrue_EvenIfOutputFolderDoesNotExist()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions
            {
                // temp is always available
                SourceCodeFolder = Path.GetTempPath(),

                // New folder which does not exist yet
                OutputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
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
        public void DoesNotShowHelp_EvenIfIntermediateFolder_DoesNotExistYet()
        {
            // Arrange
            var buildCommand = new CustomBuildCommand
            {
                // temp is always available
                SourceCodeFolder = Path.GetTempPath(),

                // New folder which does not exist yet
                OutputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()),

                // New folder which does not exist yet
                IntermediateFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = buildCommand.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        private class CustomBuildCommand : BuildCommand
        {
            internal override int Execute(IServiceProvider serviceProvider, IConsole console)
            {
                return 0;
                //return base.Execute(serviceProvider, console);
            }
        }
    }
}
