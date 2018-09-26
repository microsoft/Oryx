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
            Assert.Contains("Usage:", testConsole.Output);
        }
    }
}
