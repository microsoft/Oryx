// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Tests.Common;
using Xunit;

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
        public void IsValidInput_ReturnsFalse_WhenSourceDirDoesNotExist()
        {
            // Arrange
            var cmd = new ExecCommand
            {
                SourceDir = _testDir.GenerateRandomChildDirPath(),
                Command = "bla",
            };
            var testConsole = new TestConsole();

            // Act
            var retVal = cmd.IsValidInput(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.False(retVal);
            Assert.Contains("Could not find the source directory", testConsole.StdError);
        }
    }
}
