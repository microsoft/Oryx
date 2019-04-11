// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.IO;
using Xunit;
using Microsoft.Oryx.Tests.Common;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGeneratorCli;

namespace BuildScriptGeneratorCli.Tests
{
    public class BuildpackDetectCommandTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _testDirPath;

        public BuildpackDetectCommandTest(TestTempDirTestFixture testFixture)
        {
            _testDirPath = testFixture.RootDirPath;
        }

        [Fact]
        public void Execute_OutputsNode_WhenPackageJsonExists()
        {
            // Arrange
            var cmd = new BuildpackDetectCommand
            {
                SourceDir = _testDirPath,
                PlatformDir = string.Empty,
                PlanPath = string.Empty
            };
            var console = new TestConsole();
            var svcProvider = GetServiceProvider(cmd);

            // Act
            int exitCode;

            // Assert with an empty app directory
            Assert.Equal(BuildpackDetectCommand.DetectorFailCode, cmd.Execute(svcProvider, console));

            // Add file to app directory and assert again
            File.WriteAllText(Path.Combine(_testDirPath, NodeConstants.PackageJsonFileName), "\n");
            exitCode = cmd.Execute(svcProvider, console);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Contains(
                $"{NodeConstants.NodeJsName}=\"{NodeScriptGeneratorOptionsSetup.NodeLtsVersion}\"",
                console.StdOutput);
        }

        private static IServiceProvider GetServiceProvider(BuildpackDetectCommand cmd)
        {
            var env = new TestEnvironment();
            env.SetEnvironmentVariable("NODE_SUPPORTED_VERSIONS", NodeScriptGeneratorOptionsSetup.NodeLtsVersion);

            var svcProvider = new ServiceProviderBuilder()
                .ConfigureServices(svcs => svcs.Replace(ServiceDescriptor.Singleton(typeof(IEnvironment), env)))
                .ConfigureScriptGenerationOptions(opts => cmd.ConfigureBuildScriptGeneratorOptions(opts))
                .Build();
            return svcProvider;
        }
    }
}
