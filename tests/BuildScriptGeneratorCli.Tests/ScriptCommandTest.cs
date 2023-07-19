// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.CommandLine;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    public class ScriptCommandTest : ScriptCommandTestBase
    {
        public ScriptCommandTest(TestTempDirTestFixture testFixture) : base(testFixture) { }

        [Fact]
        public void OnExecute_ShowsHelp_AndExits_WhenSourceDirectoryDoesNotExist()
        {
            // Arrange
            var scriptCommand = new BuildScriptCommand
            {
                SourceDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
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
        public void Configure_UsesCurrentDirectory_WhenSourceDirectoryNotSupplied()
        {
            // Arrange
            var scriptCommand = new BuildScriptCommand { SourceDir = string.Empty };
            var testConsole = new TestConsole();

            // Act
            BuildScriptGeneratorOptions opts = new BuildScriptGeneratorOptions();
            scriptCommand.ConfigureBuildScriptGeneratorOptions(opts);

            // Assert
            Assert.Equal(Directory.GetCurrentDirectory(), opts.SourceDir);
        }

        [Fact]
        public void ScriptOnly_OnSuccess_Execute_WritesOnlyScriptContent_ToStandardOutput()
        {
            // Arrange
            const string scriptContent = "script content only";
            var serviceProvider = CreateServiceProvider(
                new TestProgrammingPlatform(
                    platformName: "test",
                    platformVersions: new[] { "1.0.0" },
                    canGenerateScript: true,
                    scriptContent: scriptContent,
                    detector: new TestPlatformDetectorUsingPlatformName(
                        detectedPlatformName: "test",
                        detectedPlatformVersion: "1.0.0")),
                scriptOnly: true);
            var scriptCommand = new BuildScriptCommand();
            var testConsole = new TestConsole(newLineCharacter: string.Empty);

            // Act
            var exitCode = scriptCommand.Execute(serviceProvider, testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Contains(scriptContent, testConsole.StdOutput);
            Assert.Equal(string.Empty, testConsole.StdError);
        }

        [Fact]
        public void ScriptOnly_OnSuccess_Execute_WritesOnlyScriptContent_ToFile()
        {
            // Arrange
            const string scriptContent = "script content only";
            var serviceProvider = CreateServiceProvider(
                new TestProgrammingPlatform(
                    platformName: "test",
                    platformVersions: new[] { "1.0.0" },
                    canGenerateScript: true,
                    scriptContent: scriptContent,
                    detector: new TestPlatformDetectorUsingPlatformName(
                        detectedPlatformName: "test",
                        detectedPlatformVersion: "1.0.0")),
                scriptOnly: true);
            var scriptCommand = new BuildScriptCommand();
            var testConsole = new TestConsole(newLineCharacter: string.Empty);

            var outputDirectory = _testDir.CreateChildDir();
            var scriptPath = Path.Join(outputDirectory, "build.sh");
            scriptCommand.OutputPath = scriptPath;

            // Act
            var exitCode = scriptCommand.Execute(serviceProvider, testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Contains("Script written to", testConsole.StdOutput);
            Assert.Equal(string.Empty, testConsole.StdError);

            var outputFileContent = File.ReadAllText(scriptPath);
            Assert.Contains(scriptContent, outputFileContent);
        }

        [Fact]
        public void ScriptOnly_OnSuccess_GeneratesScript_ReplacingCRLF_WithLF()
        {
            // Arrange
            const string scriptContentWithCRLF = "#!/bin/bash\r\necho Hello\r\necho World\r\n";
            var serviceProvider = CreateServiceProvider(
                new TestProgrammingPlatform(
                    platformName: "test",
                    platformVersions: new[] { "1.0.0" },
                    canGenerateScript: true,
                    scriptContent: scriptContentWithCRLF,
                    detector: new TestPlatformDetectorUsingPlatformName(
                        detectedPlatformName: "test",
                        detectedPlatformVersion: "1.0.0")),
                scriptOnly: true);
            var scriptCommand = new BuildScriptCommand();
            var testConsole = new TestConsole(newLineCharacter: string.Empty);

            // Act
            var exitCode = scriptCommand.Execute(serviceProvider, testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Contains(scriptContentWithCRLF.Replace("\r\n", "\n"), testConsole.StdOutput);
            Assert.Equal(string.Empty, testConsole.StdError);
        }

        [Fact]
        public void ScriptOnly_OnSuccess_GenerateScript_ReplacingCRLF_WithLF_ToFile()
        {
            // Arrange
            const string scriptContentWithCRLF = "#!/bin/bash\r\necho Hello\r\necho World\r\n";
            var serviceProvider = CreateServiceProvider(
                new TestProgrammingPlatform(
                    platformName: "test",
                    platformVersions: new[] { "1.0.0" },
                    canGenerateScript: true,
                    scriptContent: scriptContentWithCRLF,
                    detector: new TestPlatformDetectorUsingPlatformName(
                        detectedPlatformName: "test",
                        detectedPlatformVersion: "1.0.0")),
                scriptOnly: true);
            var scriptCommand = new BuildScriptCommand();
            var testConsole = new TestConsole(newLineCharacter: string.Empty);

            var outputDirectory = _testDir.CreateChildDir();
            var scriptPath = Path.Join(outputDirectory, "build.sh");
            scriptCommand.OutputPath = scriptPath;

            // Act
            var exitCode = scriptCommand.Execute(serviceProvider, testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Contains("Script written to", testConsole.StdOutput);
            Assert.Equal(string.Empty, testConsole.StdError);

            var outputFileContent = File.ReadAllText(scriptPath);
            Assert.Contains(scriptContentWithCRLF.Replace("\r\n", "\n"), outputFileContent);
        }
    }
}