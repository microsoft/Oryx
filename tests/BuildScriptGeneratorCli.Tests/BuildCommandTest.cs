// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Ini;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.BuildScriptGeneratorCli.Options;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    public class BuildCommandTest : IClassFixture<TestTempDirTestFixture>
    {
        private const string OS_TYPE_FILE_PATH = "/opt/oryx/.ostype";

        private readonly string _testDirPath;
        private readonly TestTempDirTestFixture _testDir;

        public BuildCommandTest(TestTempDirTestFixture testFixture)
        {
            _testDir = testFixture;
            _testDirPath = testFixture.RootDirPath;
        }

        [Fact]
        public void Options_HasCurrentDirectory_WhenSourceDirectoryNotSupplied()
        {
            // Arrange
            var buildCommand = new BuildCommand { SourceDir = string.Empty };
            var testConsole = new TestConsole();
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;

            // Assert
            Assert.Equal(Directory.GetCurrentDirectory(), options.SourceDir);
        }

        [Fact]
        public void IsValidInput_IsTrue_EvenIfDestinationDirDoesNotExist()
        {
            // Arrange
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand
            {
                SourceDir = _testDir.CreateChildDir(),
                DestinationDir = _testDir.GenerateRandomChildDirPath()
            };
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var isValid = buildCommand.IsValidInput(serviceProvider, testConsole);

            // Assert
            Assert.True(isValid);
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        [Fact]
        public void IsValidInput_ShowsWarning_WhenDeprecatedOptionUsed()
        {
            // Arrange
            var testConsole = new TestConsole();
            var expectedPlatformName = "test";
            var expectedPlatformVersion = "1.0.0";
            var buildCommand = new BuildCommand
            {
                LanguageName = expectedPlatformName,
                LanguageVersion = expectedPlatformVersion,
                SourceDir = _testDir.CreateChildDir(),
                DestinationDir = _testDir.GenerateRandomChildDirPath(),
            };
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var isValid = buildCommand.IsValidInput(serviceProvider, testConsole);

            // Assert
            Assert.True(isValid);
            Assert.Contains("deprecated option '--language'", testConsole.StdOutput);
            Assert.Contains("deprecated option '--language-version'", testConsole.StdOutput);
            Assert.Contains(expectedPlatformName, buildCommand.PlatformName);
            Assert.Contains(expectedPlatformVersion, buildCommand.PlatformVersion);
        }

        [Fact]
        public void IsValidInput_IsTrue_EvenIfDestinationDirExists_AndIsEmpty()
        {
            // Arrange
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand
            {
                SourceDir = _testDir.CreateChildDir(),
                DestinationDir = _testDir.CreateChildDir(),
            };
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var isValid = buildCommand.IsValidInput(serviceProvider, testConsole);

            // Assert
            Assert.True(isValid);
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        [Theory]
        [InlineData("functions")]
        [InlineData("static-sites")]
        [InlineData("staTic-Sites")]
        [InlineData("FUNCTIONS")]
        [InlineData("")]
        [InlineData(null)]
        public void IsValidInput_IsTrue_WhenApptype_Has_Valid_Input(string input)
        {
            // Arrange
            var dstDir = _testDir.CreateChildDir();
            File.WriteAllText(Path.Combine(dstDir, "bla.txt"), "bla");
            var buildCommand = new BuildCommand
            {
                AppType = input,
            };
            var testConsole = new TestConsole();
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var isValid = buildCommand.IsValidInput(serviceProvider, testConsole);

            // Assert
            Assert.True(isValid);
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        [Theory]
        [InlineData("asdad")]
        [InlineData("  ")]
        public void IsValidInput_IsFalse_WhenApptype_Has_InValid_Input(string input)
        {
            // Arrange
            var dstDir = _testDir.CreateChildDir();
            File.WriteAllText(Path.Combine(dstDir, "bla.txt"), "bla");
            var buildCommand = new BuildCommand
            {
                SourceDir = _testDir.CreateChildDir(),
                AppType = input,
            };
            var testConsole = new TestConsole();
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var isValid = buildCommand.IsValidInput(serviceProvider, testConsole);

            // Assert
            Assert.False(isValid);
            Assert.Empty(testConsole.StdOutput);
            Assert.NotEmpty(testConsole.StdError);
        }
        [Fact]
        public void IsValidInput_IsTrue_EvenIfDestinationDirIsNotEmpty()
        {
            // Arrange
            var dstDir = _testDir.CreateChildDir();
            File.WriteAllText(Path.Combine(dstDir, "bla.txt"), "bla");
            var buildCommand = new BuildCommand
            {
                SourceDir = _testDir.CreateChildDir(),
                DestinationDir = dstDir,
            };
            var testConsole = new TestConsole();
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

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
            var exitCode = buildCommand.OnExecute(testConsole);

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
                new TestProgrammingPlatform(
                    platformName: "test",
                    platformVersions: new[] { "1.0.0" },
                    canGenerateScript: true,
                    scriptContent: script,
                    detector: new TestPlatformDetectorUsingPlatformName(
                        detectedPlatformName: "test",
                        detectedPlatformVersion: "1.0.0")),
                scriptOnly: false,
                createOsTypeFile: true);
            var buildCommand = new BuildCommand();
            var testConsole = new TestConsole(newLineCharacter: string.Empty);

            // Act
            var exitCode = buildCommand.Execute(serviceProvider, testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Equal(string.Empty, testConsole.StdError);
            Assert.Contains(stringToPrint, testConsole.StdOutput.Replace(Environment.NewLine, string.Empty));
        }

        [EnableOnPlatform("LINUX")]
        public void BuildScriptFails_WhenOstypeFile_NotPresent()
        {
            // Arrange
            var stringToPrint = "Hello World";
            var script = $"#!/bin/bash\necho {stringToPrint}\n";
            var serviceProvider = CreateServiceProvider(
                new TestProgrammingPlatform(
                    platformName: "test",
                    platformVersions: new[] { "1.0.0" },
                    canGenerateScript: true,
                    scriptContent: script,
                    detector: new TestPlatformDetectorUsingPlatformName(
                        detectedPlatformName: "test",
                        detectedPlatformVersion: "1.0.0")),
                scriptOnly: false,
                createOsTypeFile: false);
            var buildCommand = new BuildCommand();
            var testConsole = new TestConsole(newLineCharacter: string.Empty);

            // Act
            var exitCode = buildCommand.Execute(serviceProvider, testConsole);

            // Assert failed with exit code 1
            Assert.Equal(1, exitCode);
            Assert.Contains($"File {OS_TYPE_FILE_PATH} does not exist. Cannot copy to manifest directory.", testConsole.StdError);
        }

        [Fact]
        public void IsValid_IsFalse_IfIntermediateDir_IsSameAsSourceDir()
        {
            // Arrange
            var sourceDir = _testDir.CreateChildDir();
            var buildCommand = new BuildCommand
            {
                SourceDir = sourceDir,
                IntermediateDir = sourceDir,
                DestinationDir = _testDir.CreateChildDir()
            };
            var testConsole = new TestConsole();
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var isValid = buildCommand.IsValidInput(serviceProvider, testConsole);

            // Assert
            Assert.False(isValid);
            Assert.Contains(
                $"Intermediate directory '{sourceDir}' cannot be same " +
                $"as the source directory '{sourceDir}'.",
                testConsole.StdError);
        }

        [Theory]
        [InlineData("subdir1")]
        [InlineData("subdir1", "subdir2")]
        public void IsValid_IsFalse_IfIntermediateDir_IsSubDirectory_OfSourceDir(params string[] paths)
        {
            // Arrange
            var sourceDir = _testDir.CreateChildDir();
            var subPaths = Path.Combine(paths);
            var intermediateDir = Path.Combine(sourceDir, subPaths);
            var buildCommand = new BuildCommand
            {
                SourceDir = sourceDir,
                IntermediateDir = intermediateDir,
                DestinationDir = _testDir.CreateChildDir()
            };
            var testConsole = new TestConsole();
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

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
        public void IsValid_IsFalse_IfPlatformVersionSpecified_WithoutPlatformName()
        {
            // Arrange
            var buildCommand = new BuildCommand
            {
                SourceDir = _testDir.CreateChildDir(),
                DestinationDir = _testDir.CreateChildDir(),
                PlatformName = null,
                PlatformVersion = "1.0.0"
            };
            var testConsole = new TestConsole();
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var isValid = buildCommand.IsValidInput(serviceProvider, testConsole);

            // Assert
            Assert.False(isValid);
            Assert.Contains(
                "Cannot use platform version without specifying platform name also.",
                testConsole.StdError);
        }

        public static IEnumerable<object[]> GetOperationNameEnvVarNames()
        {
            return LoggingConstants.OperationNameSourceEnvVars
                .Select(e => new[] { e.Value, LoggingConstants.EnvTypeOperationNamePrefix[e.Key] });
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

        [Fact]
        public void OptionsHasValueFromCommandLine_OverValueFromBuildEnvFile()
        {
            // Arrange
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand
            {
                SourceDir = _testDir.CreateChildDir(),
                DestinationDir = _testDir.GenerateRandomChildDirPath(),
                PlatformName = "python",
                PlatformVersion = "4.0"
            };
            File.WriteAllText(
                Path.Combine(buildCommand.SourceDir, Constants.BuildEnvironmentFileName), "PYTHON_VERSION=3.7");
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // Assert
            Assert.Equal("4.0", configuration.GetValue<string>("python_version"));
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        [Fact]
        public void ConfigurationProvidersAreInExpectedOrder()
        {
            // Arrange
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand
            {
                SourceDir = _testDir.CreateChildDir(),
                DestinationDir = _testDir.GenerateRandomChildDirPath(),
            };
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // Assert
            var configurationRoot = Assert.IsAssignableFrom<IConfigurationRoot>(configuration);
            Assert.NotNull(configurationRoot.Providers);
            var providers = configurationRoot.Providers.ToArray();
            Assert.Equal(3, providers.Length);
            _ = Assert.IsType<IniConfigurationProvider>(providers[0]);
            Assert.IsType<EnvironmentVariablesConfigurationProvider>(providers[1]);
            Assert.IsType<CustomConfigurationSource>(providers[2]);
        }

        [Fact]
        public void OptionsHasValueFromBuildEnvFile_IfNoValueFoundFromOtherSources()
        {
            // Arrange
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand
            {
                SourceDir = _testDir.CreateChildDir(),
                DestinationDir = _testDir.GenerateRandomChildDirPath(),
            };
            File.WriteAllText(
                Path.Combine(buildCommand.SourceDir, Constants.BuildEnvironmentFileName), "PYTHON_VERSION=100.100");
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // Assert
            Assert.Equal("100.100", configuration.GetValue<string>("python_version"));
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        [Fact]
        public void PythonScriptGeneratorOptions_HasPythonVersionValue_FromPlatformVersionSwitch()
        {
            // Arrange
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand
            {
                SourceDir = _testDir.CreateChildDir(),
                DestinationDir = _testDir.GenerateRandomChildDirPath(),
                PlatformName = "python",
                PlatformVersion = "4.0"
            };
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var options = serviceProvider.GetRequiredService<IOptions<PythonScriptGeneratorOptions>>().Value;
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // Assert
            Assert.Equal("4.0", configuration.GetValue<string>("python_version"));
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        [Fact]
        public void NodeScriptGeneratorOptions_HasNodeVersionValue_FromPlatformVersionSwitch()
        {
            // Arrange
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand
            {
                SourceDir = _testDir.CreateChildDir(),
                DestinationDir = _testDir.GenerateRandomChildDirPath(),
                PlatformName = "nodejs",
                PlatformVersion = "4.0"
            };
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // Assert
            Assert.Equal("4.0", configuration.GetValue<string>("node_version"));
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        [Fact]
        public void PhpScriptGeneratorOptions_HasPhpVersionValue_FromPlatformVersionSwitch()
        {
            // Arrange
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand
            {
                SourceDir = _testDir.CreateChildDir(),
                DestinationDir = _testDir.GenerateRandomChildDirPath(),
                PlatformName = "php",
                PlatformVersion = "4.0"
            };
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // Assert
            Assert.Equal("4.0", configuration.GetValue<string>("php_version"));
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        [Fact]
        public void DotNetCoreScriptGeneratorOptions_HasDotNetCoreVersionValue_FromPlatformVersionSwitch()
        {
            // Arrange
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand
            {
                SourceDir = _testDir.CreateChildDir(),
                DestinationDir = _testDir.GenerateRandomChildDirPath(),
                PlatformName = "dotnet",
                PlatformVersion = "4.0"
            };
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // Assert
            Assert.Equal("4.0", configuration.GetValue<string>("dotnet_version"));
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        [Fact]
        public void BuildProperties_AreSet_ToPropertiesOnNodeScriptGeneratorOptions()
        {
            // Arrange
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand
            {
                SourceDir = _testDir.CreateChildDir(),
                DestinationDir = _testDir.GenerateRandomChildDirPath(),
                PlatformName = "nodejs",
                PlatformVersion = "4.0",
                Properties = new[]
                {
                    $"{NodePlatform.PruneDevDependenciesPropertyKey}={true}",
                    $"{NodePlatform.RegistryUrlPropertyKey}=http://foobar.com/",
                }
            };
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var options = serviceProvider.GetRequiredService<IOptions<NodeScriptGeneratorOptions>>().Value;

            // Assert
            Assert.True(options.PruneDevDependencies);
            Assert.Equal("http://foobar.com/", options.NpmRegistryUrl);
            Assert.Equal("4.0", configuration.GetValue<string>("node_version"));
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        [Fact]
        public void BuildProperties_AreSet_ToPropertiesOnPythonScriptGeneratorOptions()
        {
            // Arrange
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand
            {
                SourceDir = _testDir.CreateChildDir(),
                DestinationDir = _testDir.GenerateRandomChildDirPath(),
                PlatformName = "python",
                PlatformVersion = "4.0",
                Properties = new[]
                {
                    $"{PythonPlatform.VirtualEnvironmentNamePropertyKey}=fooenv",
                    $"{NodePlatform.RegistryUrlPropertyKey}=http://foobar.com/",
                }
            };
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var options = serviceProvider.GetRequiredService<IOptions<PythonScriptGeneratorOptions>>().Value;
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // Assert
            Assert.Equal("fooenv", options.VirtualEnvironmentName);
            Assert.Equal("4.0", configuration.GetValue<string>("python_version"));
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        [Fact]
        public void BuildProperties_AreSet_ToPropertiesOnDotNetCoreScriptGeneratorOptions()
        {
            // Arrange
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand
            {
                SourceDir = _testDir.CreateChildDir(),
                DestinationDir = _testDir.GenerateRandomChildDirPath(),
                PlatformName = "dotnet",
                PlatformVersion = "4.0",
                Properties = new[]
                {
                    $"{DotNetCoreConstants.ProjectBuildPropertyKey}=src/foobar.csproj",
                }
            };
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var options = serviceProvider.GetRequiredService<IOptions<DotNetCoreScriptGeneratorOptions>>().Value;
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // Assert
            Assert.Equal("4.0", configuration.GetValue<string>("dotnet_version"));
            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        [Fact]
        public void BuildScriptGeneratorOptions_HasExpectedDefaultValues()
        {
            // Arrange
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand
            {
                SourceDir = _testDir.CreateChildDir(),
                DestinationDir = _testDir.GenerateRandomChildDirPath(),
            };
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;

            // Assert
            Assert.False(options.EnableMultiPlatformBuild);
            Assert.False(options.EnableDynamicInstall);
            Assert.False(options.ScriptOnly);
            Assert.False(options.ShouldPackage);

            Assert.True(options.EnableCheckers);
            Assert.True(options.EnableDotNetCoreBuild);
            Assert.True(options.EnableNodeJSBuild);
            Assert.True(options.EnablePhpBuild);
            Assert.True(options.EnablePythonBuild);
            Assert.True(options.EnableHugoBuild);
            Assert.True(options.EnableTelemetry);

            Assert.Null(options.PreBuildCommand);
            Assert.Null(options.PreBuildScriptPath);
            Assert.Null(options.PostBuildCommand);
            Assert.Null(options.PostBuildScriptPath);

            Assert.NotNull(options.Properties);
            Assert.Empty(options.Properties);

            Assert.Empty(testConsole.StdOutput);
            Assert.Empty(testConsole.StdError);
        }

        [Fact]
        public void PlatformsAreDisabledAsPerSettings()
        {
            // Arrange
            var testConsole = new TestConsole();
            var buildCommand = new BuildCommand
            {
                SourceDir = _testDir.CreateChildDir(),
                DestinationDir = _testDir.GenerateRandomChildDirPath(),
            };
            var settings = new StringBuilder();
            settings.AppendLine($"{SettingsKeys.DisableDotNetCoreBuild}=true");
            settings.AppendLine($"{SettingsKeys.DisableHugoBuild}=true");
            settings.AppendLine($"{SettingsKeys.DisableNodeJSBuild}=true");
            settings.AppendLine($"{SettingsKeys.DisablePhpBuild}=true");
            settings.AppendLine($"{SettingsKeys.DisablePythonBuild}=true");
            File.WriteAllText(
                Path.Combine(buildCommand.SourceDir, Constants.BuildEnvironmentFileName),
                settings.ToString());
            var serviceProvider = buildCommand.TryGetServiceProvider(testConsole);

            // Act
            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;

            // Assert
            Assert.False(options.EnableDotNetCoreBuild);
            Assert.False(options.EnableNodeJSBuild);
            Assert.False(options.EnablePhpBuild);
            Assert.False(options.EnablePythonBuild);
            Assert.False(options.EnableHugoBuild);
        }

        private IServiceProvider CreateServiceProvider(TestProgrammingPlatform generator, bool scriptOnly, bool createOsTypeFile)
        {
            var sourceCodeFolder = Path.Combine(_testDirPath, "src");
            Directory.CreateDirectory(sourceCodeFolder);
            var outputFolder = Path.Combine(_testDirPath, "output");
            Directory.CreateDirectory(outputFolder);
            if (createOsTypeFile)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(OS_TYPE_FILE_PATH));
                File.Create(OS_TYPE_FILE_PATH).Dispose();
            }
            var servicesBuilder = new ServiceProviderBuilder()
                .ConfigureServices(services =>
                {
                    // Add 'test' script generator here as we can control what the script output is rather
                    // than depending on in-built script generators whose script could change overtime causing
                    // this test to be difficult to manage.
                    services.RemoveAll<IPlatformDetector>();
                    services.TryAddEnumerable(
                        ServiceDescriptor.Singleton<IPlatformDetector>(
                            new TestPlatformDetectorUsingPlatformName(
                                detectedPlatformName: "test",
                                detectedPlatformVersion: "1.0.0")));
                    services.RemoveAll<IProgrammingPlatform>();
                    services.TryAddEnumerable(
                        ServiceDescriptor.Singleton<IProgrammingPlatform>(generator));
                    services.AddSingleton<ITempDirectoryProvider>(
                        new TestTempDirectoryProvider(Path.Combine(_testDirPath, "temp")));
                    var configuration = new ConfigurationBuilder().Build();
                    services.AddSingleton<IConfiguration>(configuration);
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
