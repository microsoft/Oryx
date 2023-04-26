// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Tests.Common;
using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    public class RunScriptCommandTest : ScriptCommandTestBase
    {
        public RunScriptCommandTest(TestTempDirTestFixture testFixture) : base(testFixture) { }

        [Fact]
        public void OnExecute_ShowsHelp_AndExits_WhenSourceDirectoryDoesNotExist()
        {
            // Arrange
            var scriptCommand = new RunScriptCommand
            {
                AppDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = scriptCommand.OnExecute(testConsole);

            // Assert
            Assert.NotEqual(0, exitCode);
            var error = testConsole.StdError;
            Assert.DoesNotContain("Usage:", error);
            Assert.Contains("Could not find the source directory", error);
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
            var exitCode = cmd.OnExecute(testConsole);

            // Assert
            Assert.NotEqual(0, exitCode);
            Assert.Contains($"Platform '{nonexistentPlatformName}' is not supported", testConsole.StdError);
        }

        [Fact]
        public void Configure_UsesCurrentDirectory_WhenSourceDirectoryNotSupplied()
        {
            // Arrange
            var scriptCommand = new RunScriptCommand { AppDir = string.Empty };

            // Act
            var processedInput = scriptCommand.IsValidInput(null, null);

            // Assert
            Assert.True(processedInput);
            Assert.Equal(Directory.GetCurrentDirectory(), scriptCommand.AppDir);
        }
    }
}
