// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    public class RunScriptCommandTest
    {
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
            var error = testConsole.StdError;
            Assert.DoesNotContain("Usage:", error);
            Assert.Contains($"Platform '{nonexistentPlatformName}' is not supported", error);
        }
    }
}
