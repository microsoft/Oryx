// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Xunit;

namespace BuildScriptGeneratorCli.Tests
{
    public class BuildCommandTest : IClassFixture<BuildCommandTest.TestFixture>
    {
        private static string _testDirPath;

        public BuildCommandTest(TestFixture testFixutre)
        {
            _testDirPath = testFixutre.RootDirPath;
        }

        [Fact]
        public void OnExecute_ShowsHelp_AndExits_WhenSourceDirIsNull()
        {
            // Arrange
            var buildCommand = new BuildCommand
            {
                SourceDir = null
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = buildCommand.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.NotEqual(0, exitCode);
            Assert.Contains("Usage:", testConsole.StdOutput);
        }

        [Fact]
        public void OnExecute_ShowsHelp_AndExits_WhenSourceDirectoryDoesNotExist()
        {
            // Arrange
            var buildCommand = new BuildCommand
            {
                SourceDir = CreatePathForNewDir(),
                DestinationDir = CreatePathForNewDir(),
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
        public void IsValidInput_IsTrue_EvenIfDestinationDirDoesNotExist()
        {
            // Arrange
            var serviceProvider = new ServiceProviderBuilder()
                .ConfigureScriptGenerationOptions(o =>
                {
                    o.SourceDir = CreateNewDir();
                    o.DestinationDir = CreatePathForNewDir();
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
                    o.SourceDir = CreateNewDir();
                    o.DestinationDir = CreateNewDir();
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

        public static TheoryData<string> DestinationDirectoryPathData
        {
            get
            {
                var data = new TheoryData<string>();

                // Sub-directory with a file
                var destinationDir = Directory.CreateDirectory(
                    Path.Combine(_testDirPath, Guid.NewGuid().ToString()));
                var subDir = Directory.CreateDirectory(
                    Path.Combine(destinationDir.FullName, Guid.NewGuid().ToString()));
                File.WriteAllText(Path.Combine(subDir.FullName, "file1.txt"), "file1 content");
                data.Add(destinationDir.FullName);

                // Sub-directory which is empty
                destinationDir = Directory.CreateDirectory(
                    Path.Combine(_testDirPath, Guid.NewGuid().ToString()));
                subDir = Directory.CreateDirectory(
                    Path.Combine(destinationDir.FullName, Guid.NewGuid().ToString()));
                data.Add(destinationDir.FullName);

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(DestinationDirectoryPathData))]
        public void IsValidInput_IsTrue_IfDestinationDirIsNotEmpty(
            string destinationDir)
        {
            // Arrange
            var serviceProvider = new ServiceProviderBuilder()
                .ConfigureScriptGenerationOptions(o =>
                {
                    o.SourceDir = CreateNewDir();
                    o.DestinationDir = destinationDir;
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
                SourceDir = CreateNewDir(),
                DestinationDir = CreateNewDir(),
                // New directory which does not exist yet
                IntermediateDir = CreatePathForNewDir()
            };
            var testConsole = new TestConsole();

            // Act
            var exitCode = buildCommand.OnExecute(new CommandLineApplication(testConsole), testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        [Fact]
        public void ResolvesToCurrentDirectoryAbsolutePaths_WhenDotNotationIsUsed()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var currentDir = Directory.GetCurrentDirectory();
            var buildCommand = new BuildCommand
            {
                SourceDir = ".",
                DestinationDir = ".",
                IntermediateDir = ".",
                LogFile = "logFile.txt",
            };

            // Act
            buildCommand.ConfigureBuildScriptGeneratorOptoins(options);

            // Assert
            Assert.Equal(currentDir, options.SourceDir);
            Assert.Equal(currentDir, options.DestinationDir);
            Assert.Equal(currentDir, options.IntermediateDir);
            Assert.Equal(Path.Combine(currentDir, "logFile.txt"), options.LogFile);
        }

        [Theory]
        [InlineData("dir1")]
        [InlineData("dir1", "dir2")]
        public void ResolvesToAbsolutePaths_WhenRelativePathsAreGiven(params string[] paths)
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var providedPath = Path.Combine(paths);
            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), providedPath);
            var logFile = Path.Combine(Directory.GetCurrentDirectory(), "logFile.txt");
            var buildCommand = new BuildCommand
            {
                SourceDir = providedPath,
                DestinationDir = providedPath,
                IntermediateDir = providedPath,
                LogFile = "logFile.txt",
            };

            // Act
            buildCommand.ConfigureBuildScriptGeneratorOptoins(options);

            // Assert
            Assert.Equal(absolutePath, options.SourceDir);
            Assert.Equal(absolutePath, options.DestinationDir);
            Assert.Equal(absolutePath, options.IntermediateDir);
            Assert.Equal(logFile, options.LogFile);
        }

        [Fact]
        public void ResolvesToAbsolutePaths_WhenAbsolutePathsAreGiven()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var absolutePath = Path.GetTempPath();
            var logFile = Path.Combine(Path.GetTempPath(), "logFile.txt");
            var buildCommand = new BuildCommand
            {
                SourceDir = absolutePath,
                DestinationDir = absolutePath,
                IntermediateDir = absolutePath,
                LogFile = logFile,
            };

            // Act
            buildCommand.ConfigureBuildScriptGeneratorOptoins(options);

            // Assert
            Assert.Equal(absolutePath, options.SourceDir);
            Assert.Equal(absolutePath, options.DestinationDir);
            Assert.Equal(absolutePath, options.IntermediateDir);
            Assert.Equal(logFile, options.LogFile);
        }

        [Fact]
        public void ResolvesToAbsolutePaths_WhenDoubleDotNotationIsUsed_RelativeToCurrentDir()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var currentDir = Directory.GetCurrentDirectory();
            var expected = new DirectoryInfo(currentDir).Parent.FullName;
            var buildCommand = new BuildCommand
            {
                SourceDir = "..",
                DestinationDir = "..",
                IntermediateDir = "..",
                LogFile = Path.Combine("..", "logFile.txt"),
            };

            // Act
            buildCommand.ConfigureBuildScriptGeneratorOptoins(options);

            // Assert
            Assert.Equal(expected, options.SourceDir);
            Assert.Equal(expected, options.DestinationDir);
            Assert.Equal(expected, options.IntermediateDir);
            Assert.Equal(Path.Combine(expected, "logFile.txt"), options.LogFile);
        }

        [Fact]
        public void ResolvesToAbsolutePaths_WhenDoubleDotNotationIsUsed()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions();
            var dir1 = CreateNewDir();
            var dir2 = Directory.CreateDirectory(Path.Combine(dir1, "subDir1")).FullName;
            var expected = Directory.CreateDirectory(Path.Combine(dir1, "subDir2")).FullName;
            var buildCommand = new BuildCommand
            {
                SourceDir = Path.Combine(dir2, "..", "subDir2"),
                DestinationDir = Path.Combine(dir2, "..", "subDir2"),
                IntermediateDir = Path.Combine(dir2, "..", "subDir2"),
                LogFile = Path.Combine(dir2, "..", "subDir2", "logFile.txt"),
            };

            // Act
            buildCommand.ConfigureBuildScriptGeneratorOptoins(options);

            // Assert
            Assert.Equal(expected, options.SourceDir);
            Assert.Equal(expected, options.DestinationDir);
            Assert.Equal(expected, options.IntermediateDir);
            Assert.Equal(Path.Combine(expected, "logFile.txt"), options.LogFile);
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
            var serviceProvider = CreateServiceProvider(new TestScriptGenerator(), scriptOnly: false);
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();
            var buildCommand = new BuildCommand();
            var testConsole = new TestConsole(newLineCharacter: string.Empty);

            // Act
            var exitCode = buildCommand.Execute(
                serviceProvider,
                testConsole,
                stdOutHandler: (sender, args) =>
                {
                    outputBuilder.AppendLine(args.Data);
                },
                stdErrHandler: (sender, args) =>
                {
                    errorBuilder.AppendLine(args.Data);
                });

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Equal(string.Empty, testConsole.StdOutput);
            Assert.Equal(string.Empty, testConsole.StdError);
            Assert.Equal("Hello World", outputBuilder.ToString().Replace(Environment.NewLine, string.Empty));
            Assert.Equal(string.Empty, errorBuilder.ToString().Replace(Environment.NewLine, string.Empty));
        }

        [Fact]
        public void ScriptOnly_OnSuccess_Execute_WritesOnlyScriptContent_ToStandardOutput()
        {
            // Arrange
            const string scriptContent = "script content only";
            var serviceProvider = CreateServiceProvider(
                new TestScriptGenerator(scriptContent),
                scriptOnly: true);
            var buildCommand = new BuildCommand();
            var testConsole = new TestConsole(newLineCharacter: string.Empty);

            // Act
            var exitCode = buildCommand.Execute(serviceProvider, testConsole);

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
            var buildCommand = new BuildCommand();
            var testConsole = new TestConsole(newLineCharacter: string.Empty);

            // Act
            var exitCode = buildCommand.Execute(serviceProvider, testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Equal(expected, testConsole.StdOutput);
            Assert.Equal(string.Empty, testConsole.StdError);
        }

        [Fact]
        public void IsValid_IsFalse_IfLanguageVersionSpecified_WithoutLanguageName()
        {
            // Arrange
            var serviceProvider = new ServiceProviderBuilder()
                .ConfigureScriptGenerationOptions(o =>
                {
                    o.SourceDir = CreateNewDir();
                    o.DestinationDir = CreateNewDir();
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

        private string CreateNewDir()
        {
            return Directory.CreateDirectory(CreatePathForNewDir()).FullName;
        }

        private string CreatePathForNewDir()
        {
            return Path.Combine(_testDirPath, Guid.NewGuid().ToString());
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

        public class EnableOnPlatformAttribute : FactAttribute
        {
            private readonly OSPlatform _platform;

            public EnableOnPlatformAttribute(string platform)
            {
                _platform = OSPlatform.Create(platform);

                if (!RuntimeInformation.IsOSPlatform(_platform))
                {
                    Skip = $"This test can only run on platform '{_platform}'.";
                }
            }
        }
    }
}
