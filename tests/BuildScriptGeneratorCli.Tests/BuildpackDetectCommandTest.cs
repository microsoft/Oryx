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
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    public class BuildpackDetectCommandTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _testDirPath;

        public BuildpackDetectCommandTest(TestTempDirTestFixture testFixture)
        {
            _testDirPath = testFixture.RootDirPath;
        }

        [Fact]
        public void Execute_Returns100_WhenSourceDirIsEmpty()
        {
            // Arrange
            var srcDir = Path.Combine(_testDirPath, "emptydir");
            Directory.CreateDirectory(srcDir);

            var cmd = new BuildpackDetectCommand { SourceDir = srcDir };

            // Act & Assert
            Assert.Equal(
                BuildpackDetectCommand.DetectorFailCode,
                cmd.Execute(GetServiceProvider(cmd), new TestConsole()));
        }

        [Fact(Skip = "Skipping test temporarily")]
        public void Execute_OutputsNode_WhenPackageJsonExists()
        {
            // Arrange
            var srcDir = Path.Combine(_testDirPath, "nodeappdir");
            Directory.CreateDirectory(srcDir);
            File.WriteAllText(Path.Combine(srcDir, NodeConstants.PackageJsonFileName), "\n");

            var cmd = new BuildpackDetectCommand
            {
                SourceDir = srcDir,
                PlanPath = Path.Combine(_testDirPath, "plan.toml")
            };
            var console = new TestConsole();

            // Act
            int exitCode = cmd.Execute(GetServiceProvider(cmd), console);

            // Assert
            Assert.Equal(ProcessConstants.ExitSuccess, exitCode);
            Assert.Contains(
                $"{NodeConstants.PlatformName}=\"{NodeConstants.NodeLtsVersion}\"",
                console.StdOutput);
        }

        [Fact(Skip = "Skipping test temporarily")]
        public void Execute_OutputsPhp_WhenComposerFileExists()
        {
            // Arrange
            var srcDir = Path.Combine(_testDirPath, "phpappdir");
            Directory.CreateDirectory(srcDir);
            File.WriteAllText(Path.Combine(srcDir, PhpConstants.ComposerFileName), "\n");

            var cmd = new BuildpackDetectCommand
            {
                SourceDir = srcDir,
                PlanPath = Path.Combine(_testDirPath, "plan.toml")
            };
            var console = new TestConsole();

            // Act
            int exitCode = cmd.Execute(GetServiceProvider(cmd), console);

            // Assert
            Assert.Equal(ProcessConstants.ExitSuccess, exitCode);
            Assert.Contains(
                $"{PhpConstants.PlatformName}=\"{PhpConstants.DefaultPhpRuntimeVersion}\"",
                console.StdOutput);
        }

        private static IServiceProvider GetServiceProvider(BuildpackDetectCommand cmd)
        {
            var svcProvider = new ServiceProviderBuilder()
                .ConfigureServices(svcs =>
                {
                    var configuration = new ConfigurationBuilder().Build();
                    svcs.AddSingleton<IConfiguration>(configuration);
                    svcs.AddSingleton<INodeVersionProvider, TestNodeVersionProvider>();
                    svcs.AddSingleton<IPhpVersionProvider, TestPhpVersionProvider>();
                })
                .ConfigureScriptGenerationOptions(opts => cmd.ConfigureBuildScriptGeneratorOptions(opts))
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
    }
}
