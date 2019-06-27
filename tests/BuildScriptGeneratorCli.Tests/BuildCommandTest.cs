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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    public class BuildCommandTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _testDirPath;
        private readonly TestTempDirTestFixture _testDir;

        public BuildCommandTest(TestTempDirTestFixture testFixture)
        {
            _testDir = testFixture;
            _testDirPath = testFixture.RootDirPath;
        }

        [Fact]
        public void OnExecute_ShowsErrorAndExits_WhenSourceDirectoryDoesNotExist()
        {
            // Arrange
            var buildCommand = new BuildCommand
            {
                SourceDir = _testDir.GenerateRandomChildDirPath(),
                DestinationDir = _testDir.GenerateRandomChildDirPath(),
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = buildCommand.OnExecute(new CommandLineApplication(testConsole), testConsole);

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
            var scriptCommand = new BuildCommand { SourceDir = string.Empty };
            var testConsole = new TestConsole();

            // Act
            BuildScriptGeneratorOptions opts = new BuildScriptGeneratorOptions();
            scriptCommand.ConfigureBuildScriptGeneratorOptions(opts);

            // Assert
            Assert.Equal(Directory.GetCurrentDirectory(), opts.SourceDir);
        }

        [Fact]
        public void IsValidInput_IsTrue_EvenIfDestinationDirDoesNotExist()
        {
            // Arrange
            var serviceProvider = new ServiceProviderBuilder()
                .ConfigureScriptGenerationOptions(o =>
                {
                    o.SourceDir = _testDir.CreateChildDir();
                    o.DestinationDir = _testDir.GenerateRandomChildDirPath();
                })
                .Build();
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand();

            // Act
            var isValid = buildCommand.IsValidInput(serviceProvider, testConsole);

            // Assert
            Assert.True(isValid);
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        [Fact]
        public void IsValidInput_IsTrue_EvenIfDestinationDirExists_AndIsEmpty()
        {
            // Arrange
            var serviceProvider = new ServiceProviderBuilder()
                .ConfigureScriptGenerationOptions(o =>
                {
                    o.SourceDir = _testDir.CreateChildDir();
                    o.DestinationDir = _testDir.CreateChildDir();
                })
                .Build();
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand();

            // Act
            var isValid = buildCommand.IsValidInput(serviceProvider, testConsole);

            // Assert
            Assert.True(isValid);
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        public static TheoryData<string, string> IsSubDirectoryTrueData
        {
            get
            {
                var data = new TheoryData<string, string>
                {
                    {
                        Path.Combine("c:", "foo"),
                        Path.Combine("c:", "foo")
                    },
                    {
                        Path.Combine("c:", "foo") + Path.DirectorySeparatorChar,
                        Path.Combine("c:", "foo")
                    },
                    {
                        Path.Combine("c:", "foo"),
                        Path.Combine("c:", "foo") + Path.DirectorySeparatorChar
                    },
                    {
                        Path.Combine("c:", "foo") + Path.DirectorySeparatorChar,
                        Path.Combine("c:", "foo") + Path.DirectorySeparatorChar
                    },
                    {
                        Path.Combine("c:", "foo", "bar"),
                        Path.Combine("c:", "foo")
                    },
                    {
                        Path.Combine("c:", "foo", "bar", "dir1", "dir2"),
                        Path.Combine("c:", "foo")
                    },
                    {
                        Path.Combine(Path.DirectorySeparatorChar.ToString(), "foo"),
                        Path.DirectorySeparatorChar.ToString()
                    },
                    {
                        Path.GetFullPath(Path.Combine("a", "b", "c", "d", "..")),
                        Path.GetFullPath(Path.Combine("a", "b"))
                    },
                };

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(IsSubDirectoryTrueData))]
        public void IsSubDirectory_IsTrue(string dir1, string dir2)
        {
            // Arrange
            var buildCommand = new BuildCommand();

            // Act
            var isSubDirectory = buildCommand.IsSubDirectory(dir1, dir2);

            // Assert
            Assert.True(isSubDirectory);
        }

        public static TheoryData<string, string> IsSubDirectoryFalseData
        {
            get
            {
                var data = new TheoryData<string, string>
                {
                    {
                        // case-sensitive
                        Path.Combine("c:", "Foo"),
                        Path.Combine("c:", "foo")
                    },
                    {
                        Path.Combine("c:", "foo"),
                        Path.Combine("c:", "foo", "bar")
                    },
                    {
                        Path.Combine("a", "b", "c"),
                        Path.Combine("a", "b", "cd")
                    },
                    {
                        Path.DirectorySeparatorChar.ToString(),
                        Path.Combine(Path.DirectorySeparatorChar.ToString(), "foo")
                    },
                    {
                        Path.GetFullPath(Path.Combine("a", "b", "c", "..", "..")),
                        Path.GetFullPath(Path.Combine("a", "b"))
                    },
                };

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(IsSubDirectoryFalseData))]
        public void IsSubDirectory_IsFalse(string dir1, string dir2)
        {
            // Arrange
            var buildCommand = new BuildCommand();

            // Act
            var isSubDirectory = buildCommand.IsSubDirectory(dir1, dir2);

            // Assert
            Assert.False(isSubDirectory);
        }

        [Fact]
        public void IsValidInput_IsTrue_EvenIfDestinationDirIsNotEmpty()
        {
            // Arrange
            var dstDir = _testDir.CreateChildDir();
            File.WriteAllText(Path.Combine(dstDir, "bla.txt"), "bla");

            var serviceProvider = new ServiceProviderBuilder()
                .ConfigureScriptGenerationOptions(o =>
                {
                    o.SourceDir = _testDir.CreateChildDir();
                    o.DestinationDir = dstDir;
                })
                .Build();
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand();

            // Act
            var isValid = buildCommand.IsValidInput(serviceProvider, testConsole);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void DoesNotShowHelp_EvenIfIntermediateDir_DoesNotExistYet()
        {
            // Arrange
            var buildCommand = new CustomBuildCommand
            {
                SourceDir = _testDir.CreateChildDir(),
                DestinationDir = _testDir.CreateChildDir(),
                // New directory which does not exist yet
                IntermediateDir = _testDir.GenerateRandomChildDirPath()
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = buildCommand.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        // We want to test that only build output is visible on standard output stream when a build happens
        // successfully. But for this we cannot rely on the built-in generators as their content could change
        // making this test unreliable. So we use a test generator which always outputs content that we know for
        // sure wouldn't change. Since we cannot update product code with test generator we cannot run this test in
        // a docker container. So we run this test on a Linux OS only as build sets execute permission flag and
        // as well as executes a bash script.
        [EnableOnPlatform("LINUX")]
        public void OnSuccess_Execute_WritesOnlyBuildOutput_ToStandardOutput()
        {
            // Arrange
            var stringToPrint = "Hello World";
            var script = $"#!/bin/bash\necho {stringToPrint}\n";
            var serviceProvider = CreateServiceProvider(
                new TestProgrammingPlatform("test", new[] { "1.0.0" }, true, script, new TestLanguageDetector()),
                scriptOnly: false);
            var buildCommand = new BuildCommand();
            var testConsole = new TestConsole(newLineCharacter: string.Empty);

            // Act
            var exitCode = buildCommand.Execute(serviceProvider, testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Equal(string.Empty, testConsole.StdError);
            Assert.Contains(stringToPrint, testConsole.StdOutput.Replace(Environment.NewLine, string.Empty));
        }

        [Theory]
        [InlineData("")]
        [InlineData("subdir1")]
        [InlineData("subdir1", "subdir2")]
        public void IsValid_IsFalse_IfIntermediateDir_IsSubDirectory_OfSourceDir(params string[] paths)
        {
            // Arrange
            var sourceDir = _testDir.CreateChildDir();
            var subPaths = Path.Combine(paths);
            var intermediateDir = Path.Combine(sourceDir, subPaths);
            var serviceProvider = new ServiceProviderBuilder()
                .ConfigureScriptGenerationOptions(o =>
                {
                    o.SourceDir = sourceDir;
                    o.IntermediateDir = intermediateDir;
                    o.DestinationDir = _testDir.CreateChildDir();
                })
                .Build();
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand();

            // Act
            var isValid = buildCommand.IsValidInput(serviceProvider, testConsole);

            // Assert
            Assert.False(isValid);
            Assert.Contains(
                $"Intermediate directory '{intermediateDir}' cannot be a " +
                $"sub-directory of source directory '{sourceDir}'.",
                testConsole.StdError);
        }

        [Fact]
        public void IsValid_IsFalse_IfLanguageVersionSpecified_WithoutLanguageName()
        {
            // Arrange
            var serviceProvider = new ServiceProviderBuilder()
                .ConfigureScriptGenerationOptions(o =>
                {
                    o.SourceDir = _testDir.CreateChildDir();
                    o.DestinationDir = _testDir.CreateChildDir();
                    o.Language = null;
                    o.LanguageVersion = "1.0.0";
                })
                .Build();
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand();

            // Act
            var isValid = buildCommand.IsValidInput(serviceProvider, testConsole);

            // Assert
            Assert.False(isValid);
            Assert.Contains(
                "Cannot use language version without specifying language name also.",
                testConsole.StdError);
        }

        public static IEnumerable<object[]> GetOperationNameEnvVarNames()
        {
            return Common.LoggingConstants.OperationNameSourceEnvVars
                .Select(e => new[] { e.Value, Common.LoggingConstants.EnvTypeOperationNamePrefix[e.Key] });
        }

        [Theory]
        [MemberData(nameof(GetOperationNameEnvVarNames))]
        public void BuildOperationName_ReturnsCorrectPrefix(string envVarName, string opNamePrefix)
        {
            // Arrange
            var appName = "bla";
            var env = new TestEnvironment();
            env.SetEnvironmentVariable(envVarName, appName);

            // Act & Assert
            Assert.Equal(opNamePrefix + ":" + appName, BuildCommand.BuildOperationName(env));
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

        private class CustomBuildCommand : BuildCommand
        {
            internal override int Execute(IServiceProvider serviceProvider, IConsole console)
            {
                return 0;
                //return base.Execute(serviceProvider, console);
            }
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

        private class TestScriptExecutor : IScriptExecutor
        {
            public string ScriptPath { get; private set; }
            public string[] Args { get; private set; }
            public bool ExecuteScriptCalled { get; private set; }
            public int ReturnExitCode { get; }

            public TestScriptExecutor(int returnExitCode)
            {
                ReturnExitCode = returnExitCode;
            }

            public int ExecuteScript(
                string scriptPath,
                string[] args,
                string workingDirectory,
                DataReceivedEventHandler stdOutHandler,
                DataReceivedEventHandler stdErrHandler)
            {
                ScriptPath = scriptPath;
                Args = args;
                ExecuteScriptCalled = true;
                return ReturnExitCode;
            }
        }
    }
}