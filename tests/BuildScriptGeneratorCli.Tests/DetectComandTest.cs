// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;

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
        public void Execute_OutputsNodePlatformAndVersion()
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
            var serviceProvider = detectCommand.GetServiceProvider(testConsole);

            // Act
            var exitCode = detectCommand.Execute(GetServiceProvider(detectCommand), testConsole);

            // Assert
            Assert.Equal(ProcessConstants.ExitSuccess, exitCode);
            Assert.Contains(
                $"{NodeConstants.PlatformName}",
                testConsole.StdOutput);
            Assert.Contains(
                $"{NodeConstants.NodeLtsVersion}",
                testConsole.StdOutput);

        }

        private static IServiceProvider GetServiceProvider(DetectCommand cmd)
        {
            var svcProvider = new ServiceProviderBuilder()
                .ConfigureServices(svcs =>
                {
                    var configuration = new ConfigurationBuilder().Build();
                    svcs.AddSingleton<IConfiguration>(configuration);
                    svcs.AddSingleton<INodeVersionProvider, TestNodeVersionProvider>();
                    svcs.AddSingleton<IPhpVersionProvider, TestPhpVersionProvider>();
                })
                .ConfigureDetectorOptions(opts => cmd.ConfigureDetectorOptions(opts))
                .Build();
            return svcProvider;
        }

        private class TestNodeVersionProvider : INodeVersionProvider
        {
            public PlatformVersionInfo GetVersionInfo()
            {
                return PlatformVersionInfo.CreateOnDiskVersionInfo(
                    new[] { NodeConstants.NodeLtsVersion },
                    defaultVersion: NodeConstants.NodeLtsVersion);
            }
        }

        private class TestPhpVersionProvider : IPhpVersionProvider
        {
            public PlatformVersionInfo GetVersionInfo()
            {
                return PlatformVersionInfo.CreateOnDiskVersionInfo(
                    new[] { PhpConstants.DefaultPhpRuntimeVersion },
                    defaultVersion: PhpConstants.DefaultPhpRuntimeVersion);
            }
        }

        // Work in process
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