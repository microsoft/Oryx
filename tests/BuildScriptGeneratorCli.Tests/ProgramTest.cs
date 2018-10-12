// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using McMaster.Extensions.CommandLineUtils;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Xunit;

namespace BuildScriptGeneratorCli.Tests
{
    public class ProgramTest
    {
        [Fact]
        public void OnExecute_ShowsHelp_AndExits_WithSuccessExitCode()
        {
            // Arrange
            var program = new Program();
            var testConsole = new TestConsole();

            // Act
            var exitCode = program.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Contains("Usage:", testConsole.StdOutput);
        }

        [Fact]
        public void OnExecute_ShowsVersion_WhenVersionOptionIsUsed()
        {
            // Arrange
            var program = new Program
            {
                Version = true
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = program.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Contains("Version:", testConsole.StdOutput);
            Assert.Contains("Commit:", testConsole.StdOutput);
        }
    }
}
