// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.Tests.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;

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
            var serviceProvider = GetServiceProvider(detectCommand);

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
            var serviceProvider = GetServiceProvider(detectCommand);

            // Act
            var isValidInput = detectCommand.IsValidInput(serviceProvider, testConsole);

            // Assert
            Assert.False(isValidInput);
            Assert.Contains(
                $"Could not find the source directory",
                testConsole.StdError);
        }

        [Fact]
        public void Execute_OutputsTable_PlatformAndVersionNotDetected()
        {
            // Arrange
            var sourceDir = Path.Combine(_testDirPath, "appdir");
            Directory.CreateDirectory(sourceDir);

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
                $"Platform       : Not Detected",
                testConsole.StdOutput);
            Assert.Contains(
                $"PlatformVersion: Not Detected",
                testConsole.StdOutput);
        }

        [Fact]
        public void Execute_OutputsJson_PlatformAndVersionNotDetected()
        {
            // Arrange
            var sourceDir = Path.Combine(_testDirPath, "appdir");
            Directory.CreateDirectory(sourceDir);

            var detectCommand = new DetectCommand
            {
                SourceDir = sourceDir,
                OutputFormat = "json",
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = detectCommand.Execute(GetServiceProvider(detectCommand), testConsole);

            // Assert
            Assert.Equal(ProcessConstants.ExitSuccess, exitCode);
            Assert.Contains(
                "{}",
                testConsole.StdOutput);
        }

        [Fact]
        public void Execute_OutputsTable_NodePlatform_WithVersionNotDetected_WhenJsonFileIsEmpty()
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
                $"{NodeConstants.PlatformName}",
                testConsole.StdOutput);
            Assert.Contains(
                $"Not Detected",
                testConsole.StdOutput);
        }

        [Fact]
        public void Execute_OutputsTable_PhpPlatformAndVersion_WhenComposerFileExplicitsVersion()
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
                $"{PhpConstants.PlatformName}",
                testConsole.StdOutput);
            Assert.Contains(
                $"5.6.0",
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
                OutputFormat = "json",
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = detectCommand.Execute(GetServiceProvider(detectCommand), testConsole);

            // Assert
            Assert.Equal(ProcessConstants.ExitSuccess, exitCode);
            Assert.Contains(
                $"\"Platform\": \"{NodeConstants.PlatformName}\"",
                testConsole.StdOutput);
            Assert.Contains(
                "\"PlatformVersion\": \"6.11.0\"",
                testConsole.StdOutput);
        }

        [Fact]
        public void Execute_OutputsTable_MultiplatformNamesAndVersions()
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
                $"{NodeConstants.PlatformName}",
                testConsole.StdOutput);
            Assert.Contains(
                $"Not Detected",
                testConsole.StdOutput);
            Assert.Contains(
                $"{PhpConstants.PlatformName}",
                testConsole.StdOutput);
            Assert.Contains(
                $"5.6.0",
                testConsole.StdOutput);
        }

        [Fact]
        public void Execute_OutputsJson_DotNetCorePlatformAndVersionAndProjectFile()
        {
            string ProjectFileWithMultipleProperties = @"
            <Project Sdk=""Microsoft.NET.Sdk.Web"">
              <PropertyGroup>
                <LangVersion>7.3</LangVersion>
              </PropertyGroup>
              <PropertyGroup>
                <TargetFramework>netcoreapp2.1</TargetFramework>
                <LangVersion>7.3</LangVersion>
              </PropertyGroup>
            </Project>";

            // Arrange
            var sourceDir = Path.Combine(_testDirPath, "dotnetcoreappdir");
            var projectFile = "webapp.csproj";
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, projectFile), ProjectFileWithMultipleProperties);

            var detectCommand = new DetectCommand
            {
                SourceDir = sourceDir,
                OutputFormat = "json",
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = detectCommand.Execute(GetServiceProvider(detectCommand), testConsole);

            // Assert
            Assert.Equal(ProcessConstants.ExitSuccess, exitCode);
            Assert.Contains(
                $"\"Platform\": \"{DotNetCoreConstants.PlatformName}\"",
                testConsole.StdOutput);
            Assert.Contains(
                "\"PlatformVersion\": \"2.1\"",
                testConsole.StdOutput);
            Assert.Contains(
                $"\"ProjectFile\": \"{projectFile}\"",
                testConsole.StdOutput);
        }

        [Fact]
        public void Execute_OutputsJson_NodePlatformAndVersionAndFrameworkInfos()
        {
            string PackageJsonWithNodeVersion = @"{
              ""name"": ""mynodeapp"",
              ""version"": ""1.0.0"",
              ""main"": ""server.js"",
              ""devDependencies"": {
                ""aurelia-cli"": ""1.3.1"",
                ""svelte"": ""3.0.0"",
              },
              ""dependencies"": {
                ""jquery"": ""3.5.1"",
                ""react"": ""16.12.0"",
              },
              ""author"": ""Dev"",
              ""engines"" : { ""node"" : ""6.11.0"" }
            }";
            // Arrange
            var sourceDir = Path.Combine(_testDirPath, "nodeappdir");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, NodeConstants.PackageJsonFileName), PackageJsonWithNodeVersion);

            var detectCommand = new DetectCommand
            {
                SourceDir = sourceDir,
                OutputFormat = "json",
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = detectCommand.Execute(GetServiceProvider(detectCommand), testConsole);

            // Assert
            Assert.Equal(ProcessConstants.ExitSuccess, exitCode);
            Assert.Contains(
                $"\"Platform\": \"{NodeConstants.PlatformName}\"",
                testConsole.StdOutput);
            Assert.Contains(
                "\"PlatformVersion\": \"6.11.0\"",
                testConsole.StdOutput);
            Assert.Contains(
                $"\"Framework\": \"Aurelia\"",
                testConsole.StdOutput);
            Assert.Contains(
                $"\"Framework\": \"React\"",
                testConsole.StdOutput);
        }

        [Fact]
        public void Execute_OutputsJson_MultiplatformNames_WithVersionsNotDetected()
        {
            // Arrange
            var sourceDir = Path.Combine(_testDirPath, "multiappdir");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, NodeConstants.PackageJsonFileName), "\n");
            File.WriteAllText(Path.Combine(sourceDir, PhpConstants.ComposerFileName), "\n");

            var detectCommand = new DetectCommand
            {
                SourceDir = sourceDir,
                OutputFormat = "json",
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = detectCommand.Execute(GetServiceProvider(detectCommand), testConsole);

            // Assert
            Assert.Equal(ProcessConstants.ExitSuccess, exitCode);
            Assert.Contains(
                 $"\"Platform\": \"{PhpConstants.PlatformName}\"",
                 testConsole.StdOutput);
            Assert.Contains(
                $"\"Platform\": \"{NodeConstants.PlatformName}\"",
                testConsole.StdOutput);
            Assert.Contains(
                "\"PlatformVersion\": \"\"",
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