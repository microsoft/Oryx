// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using McMaster.Extensions.CommandLineUtils;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    public class RunScriptCommandTest
    {
        [Fact]
        public void OnExecute_ShowsErrorExits_WhenNoPlatformSpecified()
        {
            // Arrange
            var cmd = new RunScriptCommand();
            var testConsole = new TestConsole();

            // Act
            var exitCode = cmd.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.NotEqual(0, exitCode);
            Assert.Contains($"Platform name is required", testConsole.StdError);
        }

        [Fact]
        public void OnExecute_ShowsErrorExits_WhenPlatformDoesNotExist()
        {
            // Arrange
            var nonexistentPlatformName = "bla";
            var cmd = new RunScriptCommand
            {
                PlatformName = nonexistentPlatformName
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = cmd.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.NotEqual(0, exitCode);
            Assert.Contains($"Platform '{nonexistentPlatformName}' is not supported", testConsole.StdError);
        }
    }
}
