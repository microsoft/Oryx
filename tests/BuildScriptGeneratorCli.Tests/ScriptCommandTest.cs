// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Xunit;

namespace BuildScriptGeneratorCli.Tests
{
    public class ScriptCommandTest : IClassFixture<ScriptCommandTest.TestFixture>
    {
        private static string _testDirPath;

        public ScriptCommandTest(TestFixture testFixutre)
        {
            _testDirPath = testFixutre.RootDirPath;
        }

        [Fact]
        public void ScriptCommand_OnExecute_ShowsHelp_AndExits_WhenSourceFolderIsEmpty()
        {
            // Arrange
            var scriptCommand = new ScriptCommand
            {
                SourceDir = string.Empty
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = scriptCommand.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.NotEqual(0, exitCode);
            Assert.Contains("Usage:", testConsole.StdOutput);
        }

        [Fact]
        public void OnExecute_ShowsHelp_AndExits_WhenSourceDirectoryDoesNotExist()
        {
            // Arrange
            var scriptCommand = new ScriptCommand
            {
                SourceDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = scriptCommand.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.NotEqual(0, exitCode);
            var error = testConsole.StdError;
            Assert.DoesNotContain("Usage:", error);
            Assert.Contains("Could not find the source code folder", error);
        }

        [Fact]
        public void ScriptOnly_OnSuccess_Execute_WritesOnlyScriptContent_ToStandardOutput()
        {
            // Arrange
            const string scriptContent = "script content only";
            var serviceProvider = CreateServiceProvider(
                new TestScriptGenerator(scriptContent),
                scriptOnly: true);
            var scriptCommand = new ScriptCommand();
            var testConsole = new TestConsole(newLineCharacter: string.Empty);

            // Act
            var exitCode = scriptCommand.Execute(serviceProvider, testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Equal(scriptContent, testConsole.StdOutput);
            Assert.Equal(string.Empty, testConsole.StdError);
        }

        [Fact]
        public void ScripOnly_OnSuccess_GeneratesScript_ReplacingCRLF_WithLF()
        {
            // Arrange
            const string scriptContentWithCRLF = "#!/bin/bash\r\necho Hello\r\necho World\r\n";
            var expected = scriptContentWithCRLF.Replace("\r\n", "\n");
            var serviceProvider = CreateServiceProvider(
                new TestScriptGenerator(scriptContentWithCRLF),
                scriptOnly: true);
            var scriptCommand = new ScriptCommand();
            var testConsole = new TestConsole(newLineCharacter: string.Empty);

            // Act
            var exitCode = scriptCommand.Execute(serviceProvider, testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Equal(expected, testConsole.StdOutput);
            Assert.Equal(string.Empty, testConsole.StdError);
        }

        public class TestFixture : IDisposable
        {
            public TestFixture()
            {
                RootDirPath = Path.Combine(
                    Path.GetTempPath(),
                    nameof(BuildCommandTest));

                Directory.CreateDirectory(RootDirPath);
            }

            public string RootDirPath { get; }

            public void Dispose()
            {
                if (Directory.Exists(RootDirPath))
                {
                    try
                    {
                        Directory.Delete(RootDirPath, recursive: true);
                    }
                    catch
                    {
                        // Do not throw in dispose
                    }
                }
            }
        }

        private IServiceProvider CreateServiceProvider(TestScriptGenerator generator, bool scriptOnly)
        {
            var sourceCodeFolder = Path.Combine(_testDirPath, "src");
            Directory.CreateDirectory(sourceCodeFolder);
            var outputFolder = Path.Combine(_testDirPath, "output");
            Directory.CreateDirectory(outputFolder);
            var servicesBuilder = new ServiceProviderBuilder()
                .ConfigureServices(services =>
                {
                    // Add 'test' script generator here as we can control what the script output is rather
                    // than depending on in-built script generators whose script could change overtime causing
                    // this test to be difficult to manage.
                    services.RemoveAll<IScriptGenerator>();
                    services.TryAddEnumerable(
                        ServiceDescriptor.Singleton<IScriptGenerator>(generator));
                    services.AddSingleton<ITempDirectoryProvider>(
                        new TestTempDirectoryProvider(Path.Combine(_testDirPath, "temp")));
                })
                .ConfigureScriptGenerationOptions(o =>
                {
                    o.SourceDir = sourceCodeFolder;
                    o.DestinationDir = outputFolder;
                    o.ScriptOnly = scriptOnly;
                });
            return servicesBuilder.Build();
        }

        private class TestTempDirectoryProvider : ITempDirectoryProvider
        {
            private readonly string _tempDir;

            public TestTempDirectoryProvider(string tempDir)
            {
                _tempDir = tempDir;
            }

            public string GetTempDirectory()
            {
                Directory.CreateDirectory(_tempDir);
                return _tempDir;
            }
        }

        private class TestScriptGenerator : IScriptGenerator
        {
            private readonly string _scriptContent;

            public TestScriptGenerator()
                : this(scriptContent: null)
            {
            }

            public TestScriptGenerator(string scriptContent)
            {
                _scriptContent = scriptContent;
            }

            public string SupportedLanguageName => "test";

            public IEnumerable<string> SupportedLanguageVersions => new[] { "1.0.0" };

            public bool CanGenerateScript(ScriptGeneratorContext scriptGeneratorContext)
            {
                return true;
            }

            public string GenerateBashScript(ScriptGeneratorContext scriptGeneratorContext)
            {
                if (!string.IsNullOrEmpty(_scriptContent))
                {
                    return _scriptContent;
                }

                return "#!/bin/bash" + Environment.NewLine + "echo Hello World" + Environment.NewLine;
            }
        }
    }
}
