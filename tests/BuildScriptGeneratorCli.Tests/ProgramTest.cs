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
    public class ProgramTest
    {
        [Fact]
        public void OnExecute_ShowsHelp_AndExits_WhenSourceFolderIsNull()
        {
            // Arrange
            var args = Array.Empty<string>();
            var program = new Program();
            var testConsole = new TestConsole();

            // Act
            var exitCode = program.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.NotEqual(0, exitCode);
            Assert.Contains("Usage:", testConsole.Output);
        }

        [Fact]
        public void OnExecute_ShowsHelp_AndExits_WhenSourceFolderIsEmpty()
        {
            // Arrange
            var args = Array.Empty<string>();
            var program = new Program
            {
                SourceCodeFolder = string.Empty
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = program.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.NotEqual(0, exitCode);
            Assert.Contains("Usage:", testConsole.Output);
        }

        [Fact]
        public void OnExecute_ShowsHelp_AndExits_WhenSourceDirectoryDoesNotExist()
        {
            // Arrange
            var args = Array.Empty<string>();
            var program = new Program
            {
                SourceCodeFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = program.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.NotEqual(0, exitCode);
            var output = testConsole.Output;
            Assert.DoesNotContain("Usage:", output);
            Assert.Contains("Could not find the source code folder", output);
        }

        [Fact]
        public void OnExecute_ShowsHelp_AndExits_WhenScriptOnlyIsTrue_ButNoScriptPathIsProvided()
        {
            // Arrange
            var args = Array.Empty<string>();
            var program = new Program
            {
                SourceCodeFolder = Path.GetTempPath(),
                ScriptOnly = true
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = program.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.NotEqual(0, exitCode);
            var output = testConsole.Output;
            Assert.DoesNotContain("Usage:", output);
            Assert.Contains("Error: Script path is required to be supplied if script only option is chosen", output);
        }

        [Fact]
        public void IsValidInput_IsTrue_EvenIfOutputFolderDoesNotExist()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions
            {
                SourceCodeFolder = Path.GetTempPath(),
                OutputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            };
            var testConsole = new TestConsole();
            var program = new Program();

            // Act
            var isValid = program.IsValidInput(options, testConsole);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void OnExecute_ShowsHelp_AndExits_IfNoScriptOnly_OrOutputFolder_IsSupplied()
        {
            // Arrange
            var args = Array.Empty<string>();
            var program = new Program
            {
                SourceCodeFolder = Path.GetTempPath()
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = program.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.NotEqual(0, exitCode);
            var output = testConsole.Output;
            Assert.DoesNotContain("Usage:", output);
            Assert.Contains("Error: Either script only or output folder must be provided.", output);
        }
    }
}
