// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Oryx.Detector.Node;
using Microsoft.Oryx.Detector.Php;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            var serviceProvider = detectCommand.GetServiceProvider(testConsole);

            // Act
            var isValidInput = detectCommand.IsValidInput(serviceProvider, testConsole);

            // Assert
            Assert.True(isValidInput);
            Assert.Equal(Directory.GetCurrentDirectory(), detectCommand.SourceDir);
        }

        [Fact]
        public void IsValidInput_IsFalse_IfSourceDirectorySuppliedDoesNotExists()
        {
            // Arrange
            var testConsole = new TestConsole();
            var detectCommand = new DetectCommand
            {
                SourceDir = _testDir.GenerateRandomChildDirPath()
            };
            var serviceProvider = detectCommand.GetServiceProvider(testConsole);

            // Act
            var isValidInput = detectCommand.IsValidInput(serviceProvider, testConsole);

            // Assert
            Assert.False(isValidInput);
            Assert.Contains(
                $"Could not find the source directory",
                testConsole.StdError);
        }

        [Fact]
        public void Execute_OutputsNodePlatformAndNullVersion_WhenJsonFileIsEmpty()
        {
            // Arrange
            var sourceDir = Path.Combine(_testDirPath, "nodeappdir");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, NodeConstants.PackageJsonFileName), "\n");

            var detectCommand = new DetectCommand
            {
                SourceDir = sourceDir,
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = detectCommand.Execute(GetServiceProvider(detectCommand), testConsole);

            // Assert
            Assert.Equal(ProcessConstants.ExitSuccess, exitCode);
            Assert.Contains(
                $"Platform: {PlatformName.Node}",
                testConsole.StdOutput);
            Assert.Contains(
                $"Version : Not Detected",
                testConsole.StdOutput);
        }

        [Fact]
        public void Execute_OutputsPhpPlatformAndVersion_WhenComposerFileExplicitsVersion()
        {
            // Arrange
            var srcDir = Path.Combine(_testDirPath, "phpappdir");
            Directory.CreateDirectory(srcDir);
            File.WriteAllText(Path.Combine(srcDir, PhpConstants.ComposerFileName), "{\"require\":{\"php\":\"5.6.0\"}}");

            var cmd = new DetectCommand
            {
                SourceDir = srcDir,
            };
            var testConsole = new TestConsole();

            // Act
            int exitCode = cmd.Execute(GetServiceProvider(cmd), testConsole);

            // Assert
            Assert.Equal(ProcessConstants.ExitSuccess, exitCode);
            Assert.Contains(
                $"Platform: {PlatformName.Php}",
                testConsole.StdOutput);
            Assert.Contains(
                $"Version : 5.6.0",
                testConsole.StdOutput);
        }

        [Fact]
        public void Execute_OutputsJson_NodePlatformAndVersion_WhenJsonFileExplicitsVersion()
        {
            string PackageJsonWithNodeVersion = @"{
              ""name"": ""mynodeapp"",
              ""version"": ""1.0.0"",
              ""description"": ""test app"",
              ""main"": ""server.js"",
              ""scripts"": {
                ""test"": ""echo \""Error: no test specified\"" && exit 1"",
                ""start"": ""node server.js""
              },
              ""author"": ""Dev"",
              ""license"": ""ISC"",
              ""engines"" : { ""node"" : ""6.11.0"" }
            }";
            // Arrange
            var sourceDir = Path.Combine(_testDirPath, "nodeappdir");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, NodeConstants.PackageJsonFileName), PackageJsonWithNodeVersion);

            var detectCommand = new DetectCommand
            {
                SourceDir = sourceDir,
                OutputJson = true,
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = detectCommand.Execute(GetServiceProvider(detectCommand), testConsole);

            // Assert
            Assert.Equal(ProcessConstants.ExitSuccess, exitCode);
            Assert.Contains(
                "App_Path",
                testConsole.StdOutput);
            Assert.Contains(
                "Platform_Data",
                testConsole.StdOutput);
            Assert.Contains(
               $"{PlatformName.Node}",
               testConsole.StdOutput);
            Assert.Contains(
                "6.11.0",
                testConsole.StdOutput);
        }

        [Fact]
        public void Execute_Outputs_MultiplatformNamesAndVersions()
        {
            // Arrange
            var sourceDir = Path.Combine(_testDirPath, "multiappdir");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, NodeConstants.PackageJsonFileName), "\n");
            File.WriteAllText(Path.Combine(sourceDir, PhpConstants.ComposerFileName), "{\"require\":{\"php\":\"5.6.0\"}}");

            var detectCommand = new DetectCommand
            {
                SourceDir = sourceDir,
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = detectCommand.Execute(GetServiceProvider(detectCommand), testConsole);

            // Assert
            Assert.Equal(ProcessConstants.ExitSuccess, exitCode);
            Assert.Contains(
                $"Platform: {PlatformName.Node}",
                testConsole.StdOutput);
            Assert.Contains(
                $"Version : Not Detected",
                testConsole.StdOutput);
            Assert.Contains(
                $"Platform: {PlatformName.Php}",
                testConsole.StdOutput);
            Assert.Contains(
                $"Version : 5.6.0",
                testConsole.StdOutput);
        }

        [Fact]
        public void Execute_OutputsJson_MultiplatformNamesAndNullVersions()
        {
            // Arrange
            var sourceDir = Path.Combine(_testDirPath, "multiappdir");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, NodeConstants.PackageJsonFileName), "\n");
            File.WriteAllText(Path.Combine(sourceDir, PhpConstants.ComposerFileName), "\n");

            var detectCommand = new DetectCommand
            {
                SourceDir = sourceDir,
                OutputJson = true,
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = detectCommand.Execute(GetServiceProvider(detectCommand), testConsole);

            // Assert
            Assert.Equal(ProcessConstants.ExitSuccess, exitCode);
            Assert.Contains(
                "App_Path",
                testConsole.StdOutput);
            Assert.Contains(
                "Platform_Data",
                testConsole.StdOutput);
            Assert.Contains(
                $"{PlatformName.Node}",
                testConsole.StdOutput);
            Assert.Contains(
                $"{PlatformName.Php}",
                testConsole.StdOutput);
            Assert.Contains(
                "Not Detected",
                testConsole.StdOutput);
        }

        private static IServiceProvider GetServiceProvider(DetectCommand cmd)
        {
            var servicesBuilder = new ServiceProviderBuilder()
                .ConfigureServices(services =>
                {
                    var configuration = new ConfigurationBuilder().Build();

                    services.AddSingleton<IConfiguration>(configuration);
                });
            return servicesBuilder.Build();
        }
    }
}