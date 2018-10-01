// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Xunit;

namespace BuildScriptGeneratorCli.Tests
{
    public class ScriptCommandTest
    {
        [Fact]
        public void ScriptCommand_OnExecute_ShowsHelp_AndExits_WhenSourceFolderIsEmpty()
        {
            // Arrange
            var scriptCommand = new ScriptCommand
            {
                SourceCodeFolder = string.Empty
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = scriptCommand.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.NotEqual(0, exitCode);
            Assert.Contains("Usage:", testConsole.StdOutput);
        }

        [Fact]
        public void OnExecute_ShowsHelp_AndExits_WhenSourceDirectoryDoesNotExist()
        {
            // Arrange
            var scriptCommand = new ScriptCommand
            {
                SourceCodeFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = scriptCommand.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.NotEqual(0, exitCode);
            var error = testConsole.StdError;
            Assert.DoesNotContain("Usage:", error);
            Assert.Contains("Could not find the source code folder", error);
        }

        [Fact(Skip = "Todo")]
        public void Execute_DoesNotWriteAnythingElseToConsole_ApartFromScript()
        {
        }
    }
}
