// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.CommandLine;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    public class ProgramTest
    {
        [Fact]
        public void OnExecute_ShowsInfo_WhenInfoOptionIsUsed()
        {
            // Arrange
            var testConsole = new TestConsole();

            // Act
            int exitCode = Program.OnExecute(console: testConsole, setInfo: true);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Contains("Version:", testConsole.StdOutput);
            Assert.Contains("Commit:", testConsole.StdOutput);
        }
    }
}