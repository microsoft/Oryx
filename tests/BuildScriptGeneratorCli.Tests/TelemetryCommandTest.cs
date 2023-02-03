// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    public class TelemetryCommandTest : IClassFixture<TestTempDirTestFixture>
    {
        internal static TestTempDirTestFixture _testDir;
        internal static string _testDirPath;

        public TelemetryCommandTest(TestTempDirTestFixture testFixture)
        {
            _testDir = testFixture;
            _testDirPath = testFixture.RootDirPath;
        }

        [Fact]
        public void IsValidInput_IsFalse_WhenEventNameNotProvided()
        {
            // Arrange
            var telemetryCommand = new TelemetryCommand();
            var testConsole = new TestConsole();

            // Act
            var isValidInput = telemetryCommand.IsValidInput(null, testConsole);

            // Assert
            Assert.False(isValidInput);
            Assert.Contains("The 'oryx telemetry' command requires a value for --event-name.", testConsole.StdError);
        }
    }
}
