// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    public class ExecCommandTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly TestTempDirTestFixture _testDir;

        public ExecCommandTest(TestTempDirTestFixture testFixture)
        {
            _testDir = testFixture;
        }

        [Fact]
        public void OnExecute_ShowsErrorAndExits_WhenSourceDirectoryDoesNotExist()
        {
            // Arrange
            var cmd = new ExecCommand
            {
                SourceDir = _testDir.GenerateRandomChildDirPath(),
                Command = "bla",
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = cmd.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.Equal(ProcessConstants.ExitFailure, exitCode);
            Assert.Contains("Could not find the source directory", testConsole.StdError);
        }
    }
}
