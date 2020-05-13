// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Microsoft.Oryx.Common;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Php;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    public class DetectComandTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _testDirPath;
        private readonly TestTempDirTestFixture _testDir;

        public DetectComandTest(TestTempDirTestFixture testFixture)
        {
            _testDirPath = testFixture.RootDirPath;
            _testDir = testFixture;
        }
        
        [Fact]
        public void IsValidInput_UsesCurrentDirectory_IfSourceDirectoryNotSupplied()
        {
            // Arrange
            var detectCommand = new DetectCommand
            {
                SourceDir = string.Empty
            };
            var testConsole = new TestConsole();

            // Act
            var isValidInput = detectCommand.IsValidInput(null, testConsole);

            // Assert
            Assert.True(isValidInput);
            Assert.Equal(Directory.GetCurrentDirectory(), detectCommand.SourceDir);
            Assert.Empty(testConsole.StdError);
        }
        
        [Fact]
        public void IsValidInput_IsFalse_IfProvidedSourceDirectoryDoesNotExists()
        {
            // Arrange
            var sourceDir = _testDir.CreateChildDir();
            var testConsole = new TestConsole();
            var detectCommand = new DetectCommand
            {
                SourceDir = sourceDir + "blah",
            };
            var serviceProvider = detectCommand.GetServiceProvider(testConsole);

            // Act
            var isValidInput = detectCommand.IsValidInput(serviceProvider, testConsole);

            // Assert
            Assert.False(isValidInput);
            Assert.Contains(
                $"Could not find the source directory: '{sourceDir}'",
                testConsole.StdError);
        }
        
        [Fact]
        public void Execute_OutputsNodePlatformAndVersion()
        {
            // Arrange
            var sourceDir = Path.Combine(_testDirPath, "nodeAppDir");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, NodeConstants.PackageJsonFileName), "\n");

            var detectCommand = new DetectCommand
            {
                SourceDir = sourceDir,
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = detectCommand.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.Equal(ProcessConstants.ExitSuccess, exitCode);
            Assert.Contains(
                $"{NodeConstants.PlatformName}",
                testConsole.StdOutput);
            
        }
        
        [Fact]
        public void Execute_OutputsMultiplePlatformAndVersion()
        {
            
        }
        
        [Fact]
        public void Execute_OutputsMultiplePlatformAndVersionInJsonFormat()
        {
            
        }
        
        
        
    }
}