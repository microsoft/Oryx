// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using System.IO;
using Xunit;
using Microsoft.Oryx.Tests.Common;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Common;
using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator;

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
            File.WriteAllText(Path.Combine(_testDirPath, NodeConstants.PackageJsonFileName), "\n");

            var detectCommand = new BuildpackDetectCommand { PlatformDir = string.Empty, PlanPath = string.Empty };
            var testConsole = new TestConsole();

            var env = new TestEnvironment();
            env.SetEnvironmentVariable("NODE_SUPPORTED_VERSIONS", NodeScriptGeneratorOptionsSetup.NodeLtsVersion);

            var svcProvider = new ServiceProviderBuilder()
                .ConfigureServices(svcs => svcs.Replace(ServiceDescriptor.Singleton(typeof(IEnvironment), env)))
                .ConfigureScriptGenerationOptions(opts => detectCommand.ConfigureBuildScriptGeneratorOptions(opts))
                .Build();

            // Act
            int exitCode;
            using (new CurrentDirectoryChange(_testDirPath))
            {
                exitCode = detectCommand.Execute(svcProvider, testConsole);
            }

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Contains(
                $"{NodeConstants.NodeJsName}=\"{NodeScriptGeneratorOptionsSetup.NodeLtsVersion}\"",
                testConsole.StdOutput);
        }
    }
}
