// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Node
{
    public class NodePlatformTest
    {
        [Fact]
        public void GeneratedBuildSnippet_HasCustomNpmRunBuildCommand_EvenIfPackageJsonHasBuildNodes()
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
                ""build"": ""build-node"",
                ""build:azure"": ""azure-node"",
              },
            }";
            var expectedText = "custom-npm-run-build";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions { CustomRunBuildCommand = expectedText },
                new NodePlatformInstaller(Options.Create(commonOptions), new TestEnvironment()));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.NodeVersion = "10.10";

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains(expectedText, buildScriptSnippet.BashBuildScriptSnippet);
            Assert.DoesNotContain("npm run build", buildScriptSnippet.BashBuildScriptSnippet);
            Assert.DoesNotContain("npm run build:azure", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_HasNpmRunBuildCommand()
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
                ""build"": ""build-node"",
              },
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions { CustomRunBuildCommand = null },
                new NodePlatformInstaller(Options.Create(commonOptions), new TestEnvironment()));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.NodeVersion = "10.10";

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.DoesNotContain("npm run build:azure", buildScriptSnippet.BashBuildScriptSnippet);
            Assert.Contains("npm run build", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_HasNpmRunBuildAzureCommand()
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
                ""build:azure"": ""build-azure-node"",
              },
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions { CustomRunBuildCommand = null },
                new NodePlatformInstaller(Options.Create(commonOptions), new TestEnvironment()));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.NodeVersion = "10.10";

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains("npm run build:azure", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void BuildScript_HasSdkInstallScript_IfDynamicInstallIsEnabled_AndSdkIsNotAlreadyInstalled()
        {
            // Arrange
            var nodePlatform = CreateNodePlatform(dynamicInstallIsEnabled: true, sdkAlreadyInstalled: false);
            var repo = new MemorySourceRepo();
            repo.AddFile(string.Empty, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.NodeVersion = "10.10";

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.NotNull(buildScriptSnippet.PlatformInstallationScriptSnippet);
            Assert.Equal(
                TestNodePlatformInstaller.InstallerScript,
                buildScriptSnippet.PlatformInstallationScriptSnippet);
        }

        [Fact]
        public void BuildScript_HasNoSdkInstallScript_IfDynamicInstallIsEnabled_AndSdkIsAlreadyInstalled()
        {
            // Arrange
            var nodePlatform = CreateNodePlatform(dynamicInstallIsEnabled: true, sdkAlreadyInstalled: true);
            var repo = new MemorySourceRepo();
            repo.AddFile(string.Empty, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.NodeVersion = "10.10";

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Null(buildScriptSnippet.PlatformInstallationScriptSnippet);
        }

        [Fact]
        public void BuildScript_DoesNotHaveSdkInstallScript_IfDynamicInstallNotEnabled_AndSdkIsNotAlreadyInstalled()
        {
            // Arrange
            var nodePlatform = CreateNodePlatform(dynamicInstallIsEnabled: false, sdkAlreadyInstalled: false);
            var repo = new MemorySourceRepo();
            repo.AddFile(string.Empty, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.NodeVersion = "10.10";

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Null(buildScriptSnippet.PlatformInstallationScriptSnippet);
        }

        [Theory]
        [InlineData("true")]
        [InlineData("")]
        public void GeneratedBuildSnippet_DoesNotThrowException_IfPackageJsonHasBuildNode_AndRequireBuildPropertyIsSet(
            string requireBuild)
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
                ""build"": ""build-node"",
              },
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions(),
                new NodePlatformInstaller(Options.Create(commonOptions), new TestEnvironment()));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.NodeVersion = "10.10";
            context.Properties[NodePlatform.RequireBuildPropertyKey] = requireBuild;

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains("npm run build", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_DoesNotThrowException_IfPackageJsonHasBuildAzureNode_AndRequireBuildPropertyIsSet()
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
                ""build:azure"": ""azure-node"",
              },
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions(),
                new NodePlatformInstaller(Options.Create(commonOptions), new TestEnvironment()));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.NodeVersion = "10.10";
            context.Properties[NodePlatform.RequireBuildPropertyKey] = "true";

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains("npm run build:azure", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_DoesNotThrowException_IfCustomRunCommandIsProvided_AndRequireBuildPropertyIsSet()
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
              },
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions { CustomRunBuildCommand = "custom command here" },
                new NodePlatformInstaller(Options.Create(commonOptions), new TestEnvironment()));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.NodeVersion = "10.10";
            context.Properties[NodePlatform.RequireBuildPropertyKey] = "true";

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains("custom command here", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_ThrowsException_IfRequireBuildPropertyIsSet_AndNoBuildStepIsProvided()
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
              },
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions(),
                new NodePlatformInstaller(Options.Create(commonOptions), new TestEnvironment()));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.NodeVersion = "10.10";
            context.Properties[NodePlatform.RequireBuildPropertyKey] = "true";

            // Act & Assert
            var exception = Assert.Throws<NoBuildStepException>(
                () => nodePlatform.GenerateBashBuildScriptSnippet(context));
            Assert.Equal(
                "Could not find either 'build' or 'build:azure' node under 'scripts' in package.json. " +
                "Could not find value for custom run build command using the environment variable " +
                "key 'RUN_BUILD_COMMAND'.",
                exception.Message);
        }

        [Fact]
        public void GeneratedBuildSnippet_DoesNotThrowException_IfNoBuildStepIsProvided_AndRequireBuildPropertyIsSet_ToFalse()
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
              },
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions { CustomRunBuildCommand = "custom command here" },
                new NodePlatformInstaller(Options.Create(commonOptions), new TestEnvironment()));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.NodeVersion = "10.10";
            context.Properties[NodePlatform.RequireBuildPropertyKey] = "false";

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains("custom command here", buildScriptSnippet.BashBuildScriptSnippet);
        }

        private TestNodePlatform CreateNodePlatform(
            BuildScriptGeneratorOptions commonOptions,
            NodeScriptGeneratorOptions nodeScriptGeneratorOptions,
            NodePlatformInstaller platformInstaller)
        {
            var environment = new TestEnvironment();

            var versionProvider = new TestNodeVersionProvider();
            var detector = new TestNodePlatformDetector(
                versionProvider,
                Options.Create(nodeScriptGeneratorOptions),
                NullLogger<NodePlatformDetector>.Instance,
                environment,
                new TestStandardOutputWriter());

            return new TestNodePlatform(
                Options.Create(commonOptions),
                Options.Create(nodeScriptGeneratorOptions),
                versionProvider,
                NullLogger<NodePlatform>.Instance,
                detector,
                environment,
                platformInstaller);
        }

        private TestNodePlatform CreateNodePlatform(
            bool dynamicInstallIsEnabled,
            bool sdkAlreadyInstalled)
        {
            var cliOptions = new BuildScriptGeneratorOptions();
            cliOptions.EnableDynamicInstall = dynamicInstallIsEnabled;
            var environment = new TestEnvironment();
            var installer = new TestNodePlatformInstaller(
                Options.Create(cliOptions),
                environment,
                sdkAlreadyInstalled);

            var versionProvider = new TestNodeVersionProvider();
            var nodeScriptGeneratorOptions = new NodeScriptGeneratorOptions();
            var detector = new TestNodePlatformDetector(
                versionProvider,
                Options.Create(nodeScriptGeneratorOptions),
                NullLogger<NodePlatformDetector>.Instance,
                environment,
                new TestStandardOutputWriter());

            return new TestNodePlatform(
                Options.Create(cliOptions),
                Options.Create(nodeScriptGeneratorOptions),
                versionProvider,
                NullLogger<NodePlatform>.Instance,
                detector,
                environment,
                installer);
        }

        private BuildScriptGeneratorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = sourceRepo,
                Properties = new Dictionary<string, string>(),
            };
        }

        private class TestNodePlatform : NodePlatform
        {
            public TestNodePlatform(
                IOptions<BuildScriptGeneratorOptions> cliOptions,
                IOptions<NodeScriptGeneratorOptions> nodeScriptGeneratorOptions,
                INodeVersionProvider nodeVersionProvider,
                ILogger<NodePlatform> logger,
                NodePlatformDetector detector,
                IEnvironment environment,
                NodePlatformInstaller nodePlatformInstaller)
                : base(
                      cliOptions,
                      nodeScriptGeneratorOptions,
                      nodeVersionProvider,
                      logger,
                      detector,
                      environment,
                      nodePlatformInstaller)
            {
            }
        }

        private class TestNodePlatformInstaller : NodePlatformInstaller
        {
            private readonly bool _sdkIsAlreadyInstalled;
            public static string InstallerScript = "installer-script-snippet";

            public TestNodePlatformInstaller(
                IOptions<BuildScriptGeneratorOptions> cliOptions,
                IEnvironment environment,
                bool sdkIsAlreadyInstalled)
                : base(cliOptions, environment)
            {
                _sdkIsAlreadyInstalled = sdkIsAlreadyInstalled;
            }

            public override bool IsVersionAlreadyInstalled(string version)
            {
                return _sdkIsAlreadyInstalled;
            }

            public override string GetInstallerScriptSnippet(string version)
            {
                return InstallerScript;
            }
        }

        private class TestNodePlatformDetector : NodePlatformDetector
        {
            public TestNodePlatformDetector(
                INodeVersionProvider nodeVersionProvider,
                IOptions<NodeScriptGeneratorOptions> nodeScriptGeneratorOptions,
                ILogger<NodePlatformDetector> logger,
                IEnvironment environment,
                IStandardOutputWriter writer)
                : base(nodeVersionProvider, nodeScriptGeneratorOptions, logger, environment, writer)
            {
            }
        }

        private class TestNodeVersionProvider : INodeVersionProvider
        {
            public PlatformVersionInfo GetVersionInfo()
            {
                throw new System.NotImplementedException();
            }
        }

        private class TestStandardOutputWriter : IStandardOutputWriter
        {
            public void Write(string message)
            {
            }

            public void WriteLine(string message)
            {
            }
        }
    }
}
