// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Oryx.Tests.Common;
using Xunit;

namespace BuildScriptGeneratorCli.Tests
{
    public class ScriptCommandTest : IClassFixture<TestTempDirTestFixture>
    {
        private static string _testDirPath;

        public ScriptCommandTest(TestTempDirTestFixture testFixture)
        {
            _testDirPath = testFixture.RootDirPath;
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
        public void Configure_UsesCurrentDirectory_WhenSourceDirectoryNotSupplied()
        {
            // Arrange
            var scriptCommand = new ScriptCommand { SourceDir = string.Empty };
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
                new TestProgrammingPlatform(scriptContent),
                scriptOnly: true);
            var scriptCommand = new ScriptCommand();
            var testConsole = new TestConsole(newLineCharacter: string.Empty);

            // Act
            var exitCode = scriptCommand.Execute(serviceProvider, testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Contains(scriptContent, testConsole.StdOutput);
            Assert.Equal(string.Empty, testConsole.StdError);
        }

        [Fact]
        public void ScriptOnly_OnSuccess_GeneratesScript_ReplacingCRLF_WithLF()
        {
            // Arrange
            const string scriptContentWithCRLF = "#!/bin/bash\r\necho Hello\r\necho World\r\n";
            var expected = scriptContentWithCRLF.Replace("\r\n", "\n");
            var serviceProvider = CreateServiceProvider(
                new TestProgrammingPlatform(scriptContentWithCRLF),
                scriptOnly: true);
            var scriptCommand = new ScriptCommand();
            var testConsole = new TestConsole(newLineCharacter: string.Empty);

            // Act
            var exitCode = scriptCommand.Execute(serviceProvider, testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Contains(expected, testConsole.StdOutput);
            Assert.Equal(string.Empty, testConsole.StdError);
        }

        private IServiceProvider CreateServiceProvider(TestProgrammingPlatform generator, bool scriptOnly)
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

                    services.RemoveAll<ILanguageDetector>();
                    services.TryAddEnumerable(
                        ServiceDescriptor.Singleton<ILanguageDetector, TestLanguageDetector>());

                    services.RemoveAll<IProgrammingPlatform>();
                    services.TryAddEnumerable(
                        ServiceDescriptor.Singleton<IProgrammingPlatform>(generator));

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

        private class TestProgrammingPlatform : IProgrammingPlatform
        {
            private readonly string _scriptContent;

            public TestProgrammingPlatform()
                : this(scriptContent: null)
            {
            }

            public TestProgrammingPlatform(string scriptContent)
            {
                _scriptContent = scriptContent;
            }

            public string Name => "test";

            public IEnumerable<string> SupportedLanguageVersions => new[] { "1.0.0" };

            public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
            {
                return new LanguageDetectorResult
                {
                    Language = Name,
                    LanguageVersion = SupportedLanguageVersions.First()
                };
            }

            public BuildScriptSnippet GenerateBashBuildScriptSnippet(ScriptGeneratorContext scriptGeneratorContext)
            {
                string script;
                if (string.IsNullOrEmpty(_scriptContent))
                {
                    script = "#!/bin/bash" + Environment.NewLine + "echo Hello World" + Environment.NewLine;
                }
                else
                {
                    script = _scriptContent;
                }
                return new BuildScriptSnippet()
                {
                    BashBuildScriptSnippet = script
                };
            }

            public bool IsEnabled(ScriptGeneratorContext scriptGeneratorContext)
            {
                return true;
            }

            public void SetRequiredTools(ISourceRepo sourceRepo, string targetPlatformVersion, IDictionary<string, string> toolsToVersion)
            {
            }

            public void SetVersion(ScriptGeneratorContext context, string version)
            {
            }
        }

        private class TestLanguageDetector : ILanguageDetector
        {
            public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
            {
                return new LanguageDetectorResult
                {
                    Language = "test",
                    LanguageVersion = "1.0.0"
                };
            }
        }
    }
}