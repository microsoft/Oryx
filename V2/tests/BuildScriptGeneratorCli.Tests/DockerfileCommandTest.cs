// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System.IO;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    public class DockerfileCommandTest : IClassFixture<TestTempDirTestFixture>
    {
        internal static TestTempDirTestFixture _testDir;
        internal static string _testDirPath;

        public DockerfileCommandTest(TestTempDirTestFixture testFixture)
        {
            _testDir = testFixture;
            _testDirPath = testFixture.RootDirPath;
        }

        [Fact]
        public void IsValidInput_UsesCurrentDirectory_WhenSourceDirectoryNotSupplied()
        {
            // Arrange
            var dockerfileCommand = new DockerfileCommand { SourceDir = string.Empty };
            var testConsole = new TestConsole();

            // Act
            var isValidInput = dockerfileCommand.IsValidInput(null, testConsole);

            // Assert
            Assert.True(isValidInput);
            Assert.Equal(Directory.GetCurrentDirectory(), dockerfileCommand.SourceDir);
        }

        [Fact]
        public void IsValidInput_IsFalse_WhenSourceDirectorySuppliedDoesNotExist()
        {
            // Arrange
            var dockerfileCommand = new DockerfileCommand { SourceDir = _testDir.GenerateRandomChildDirPath() };
            var testConsole = new TestConsole();

            // Act
            var isValidInput = dockerfileCommand.IsValidInput(null, testConsole);

            // Assert
            Assert.False(isValidInput);
            Assert.Contains("Could not find the source directory", testConsole.StdError);
        }

        [Fact]
        public void IsValidInput_IsFalse_WhenPlatformVersionSpecified_WithoutPlatformName()
        {
            // Arrange
            var dockerfileCommand = new DockerfileCommand { SourceDir = string.Empty, PlatformVersion = "1.0.0" };
            var testConsole = new TestConsole();

            // Act
            var isValidInput = dockerfileCommand.IsValidInput(null, testConsole);

            // Assert
            Assert.False(isValidInput);
            Assert.Contains("Cannot use platform version without specifying platform name also.", testConsole.StdError);
        }

        [Fact]
        public void IsValidInput_IsFalse_WhenBindPortSpecified_NotInteger()
        {
            // Arrange
            var invalidBindPort = "foo";
            var dockerfileCommand = new DockerfileCommand { SourceDir = string.Empty, BindPort = invalidBindPort };
            var testConsole = new TestConsole();

            // Act
            var isValidInput = dockerfileCommand.IsValidInput(null, testConsole);

            // Assert
            Assert.False(isValidInput);
            Assert.Contains($"Provided bind port '{invalidBindPort}' is not valid.", testConsole.StdError);
        }

        [Fact]
        public void IsValidInput_IsFalse_WhenBindPortSpecified_NotValidPort()
        {
            // Arrange
            var invalidBindPort = "1000000";
            var dockerfileCommand = new DockerfileCommand { SourceDir = string.Empty, BindPort = invalidBindPort };
            var testConsole = new TestConsole();

            // Act
            var isValidInput = dockerfileCommand.IsValidInput(null, testConsole);

            // Assert
            Assert.False(isValidInput);
            Assert.Contains($"Provided bind port '{invalidBindPort}' is not valid.", testConsole.StdError);
        }

        [Fact]
        public void IsValidInput_IsFalse_WhenBuildImageSpecified_NotValid()
        {
            // Arrange
            var invalidBuildImage = "build";
            var dockerfileCommand = new DockerfileCommand { SourceDir = string.Empty, BuildImage = invalidBuildImage };
            var testConsole = new TestConsole();

            // Act
            var isValidInput = dockerfileCommand.IsValidInput(null, testConsole);

            // Assert
            Assert.False(isValidInput);
            Assert.Contains($"Provided build image must be in the format '<image>:<tag>'.", testConsole.StdError);
        }
    }
}
